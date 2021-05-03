using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jetlix.Shared.Models
{
    public class Article
    {
        public string apiUrl { get; set; }
        public string wookiepediaUrl { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public ICollection<Category> parentCategories { get; set; } = new Collection<Category>();
        public ICollection<string> categories { get; set; }
        public Infobox infobox { get; set; }
        public ICollection<Figure> figures { get; set; } = new Collection<Figure>();
        public ICollection<Quote> quotes { get; set; } = new Collection<Quote>();
        public ICollection<string> references { get; set; } = new Collection<string>();
        public ICollection<ArticleElement> article { get; set; } = new Collection<ArticleElement>();


        public string ToString()
        {
            return $"{ title } - { wookiepediaUrl }";
        }


        public static string BuildApiUrl(HttpRequest _Request)
        {
            var hScheme = _Request.Host.Host.Contains("localhost") ? _Request.Scheme : "https";
            //var hApiUrl = $"{ hScheme }://{ _Request.Host }{ _Request.Path }";
            var hApiUrl = $"{ hScheme }://{ _Request.Host }/api/v1/articles";
            return hApiUrl;
        }
        public Article ArticleWithApiUrl(HttpRequest _Request)
        {
            if (int.TryParse(BuildApiUrl(_Request).Split('/')[^1], out int n) && n == id)
            {
                apiUrl = BuildApiUrl(_Request);
            }
            else
            {
                apiUrl = $"{BuildApiUrl(_Request)}/{id}";
            }
            return this;
        }

        public Article ArticleWithCategories(HttpRequest _Request)
        {
            categories = new Collection<string>();

            foreach(var hCategory in parentCategories)
            {
                var category = hCategory.CategoryWithApiUrl(_Request);
                categories.Add(category.apiUrl);
            }

            parentCategories = null;

            return this;
        }

        public Article ArticleIncludeParameters(bool includeCategories = true, bool includeInfobox = true, 
            bool includeFigures = true, bool includeQuotes = true, bool includeReferences = true, bool includeArticle = true)
        {
            if(!includeCategories)
            {
                categories = null;
            }
            if(!includeInfobox)
            {
                infobox = null;
            }
            if (!includeFigures)
            {
                figures = null;
            }
            if (!includeQuotes)
            {
                quotes = null;
            }
            if (!includeReferences)
            {
                references = null;
            }
            if (!includeArticle)
            {
                article = null;
            }
            return this;

        }


    }
}
