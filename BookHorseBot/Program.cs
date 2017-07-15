using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using BookHorseBot.Models;
using Newtonsoft.Json;
using RedditSharp;
using RedditSharp.Things;
using static BookHorseBot.Models.Misc;

namespace BookHorseBot
{
    class Program
    {
        private static readonly HttpClient BotClient = new HttpClient();

        static void Main()
        {
            Console.Title = "BookHorseBot";
            List<string> ignoredUsers = new List<string> {"nightmirrormoon"};

            if (string.IsNullOrEmpty(Properties.Settings.Default.FF_Token))
            {
                string receiveStream = FimFictionGetAuthToken();
                FfAuthorization authorization = JsonConvert.DeserializeObject<FfAuthorization>(receiveStream);
                Properties.Settings.Default.FF_Token = authorization.access_token;
                Properties.Settings.Default.Save();
            }

            BotClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            BotClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                Properties.Settings.Default.FF_Token);
            BotWebAgent webAgent = new BotWebAgent(Properties.Settings.Default.R_Username,
                Properties.Settings.Default.R_Password,
                Properties.Settings.Default.R_client_id,
                Properties.Settings.Default.R_client_secret,
                "https://google.com");

            Reddit reddit = new Reddit(webAgent, true);
            reddit.LogIn(Properties.Settings.Default.R_Username, Properties.Settings.Default.R_Password);
            string redditName = reddit.User.FullName;
            if (redditName.ToLower() == Properties.Settings.Default.R_Username.ToLower())
            {
                Console.WriteLine("Logged in!");
            }

            Subreddit subreddit = reddit.GetSubreddit("bronyvillers");


            IEnumerable<Comment> comments =
                subreddit.CommentStream.Where(c => !ignoredUsers.Contains(c.AuthorName.ToLower())
                                                   && c.CreatedUTC >= DateTime.UtcNow.AddMinutes(-95)
                );

            foreach (Comment comment in comments)
            {
                Comment qualifiedComment = reddit.GetComment(new Uri(comment.Shortlink));
                if (qualifiedComment.Comments.All(x => x.AuthorName != redditName))
                {
                    MatchCollection matches = Regex.Matches(comment.Body, @"\{.*\}", RegexOptions.None);
                    List<string> list =
                        matches.Cast<Match>()
                            .Select(match => match.Value.Replace("{", "").Replace("}", "").Trim())
                            .ToList();
                    if (list.Count > 0)
                    {
                        string sanitizedName = list.First();
                        sanitizedName = Regex.Replace(sanitizedName, @"[^a-zA-Z0-9 -]", "");
                        Story.Rootobject story = GetPostText(sanitizedName);
                        string postReplyBody = GeneratePostBody(story);
                        comment.Reply(postReplyBody);
                        string about = story.data.Length == 0
                            ? "Twilight being a failure."
                            : story.data.First().attributes.title;
                        Console.WriteLine($"Reply posted to {comment.AuthorName} about {about}!");
                    }
                }
            }
        }

        private static string GeneratePostBody(Story.Rootobject root)
        {
            
            string template;
            if (root.data.Length == 0)
            {
                template = "[](/twisad) \r\n [I'm sorry, I looked everywhere but I couldn't find that fanfic...](https://www.youtube.com/watch?v=BmRAGl1BOiQ)";
            }
            else
            {
                Story.Datum story = root.data.First();
                if (story.attributes.content_rating == "mature")
                {
                    template = "[](/twirage) \r\n  Please don't link mature rated fanfics in this sub! It isn't allowed!";
                }
                else
                {
                    template = "[](/twibeam) \r\n" +
                         $"#[{story.attributes.title}]({story.meta.url})\r\n" +
                         $"*by [{GetUsername(root)}](https://www.fimfiction.net/user/{story.relationships.author.data.id}) | {story.attributes.date_published:dd MMM yy} | Views: {FormatNumber(story.attributes.total_num_views)} | {FormatNumber(story.attributes.num_words)} Words | Status: `{UppercaseFirst(story.attributes.completion_status)}` | Rating: `{(double)story.attributes.rating}%`*\r\n\r\n" +
                         $"{GenerateDescription(story.attributes.description)}" +
                         "\r\n\r\n" +
                         $"**Tags**: {GenerateTags(root)}";
                }
            }
            template += 
                     "\r\n \r\n" +
                     "---" +
                     "\r\n \r\n" + 
                     "This is a bot | [Report problems](/message/compose/?to=BitzLeon&subject=Bookhorsebot running BHB 0.0.2) ";
            return template;
        }

