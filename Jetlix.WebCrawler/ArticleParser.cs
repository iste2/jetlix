using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json;
using Jetlix.Shared.Models;
using System.Net;
using System.IO;
using System.Collections.ObjectModel;
using System.Drawing;

namespace JetlixWebCrawlerBased
{
    public class ArticleParser
    {
        public static async System.Threading.Tasks.Task<Article> ParseArticleAsync(string _Link, string _HtmlString = "", 
            bool _IncludeTitle = true, bool _IncludeLink = true, bool _IncludeInfobox = true, 
            bool _IncludeFigures = true, bool _IncludeQuotes = true, bool _IncludeReferences = true,
            bool _IncludeCategories = true, bool _IncludeArticle = true)
        {

            var hArticle = new Article();

            if(_HtmlString == "") _HtmlString = GetURLData(_Link);
            var hDoc = new HtmlDocument();
            hDoc.LoadHtml(_HtmlString);
            //var hWeb = new HtmlWeb();
            //var hDoc = hWeb.Load(_Link);

            // title
            if(_IncludeTitle) hArticle.title = hDoc.GetElementbyId("firstHeading")?.InnerText;

            // link
            if (_IncludeLink) hArticle.wookiepediaUrl = _Link;

            // figures
            if (_IncludeFigures) hArticle.figures = hDoc.DocumentNode.SelectNodes("//a[@class='image'] | //a[@class='image image-thumbnail']")?
                .Where(_ => _.ParentNode.Name != "noscript")?
                .Select(_ => new Figure
                {
                    url = ImageUrl(_.Attributes["href"].Value),
                    caption = _.ParentNode.InnerText?.Replace("\t", "").Replace("\n", "").Trim(),
                }).ToList();
            if (hArticle.figures == null) hArticle.figures = new Collection<Figure>();

            // quotes
            if (_IncludeQuotes) hArticle.quotes = hDoc.DocumentNode.SelectNodes("//div[@class='quote']")?
                .Select(_ => new Quote
                {
                    text = _.SelectSingleNode("./dl/dd")?.InnerText,
                    reference = _.SelectNodes("./dl/dd")[1]?.InnerText,
                    source = CompleteLink(_.SelectNodes("./dl/dd")[1]?.SelectSingleNode(".//sup/a")?.Attributes["href"]?.Value),
                }).ToList();
            if (hArticle.quotes == null) hArticle.quotes = new Collection<Quote>();

            // references
            if (_IncludeReferences) hArticle.references = hDoc.DocumentNode.SelectNodes("//a[@href]")?
                .Where(_ => (bool)(_.Attributes["href"]?.Value.StartsWith("/wiki/")))
                .Select(_ => CompleteLink(_.Attributes["href"]?.Value))
                .Where(_ => IsArticleUrl(_))
                .Distinct()
                .ToList();
            if (hArticle.references == null) hArticle.references = new Collection<string>();

            // categories
            if (_IncludeCategories)
            {
                hArticle.parentCategories = new List<Category>();
                var hParents1 = hDoc.DocumentNode.SelectNodes("//div[@class='page-header__categories-links']/a")?
                        .Select(_ => new Category
                        {
                            wookiepediaUrl = CompleteLink(_.Attributes["href"].Value),
                            title = _.InnerText
                        })
                        .ToList();

                if (hParents1 != null) ((List<Category>)hArticle.parentCategories).AddRange(hParents1);

                var hParents2 = hDoc.DocumentNode.SelectNodes("//div[@class='page-header__categories-links']//ul[@class='wds-list wds-is-linked']/li/a")?
                    .Select(_ => new Category
                    {
                        wookiepediaUrl = CompleteLink(_.Attributes["href"].Value),
                        title = _.InnerText
                    })
                    .ToList();

                if (hParents2 != null) ((List<Category>)hArticle.parentCategories).AddRange(hParents2);
            }
            if (hArticle.parentCategories == null) hArticle.parentCategories = new Collection<Category>();


            // article
            if (_IncludeArticle)
            {
                hArticle.article = new List<ArticleElement>();
                var hArticleElements = hDoc.DocumentNode.SelectSingleNode("//div[@class='mw-parser-output']")?
                    .SelectNodes("./p | ./div[@class='quote'] | ./*[substring-after(name(), 'h') > 0] | ./figure").ToList();

                if(hArticleElements != null && hArticleElements.Count() != 0)
                {
                    foreach (var hArticleElement in hArticleElements)
                    {
                        ArticleElement hElement = null;
                        if (hArticleElement.Name == "p")
                        {
                            var hParagraph = new Paragraph
                            {
                                text = hArticleElement.InnerText,
                                references = hArticleElement.SelectNodes("./a[@href]")?
                                    .Where(_ => (bool)(_.Attributes["href"]?.Value.StartsWith("/wiki/")))?
                                    .Select(_ => "https://starwars.fandom.com" + _.Attributes["href"]?.Value)?
                                    .Distinct()?
                                    .ToList(),
                            };
                            hElement = new ArticleElement
                            {
                                type = Types.PARAGRAPH,
                                paragraph = hParagraph,
                            };
                        }
                        else if (hArticleElement.Name.StartsWith("h"))
                        {
                            var hHeadline = new Headline
                            {
                                text = hArticleElement.InnerText.Split("[")[0],
                                level = int.Parse(hArticleElement.Name[1].ToString()),
                            };
                            hElement = new ArticleElement
                            {
                                type = Types.HEADLINE,
                                headline = hHeadline,
                            };
                        }
                        else if (hArticleElement.Name == "div")
                        {
                            var hQuote = new Quote
                            {
                                text = hArticleElement.SelectSingleNode("./dl/dd")?.InnerText,
                                reference = hArticleElement.SelectNodes("./dl/dd")[1]?.InnerText,
                                source = CompleteLink(hArticleElement.SelectNodes("./dl/dd")[1]?.SelectSingleNode(".//sup/a")?.Attributes["href"]?.Value),
                            };
                            hElement = new ArticleElement
                            {
                                type = Types.QUOTE,
                                quote = hQuote,
                            };
                        }
                        else if (hArticleElement.Name == "figure")
                        {
                            var hFigure = new Figure
                            {
                                url = ImageUrl(hArticleElement.SelectSingleNode("./a").Attributes["href"].Value),
                                caption = hArticleElement.SelectSingleNode("./a").ParentNode.InnerText?.Replace("\t", "").Replace("\n", "").Trim(),
                            };
                            hElement = new ArticleElement
                            {
                                type = Types.FIGURE,
                                figure = hFigure,
                            };
                        }
                        hArticle.article.Add(hElement);
                    }

                    // clear article
                    int i = hArticle.article.Count() - 1;
                    while (i >= 0)
                    {
                        var hArticleElement = hArticle.article.ToList()[i];
                        if (hArticleElement.type == Types.HEADLINE && i == hArticle.article.Count() - 1
                            || hArticleElement.type == Types.PARAGRAPH
                                && (hArticleElement.paragraph.text == "\n"
                                    || hArticleElement.paragraph.text == ""
                                    || hArticleElement.paragraph.text == null
                                    || !hArticleElement.paragraph.text.Any(_ => char.IsUpper(_))))
                        {
                            hArticle.article.Remove(hArticleElement);
                        }
                        i--;
                    }
                }

            }
            if (hArticle.article == null) hArticle.article = new Collection<ArticleElement>();

            // infobox
            if (_IncludeInfobox) hArticle.infobox = new Infobox
            {
                title = hDoc.DocumentNode.SelectSingleNode("//aside/h2")?.InnerText,

                groups = hDoc.DocumentNode.SelectSingleNode("//aside")?.SelectNodes("./section")?.Select(_ => new InfoboxGroup
                {
                    title = _.SelectSingleNode("./h2")?.InnerText,

                    infoboxItems = _.SelectNodes("./div")?.Select(_1 => new InfoboxItem
                    {
                        label = _1.SelectSingleNode("./h3")?.InnerText,

                        value = _1.SelectSingleNode("./div")?.InnerText,

                        references = _1.SelectNodes(".//a[@href]")?
                            .Where(_2 => _2.Attributes["href"].Value.Contains("/wiki/"))
                            .Select(_2 => CompleteLink(_2.Attributes["href"].Value)).ToList(),

                    }).ToList(),

                }).ToList(),
                

            };
            if (hArticle.infobox.title == null && hArticle.infobox.groups == null) hArticle.infobox = new Infobox();

            return hArticle;

        }

