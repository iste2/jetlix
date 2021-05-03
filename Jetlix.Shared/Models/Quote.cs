using System;
using System.Collections.Generic;
using System.Text;

namespace Jetlix.Shared.Models
{
    public class Quote
    {
        public string text { get; set; }
        public string reference { get; set; }
        public string source { get; set; }


        public string ToString()
        {
            return $"Quote - { text.Substring(0, 15) }...";
        }
    }
}
