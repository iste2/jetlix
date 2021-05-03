using Abot2.Crawler;
using Abot2.Poco;
using Jetlix.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace JetlixWebCrawlerBased
{
    enum CrawlerMode
    {
        DB, FILE
    }
    
    class Crawler
    {

        public ConcurrentQueue<string> FAllUris { get; set; }
        public ConcurrentQueue<string> FCategoryUris { get; set; }
        public ConcurrentQueue<string> FArticleUris { get; set; }
        public Stopwatch FWatch { get; set; }
        public string FBaseUri { get; set; }
        public string FAllUrisPath { get; set; }
        public string FCategoryUrisPath { get; set; }
        public string FArticleUrisPath { get; set; }
        public CrawlerMode FCrawlerMode { get; set; }
        public HttpClient FClient { get; set; }

        public static Semaphore WriteLock = new Semaphore(1, 1);

        public Crawler()
        {
            FAllUris = new ConcurrentQueue<string>();
            FCategoryUris = new ConcurrentQueue<string>();
            FArticleUris = new ConcurrentQueue<string>();
            FWatch = new Stopwatch();
            FBaseUri = "https://starwars.fandom.com/";
            FClient = new HttpClient();
        }

        public async Task CrawlAsync(string _AllUris_Path = "", string _CategoryUris_Path = "", string _ArticleUris_Path = "")
        {
            if(_AllUris_Path == "" && _CategoryUris_Path == "" && _ArticleUris_Path == "")
            {
                FCrawlerMode = CrawlerMode.DB;
            } else
            {
                FAllUrisPath = _AllUris_Path;
                FCategoryUrisPath = _CategoryUris_Path;
                FArticleUrisPath = _ArticleUris_Path;
                FCrawlerMode = CrawlerMode.FILE;
            }

            // Crawler configuration
            var hConfig = new CrawlConfiguration
            {
                MaxPagesToCrawl = 10,
            };

            // Crawler
            var hCrawler = new PoliteWebCrawler(hConfig);
            hCrawler.PageCrawlCompleted += HCrawler_PageCrawlCompletedAsync;

            // Decision makers
            hCrawler.ShouldCrawlPageDecisionMaker = (_PageToCrawl, _CrawlContext) =>
            {

                var hDecision = new CrawlDecision { Allow = true, };

                if (_PageToCrawl.Uri.ToString() != FBaseUri)
                {
                    if (_PageToCrawl.Uri.ToString().Split("/")[3] != "wiki"
                    || _PageToCrawl.Uri.ToString().Contains("?"))
                    {
                        hDecision = new CrawlDecision { Allow = false, Reason = "Don't crawl foreign languages." };
                    }
                }

                return hDecision;

            };

            // Crawl
            Console.WriteLine("Start crawling...\n");

            FWatch = new Stopwatch();
            FWatch.Start();

            var hCrawlResult = await hCrawler.CrawlAsync(new Uri(FBaseUri));

            FWatch.Stop();

            Console.WriteLine($"\nCrawling { hCrawlResult.CrawlContext.CrawledCount } pages took { FWatch.ElapsedMilliseconds } ms.");

            // Write to files/db
            
            if(FCrawlerMode == CrawlerMode.FILE)
            {
                Console.WriteLine("\nStart writing...\n");

                await File.WriteAllLinesAsync(FCategoryUrisPath, FCategoryUris);
                await File.AppendAllLinesAsync(FAllUrisPath, FAllUris);
                await File.AppendAllLinesAsync(FArticleUrisPath, FArticleUris);
            } else
            {
                
            }

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();

        }

        private async void HCrawler_PageCrawlCompletedAsync(object sender, PageCrawlCompletedArgs e)
        {
            var hUri = e.CrawledPage.Uri.ToString();
            //Console.WriteLine($"Crawling { hUri }");

            if (hUri.Split("/").Length >= 5 && hUri.Split("/")[4].Split(":")[0] == "Category")
            {
                await WriteCategoryToDb(hUri);
            } else if(hUri.Split("/").Length >= 5 
                && hUri.Split("/")[4].Split(":")[0] != "File"
                && hUri.Split("/")[4].Split(":")[0] != "Talk"
                && hUri.Split("/")[4].Split(":")[0] != "User"
                && hUri.Split("/")[4].Split(":")[0] != "Wookieepedia"
                && hUri.Split("/")[4].Split(":")[0] != "Special"
                && hUri.Split("/")[4].Split(":")[0] != "Template"
                )
            {
                await WriteArticleToDb(hUri, e.CrawledPage.Content.Text);
            }

        }

        public async Task WriteArticleToDb(string _Uri, string _Html = "")
        {
            

            var hArticle = await ArticleParser.ParseArticleAsync(_Uri, _Html);

            // Write categories first
            foreach(var hCategory in hArticle.parentCategories)
            {
                await WriteCategoryToDb(hCategory.wookiepediaUrl);
            }

            WriteLock.WaitOne();

            hArticle.parentCategories = (await FClient.GetAsync("https://localhost:5001/api/Categories").Result.Content.ReadAsAsync<Collection<Category>>())
                .Where(_ => hArticle.parentCategories.Any(_1 => _1.title == _.title)).ToList();

            Console.WriteLine($"Writing article \"{ hArticle.title }\" to db.");
            var hResult = await FClient.PostAsJsonAsync("https://localhost:5001/api/Articles", hArticle);
            WriteLock.Release(1);
            
        }

        public async Task WriteCategoryToDb(string _Uri)
        {
                     
            var hCategory = CategoryParser.ParseCategory(_Uri, true, true, true, true);

            if (hCategory.title == "Browse")
            {
                hCategory.parentCategories = new List<Category>();
            }

            // Write parent categories first
            foreach (var hParent in hCategory.parentCategories)
            {
                await WriteCategoryToDb(hParent.wookiepediaUrl);
            }

            WriteLock.WaitOne();

            hCategory.parentCategories = (await FClient.GetAsync("https://localhost:5001/api/Categories").Result.Content.ReadAsAsync<Collection<Category>>())
                .Where(_ => hCategory.parentCategories.Any(_1 => _1.title == _.title)).ToList();

            Console.WriteLine($"Writing category \"{ hCategory.title }\" to db.");
            var hResult = await FClient.PostAsJsonAsync("https://localhost:5001/api/Categories", hCategory);
            WriteLock.Release(1);

        }

        public async Task WriteArticleCategoryRefToDb(ArticleDb _ArticleDb, CategoryDb _CategoryDb)
        {
            var hArticleCategoryRef = new ArticleCategoryRef
            {
                articleId = _ArticleDb.Id,
                categoryId = _CategoryDb.Id,
            };
            var hResult = await FClient.PostAsJsonAsync("https://localhost:5001/api/ArticleCategoryRefs", hArticleCategoryRef);
            if (hResult.IsSuccessStatusCode)
            {
                Console.WriteLine($"        ArticleCategoryRef \"{ _ArticleDb.Title }\" --> \"{ _CategoryDb.Title }\" written to DB");
            }
        }

        public async Task WriteCategoryParentRefToDb(CategoryDb _Child, CategoryDb _Parent)
        {
            var hCategoryParentRef = new CategoryParentRef
            {
                CategoryChildId = _Child.Id,
                CategoryParentId = _Parent.Id,
            };
            var hResult = await FClient.PostAsJsonAsync("https://localhost:5001/api/CategoryParentRefs", hCategoryParentRef);
            if (hResult.IsSuccessStatusCode)
            {
                Console.WriteLine($"        CategoryParentRef \"{ _Child.Title }\" --> \"{ _Parent.Title }\" written to DB");
            }
        }

    }
}