        public static string ParseArticleToJson(string _Link)
        {
            var hArticle = ParseArticleAsync(_Link);
            var hResult = JsonConvert.SerializeObject(hArticle, Formatting.Indented, new JsonSerializerSettings { 
                NullValueHandling = NullValueHandling.Ignore,
            });
            Console.WriteLine(hResult);
            return hResult;
        }

        private static string CompleteLink(string _Link)
        {
            if(_Link.StartsWith("/wiki/"))
            {
                return "https://starwars.fandom.com" + _Link;
            } else
            {
                return _Link;
            }
        }

        public static string ImageUrl(string _Url)
        {
            _Url = _Url.Split("revision")[0];
            if(_Url.Last() == '/')
            {
                return _Url.Remove(_Url.Length - 1);
            }
            return _Url;
        }

        public static string GetURLData(string URL)
        {
            try
            {
                var request = (HttpWebRequest)HttpWebRequest.Create(URL);
                request.UserAgent = "Omurcek";
                request.Timeout = 60 * 1000 * 10;
                WebResponse response = request.GetResponse();
                var stream = response.GetResponseStream();
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }

            catch (Exception ex)
            {
                
                return "";
            }

        }

        public static bool IsArticleUrl(string _Uri)
        {
            if (_Uri == null) return false;

            int hEntry = 4;
            if(!_Uri.StartsWith("https://starwars.fandom.com"))
            {
                return false;
            }
            
            return _Uri.Split("/").Length >= 5
                && _Uri.Split("/")[hEntry].Split(":")[0] != "File"
                && _Uri.Split("/")[hEntry].Split(":")[0] != "Talk"
                && _Uri.Split("/")[hEntry].Split(":")[0] != "User"
                && _Uri.Split("/")[hEntry].Split(":")[0] != "Category"
                && _Uri.Split("/")[hEntry].Split(":")[0] != "Wookieepedia"
                && _Uri.Split("/")[hEntry].Split(":")[0] != "Special"
                && _Uri.Split("/")[hEntry].Split(":")[0] != "Template";
        }


    }
}
