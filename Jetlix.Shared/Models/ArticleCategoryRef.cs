using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jetlix.Shared.Models
{
    public class ArticleCategoryRef
    {

        public long Id { get; set; }
        public long articleId { get; set; }
        public ArticleDb article { get; set; }
        public long categoryId { get; set; }
        public CategoryDb category { get; set; }

    }
}
