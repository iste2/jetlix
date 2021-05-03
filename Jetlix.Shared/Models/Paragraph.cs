using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Jetlix.Shared.Models
{
    public class Paragraph
    {
        public string text { get; set; }
        public ICollection<string> references { get; set; } = new Collection<string>();

        // TODO List of sources

        public string ToString()
        {
            return $"Paragraph - { text.Substring(0, Math.Min(text.Length, 15)) }...";
        }

    }
}
