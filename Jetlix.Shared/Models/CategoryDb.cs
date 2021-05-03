using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jetlix.Shared.Models
{
    public class CategoryDb
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Uri { get; set; }
        public ICollection<ArticleCategoryRef> ArticleCategoryRefs { get; set; }
    }
}