        private static string GenerateDescription(string description)
        {
            if (description.Length > 255) {
                description = description.Substring(0, 255);  //.Split(new[] { "\r\n" }, StringSplitOptions.None).First();
            }
            if (description.Contains("[url=") && !description.Contains("[/url]"))
            {
                //If we stripped part of the body and didn't keep the closing tag for this.
                description = Regex.Replace(description, @"\[url=(.+?)\]", "");
            }
            //Bold
            description = Regex.Replace(description, @"\[b\]((?:.|\n)+?)\[\/b\]", match => "**" + match.Groups[1] + "**", RegexOptions.Multiline & RegexOptions.IgnoreCase);
            //Italics
            description = Regex.Replace(description, @"\[i\]((?:.|\n)+?)\[\/i\]", match => "*" + match.Groups[1] + "*", RegexOptions.Multiline & RegexOptions.IgnoreCase);
            //Strike-through
            description = Regex.Replace(description, @"\[s\]((?:.|\n)+?)\[\/s\]", match => "~~" + match.Groups[1] + "~~", RegexOptions.Multiline & RegexOptions.IgnoreCase);
            //URL
            description = Regex.Replace(description, @"\[url=(.+?)\]((?:.|\n)+?)\[\/url\]", match => $"[{match.Groups[2]}]({match.Groups[1]})", RegexOptions.Multiline & RegexOptions.IgnoreCase);

            return description;
        }

        private static string GetUsername(Story.Rootobject s)
        {
            string authorId = s.data.First().relationships.author.data.id;

            var authorName = s.included.First(x => x.id == authorId && x.type == "user").attributes.name;
            return authorName;
        }

        private static string GenerateTags(Story.Rootobject relationshipsTags)
        {
            List<string> tagIds = relationshipsTags.data.First().relationships.tags.data.Select(datum1 => datum1.id).ToList();
            List<string> tagNames = new List<string>();
            foreach (string tagId in tagIds)
            {
                string tagName = relationshipsTags.included.First(x => x.id == tagId && x.type == "story_tag").attributes.name;
                tagNames.Add(tagName);
            }

            string builtTagLineContent = string.Join("`, `", tagNames);
            builtTagLineContent = "`" + builtTagLineContent + "`";
            return builtTagLineContent;
        }

        private static Story.Rootobject GetPostText(string sanitizedName)
        {
            string r = BotClient.GetStringAsync($"https://www.fimfiction.net/api/v2/stories?include=characters,tags,author&sort=-relevance&query={sanitizedName}&page[size]=2").Result;
            Story.Rootobject searchResult = JsonConvert.DeserializeObject<Story.Rootobject>(r);
            return searchResult;
        }

        private static string FimFictionGetAuthToken()
        {
            var values = new Dictionary<string, string>
            {
                {"client_id", Properties.Settings.Default.FF_client_id},
                {"client_secret", Properties.Settings.Default.FF_client_secret},
                {"grant_type", "client_credentials"}
            };

            HttpContent content = new FormUrlEncodedContent(values);
            HttpResponseMessage response = BotClient.PostAsync("https://www.fimfiction.net/api/v2/token", content).Result;
            string receiveStream = response.Content.ReadAsStringAsync().Result;
            return receiveStream;
        }


        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        private static string FormatNumber(long num)
        {
            long i = (long)Math.Pow(10, (int)Math.Max(0, Math.Log10(num) - 2));
            num = num / i * i;

            if (num >= 1000000000)
                return (num / 1000000000D).ToString("0.##") + "B";
            if (num >= 1000000)
                return (num / 1000000D).ToString("0.##") + "M";
            if (num >= 1000)
                return (num / 1000D).ToString("0.##") + "K";

            return num.ToString("#,0");
        }
    }
}
