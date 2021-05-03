using Jetlix.Shared.Models;
using LiteDB;

namespace Jetlix.ApiServer
{

    public static class Tables
    {
        public static string ARTICLES = "articles";
        public static string CATEGORIES = "categories";
    }

    public class LiteDbContext
    {

        //public LiteDatabase db;
        public static string dbName = "jetlixlite.db";

        public ILiteCollection<Article> Articles { get; set; }
        public ILiteCollection<Category> Categories { get; set; }

        public LiteDbContext()
        {
            using (var db = new LiteDatabase(dbName))
            {
                Articles = db.GetCollection<Article>(Tables.ARTICLES);
                Categories = db.GetCollection<Category>(Tables.CATEGORIES);

                // Indexes
                db.GetCollection<Article>(Tables.ARTICLES)
                    .EnsureIndex(_ => _.title);
                db.GetCollection<Article>(Tables.ARTICLES)
                    .EnsureIndex(_ => _.wookiepediaUrl);

                db.GetCollection<Category>(Tables.CATEGORIES)
                    .EnsureIndex(_ => _.title);
                db.GetCollection<Category>(Tables.CATEGORIES)
                    .EnsureIndex(_ => _.wookiepediaUrl);

                
            }

            // Mapping
            BsonMapper.Global.Entity<Article>()
                .DbRef(_ => _.parentCategories, Tables.CATEGORIES);

            BsonMapper.Global.Entity<Category>()
                .DbRef(_ => _.parentCategories, Tables.CATEGORIES);

        }


    }
}
