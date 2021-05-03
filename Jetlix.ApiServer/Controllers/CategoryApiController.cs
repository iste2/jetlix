using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jetlix.Shared.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Jetlix.ApiServer.Controllers
{
    [Route(Const.CATEGORY_ROUTE)]
    [ApiController]
    public class CategoryApiController : ControllerBase
    {

        public CategoryApiController()
        {
            var hContext = new LiteDbContext();
        }

        // GET: api/v1/categories
        [HttpGet]
        public async Task<List<Category>> GetCategories(bool includeParents = true)
        {
            using (var db = new LiteDatabase(LiteDbContext.dbName))
            {
                return db.GetCollection<Category>(Tables.CATEGORIES)
                    .FindAll()
                    .Select(_ => _.CategoryWithApiUrl(Request).CategoryWithParents(Request).CategoryWithoutId())
                    .Select(_ => _.CategoryIncludeParameters(includeParents))
                    .ToList();
            }
        }

        // GET
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Article>>> GetCategories(int id, bool includeParents = true)
        {
            using (var db = new LiteDatabase(LiteDbContext.dbName))
            {
                return Ok(db.GetCollection<Category>(Tables.CATEGORIES)
                    .FindById(id)
                        .CategoryWithApiUrl(Request)
                        .CategoryWithParents(Request)
                        .CategoryWithoutId()
                        .CategoryIncludeParameters(includeParents));
            }
        }

        // GET
        [HttpGet("{id}/articles")]
        public async Task<ActionResult<Article>> GetArticles(int id)
        {
            using (var db = new LiteDatabase(LiteDbContext.dbName))
            {
                // Find category
                var hCategory = db.GetCollection<Category>(Tables.CATEGORIES)
                    .FindById(id);


                if (hCategory != null)
                {
                    var hCatId = hCategory.id;

                    var hAllArticles = db.GetCollection<Article>(Tables.ARTICLES)
                        .FindAll()
                        .Where(_ => _.parentCategories.Select(_ => _.id).Any(_1 => hCatId == _1))
                        .ToList();
                    
                    return Ok(hAllArticles[id]);
                }

                return Ok();
            }
            
        }
    }
}
