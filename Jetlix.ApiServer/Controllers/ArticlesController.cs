using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Jetlix.ApiServer;
using Jetlix.Shared.Models;
using System.Linq;
using LiteDB;

namespace Jetlix.ApiServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {

        public ArticlesController()
        {
            var hContext = new LiteDbContext();
        }

        // GET: api/Articles
        [HttpGet]
        public async Task<IEnumerable<Article>> GetArticle()
        {
            using (var db = new LiteDatabase(LiteDbContext.dbName))
            {
                return db.GetCollection<Article>(Tables.ARTICLES).FindAll().ToList();
            }
        }
        /*
        // GET: api/Articles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Article>> GetArticle(int id)
        {
            var context = new LiteDbContext();
            var col = context.db.GetCollection<Article>(Tables.ARTICLES);
            var article = col.FindOne(_ => _.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            return article;
        }*/

        // POST: api/Articles
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<Article> PostArticle(Article article)
        {
            using (var db = new LiteDatabase(LiteDbContext.dbName))
            {
                if(db.GetCollection<Article>(Tables.ARTICLES).FindOne(_ => _.title == article.title) == null)
                {
                    db.GetCollection<Article>(Tables.ARTICLES).Insert(article);
                    
                    db.GetCollection<Article>(Tables.ARTICLES)
                    .EnsureIndex(_ => _.title);
                    db.GetCollection<Article>(Tables.ARTICLES)
                        .EnsureIndex(_ => _.wookiepediaUrl);
                }
                    
            }

            return article;
        }

        /*
        // PUT: api/Articles/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle(int id, Article article)
        {
            if (id != article.Id)
            {
                return BadRequest();
            }

            _context.Entry(article).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        */


        /*
        // DELETE: api/Articles/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Article>> DeleteArticle(int id)
        {
            var article = await _context.Article.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            _context.Article.Remove(article);
            await _context.SaveChangesAsync();

            return article;
        }

        private bool ArticleExists(int id)
        {
            return _context.Article.Any(e => e.Id == id);
        }
        */
    }
}
