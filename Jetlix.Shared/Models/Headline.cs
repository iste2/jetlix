using System;
using System.Collections.Generic;
using System.Text;

namespace Jetlix.Shared.Models
{
    public class Headline
    {
        public int level { get; set; }
        public string text { get; set; }


        public string ToString()
        {
            return $"Headline L{ level } - \"{ text }\"";
        }

    }
}
