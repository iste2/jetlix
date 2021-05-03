using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Jetlix.Shared.Models
{
    public class InfoboxGroup
    {
        public string title { get; set; }
        public ICollection<InfoboxItem> infoboxItems { get; set; } = new Collection<InfoboxItem>();

        public string ToString()
        {
            return $"{ title }";
        }
    }
}
