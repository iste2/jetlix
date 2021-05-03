using System;
using System.Collections.Generic;
using System.Text;

namespace Jetlix.Shared.Models
{
    public class Figure
    {
        public string url { get; set; }
        public string caption { get; set; }

        public string ToString()
        {
            return $"Figure - { url }";
        }
    }
}
