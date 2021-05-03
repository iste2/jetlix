using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Jetlix.Shared.Models
{
    public class Infobox
    {

        public string title { get; set; } = "";
        public ICollection<InfoboxGroup> groups { get; set; } = new Collection<InfoboxGroup>();
        //public ICollection<InfoboxItem> FInfoboxItems { get; set; } = new Collection<InfoboxItem>();

        public string ToString()
        {
            return $"{ title }";
        }
    }
}
