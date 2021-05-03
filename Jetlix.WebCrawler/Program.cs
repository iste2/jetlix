using System.Threading.Tasks;

namespace JetlixWebCrawlerBased
{
    class Program
    {

        static async Task Main(string[] args)
        {
            //ArticleParser.ParseArticleToJson("https://starwars.fandom.com/wiki/Ric_Oli%C3%A9");
            var hCrawler = new Crawler();
            //await hCrawler.CrawlAsync(@"C:\Users\Lukas\Desktop\AllUris", @"C:\Users\Lukas\Desktop\CategoryUris", @"C:\Users\Lukas\Desktop\ArticleUris");
            await hCrawler.CrawlAsync();
            //await hCrawler.WriteArticleToDb("https://starwars.fandom.com/wiki/Ric_Oli%C3%A9");
        }

    }
}
