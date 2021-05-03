using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Jetlix.Shared.Models
{
    public static class Types
    {
        public static string PARAGRAPH = "paragraph";
        public static string QUOTE = "quote";
        public static string FIGURE = "figure";
        public static string HEADLINE = "headline";
    }
    
    public class ArticleElement
    {
        [JsonIgnore]
        public string type { get; set; }
        public Headline headline { get; set; }
        public Figure figure { get; set; }
        public Quote quote { get; set; }
        public Paragraph paragraph { get; set; }

    }
}
