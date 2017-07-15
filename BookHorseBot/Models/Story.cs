using System;
using System.Diagnostics.CodeAnalysis;

namespace BookHorseBot.Models
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    class Story
    {

        public class Rootobject
        {
            public Datum[] data { get; set; }
            public Included[] included { get; set; }
            public Links links { get; set; }
            public string uri { get; set; }
            public string method { get; set; }
            public Debug debug { get; set; }
        }

        public class Links
        {
            public string first { get; set; }
            public string prev { get; set; }
            public string next { get; set; }
        }

        public class Debug
        {
            public string duration { get; set; }
        }

        public class Datum
        {
            public string id { get; set; }
            public string type { get; set; }
            public Attributes attributes { get; set; }
            public Relationships relationships { get; set; }
            public Links1 links { get; set; }
            public Meta meta { get; set; }
        }

        public class Attributes
        {
            public string title { get; set; }
            public string short_description { get; set; }
            public string description { get; set; }
            public string description_html { get; set; }
            public DateTime date_modified { get; set; }
            public DateTime date_published { get; set; }
            public bool published { get; set; }
            public Cover_Image cover_image { get; set; }
            public Color color { get; set; }
            public int num_views { get; set; }
            public int total_num_views { get; set; }
            public int num_words { get; set; }
            public int num_chapters { get; set; }
            public int num_comments { get; set; }
            public int rating { get; set; }
            public string status { get; set; }
            public string completion_status { get; set; }
            public string content_rating { get; set; }
            public int num_likes { get; set; }
            public int num_dislikes { get; set; }
        }

        public class Cover_Image
        {
            public string thumbnail { get; set; }
            public string medium { get; set; }
            public string large { get; set; }
            public string full { get; set; }
        }

        public class Color
        {
            public string hex { get; set; }
            public int[] rgb { get; set; }
        }

        public class Relationships
        {
            public Author author { get; set; }
            public Tags tags { get; set; }
        }

        public class Author
        {
            public Data data { get; set; }
        }

        public class Data
        {
            public string type { get; set; }
            public string id { get; set; }
        }

        public class Tags
        {
            public Datum1[] data { get; set; }
        }

        public class Datum1
        {
            public string type { get; set; }
            public string id { get; set; }
        }

        public class Links1
        {
            public string self { get; set; }
        }

        public class Meta
        {
            public string url { get; set; }
        }

        public class Included
        {
            public string id { get; set; }
            public string type { get; set; }
            public Attributes1 attributes { get; set; }
            public Meta1 meta { get; set; }
            public Links2 links { get; set; }
        }

        public class Attributes1
        {
            public string name { get; set; }
            public string type { get; set; }
            public int num_stories { get; set; }
            public string bio { get; set; }
            public string bio_html { get; set; }
            public int num_followers { get; set; }
            public int num_blog_posts { get; set; }
            public Avatar avatar { get; set; }
            public Color1 color { get; set; }
            public DateTime date_joined { get; set; }
        }

        public class Avatar
        {
            public string _16 { get; set; }
            public string _32 { get; set; }
            public string _48 { get; set; }
            public string _64 { get; set; }
            public string _96 { get; set; }
            public string _128 { get; set; }
            public string _192 { get; set; }
            public string _256 { get; set; }
            public string _384 { get; set; }
            public string _512 { get; set; }
        }

        public class Color1
        {
            public string hex { get; set; }
            public int[] rgb { get; set; }
        }

        public class Meta1
        {
            public string old_id { get; set; }
            public string url { get; set; }
        }

        public class Links2
        {
            public string self { get; set; }
        }


    }
}
