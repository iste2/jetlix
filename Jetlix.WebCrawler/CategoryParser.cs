using HtmlAgilityPack;
using Jetlix.Shared.Models;
using System;
using System.Linq;

namespace JetlixWebCrawlerBased
{
    public class CategoryParser
    {

        public static Category ParseCategory(string _Link, bool _IncludeName = true, bool _IncludeLink = true, bool _IncludeDescription = true, bool _IncludeParents = true)
        {
            var hCategory = new Category();

            var hWeb = new HtmlWeb();
            var hDoc = hWeb.Load(_Link);

            // name, link, description
            if (_IncludeLink) hCategory.wookiepediaUrl = _Link;
            if (_IncludeName) hCategory.title = hDoc.GetElementbyId("firstHeading")?.InnerText;
            if (_IncludeDescription) hCategory.description = hDoc.DocumentNode.SelectSingleNode("//div[@class='mw-parser-output']/p")?.InnerText;
            

            // parent categories
            if (_IncludeParents)
            {
                try
                {
                    var hParents1 = hDoc.DocumentNode.SelectNodes("//div[@class='page-header__categories-links']/a")
                        .Select(_ => new Category
                        {
                            wookiepediaUrl = "https://starwars.fandom.com" + _.Attributes["href"].Value,
                            title = _.InnerText
                        })
                        .ToList();

                    foreach(var hParent in hParents1)
                    {
                        hCategory.parentCategories.Add(hParent);
                    }

                    var hParents2 = hDoc.DocumentNode.SelectNodes("//div[@class='page-header__categories-links']//ul[@class='wds-list wds-is-linked']/li/a")
                        .Select(_ => new Category
                        {
                            wookiepediaUrl = "https://starwars.fandom.com" + _.Attributes["href"].Value,
                            title = _.InnerText
                        })
                        .ToList();

                    foreach (var hParent in hParents2)
                    {
                        hCategory.parentCategories.Add(hParent);
                    }

                }
                catch (Exception e)
                {

                }
            }
            
            return hCategory;
        }

    }
}
