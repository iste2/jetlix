using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jetlix.Shared.Models;
using LiteDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Jetlix.ApiServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        public CategoriesController()
        {
            var hContext = new LiteDbContext();
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<List<Category>> GetCategories()
        {
            using (var db = new LiteDatabase(LiteDbContext.dbName))
            {
                return db.GetCollection<Category>(Tables.CATEGORIES)
                    .FindAll()
                    .Select(_ => _.CategoryWithApiUrl(Request))
                    .ToList();
            }
        }

        // GET: api/Articles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            using (var db = new LiteDatabase(LiteDbContext.dbName))
            {
                return db.GetCollection<Category>(Tables.CATEGORIES)
                    .FindById(id).CategoryWithApiUrl(Request);
            }
        }


        // POST: api/Categories
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<Category> PostCategories(Category category)
        {
            using (var db = new LiteDatabase(LiteDbContext.dbName))
            {
                if (db.GetCollection<Category>(Tables.CATEGORIES).FindOne(_ => _.title == category.title) == null)
                {
                    db.GetCollection<Category>(Tables.CATEGORIES).Insert(category);

                    
                }
                    

            }

            return category;
        }


    }
}
