using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jetlix.Shared.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;

namespace Jetlix.ApiServer.Controllers
{
    [Route(Const.ARTICLE_ROUTE)]
    [ApiController]
    public class ArticleApiController : ControllerBase
    {

        // GET: api/v1/articles
        [HttpGet]
        public async Task<List<Article>> GetArticles(bool includeCategories = true, bool includeInfobox = true,
            bool includeFigures = true, bool includeQuotes = true, bool includeReferences = true, bool includeArticle = true)
        {
            
            using (var db = new LiteDatabase(LiteDbContext.dbName))
            {
                return db.GetCollection<Article>(Tables.ARTICLES)
                    .FindAll()
                    .Select(_ => _.ArticleWithApiUrl(Request).ArticleWithCategories(Request)
                        .ArticleIncludeParameters(includeCategories, includeInfobox, includeFigures, includeQuotes, includeReferences, includeArticle))
                    .ToList();
            }
        }

        // GET
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticles(int id, bool includeCategories = true, bool includeInfobox = true,
            bool includeFigures = true, bool includeQuotes = true, bool includeReferences = true, bool includeArticle = true)
        {
            using (var db = new LiteDatabase(LiteDbContext.dbName))
            {
                return Ok(db.GetCollection<Article>(Tables.ARTICLES)
                    .FindById(id).ArticleWithApiUrl(Request).ArticleWithCategories(Request)
                    .ArticleIncludeParameters(includeCategories, includeInfobox, includeFigures, includeQuotes, includeReferences, includeArticle));
            }
        }

    }
}
