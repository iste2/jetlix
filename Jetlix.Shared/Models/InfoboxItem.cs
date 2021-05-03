using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Jetlix.Shared.Models
{
    public class InfoboxItem
    {
        public string label { get; set; }
        public string value { get; set; }
        public ICollection<string> references { get; set; } = new Collection<string>();
        //public List<string> FSources { get; set; }

        public string ToString()
        {
            return $"{ label } - { value.Substring(0, Math.Min(value.Length, 15)) }...";
        }
    }
}
