using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jetlix.Shared.Models
{
    public class CategoryParentRef
    {

        public long Id { get; set; }
        public long CategoryParentId { get; set; }
        public CategoryDb CategoryParent { get; set; }
        public long CategoryChildId { get; set; }
        public CategoryDb CategoryChild { get; set; }
    }
}
