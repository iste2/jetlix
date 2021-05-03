using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jetlix.Shared.Models
{
    public class Category
    {
        public string apiUrl { get; set; }
        public string wookiepediaUrl { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public ICollection<Category> parentCategories { get; set; } = new Collection<Category>();
        public ICollection<string> parents { get; set; }

        public string ToString()
        {
            return $"{ title }";
        }

        
        public static string BuildApiUrl(HttpRequest _Request)
        {
            var hScheme = _Request.Host.Host.Contains("localhost") ? _Request.Scheme : "https";
            //var hApiUrl = $"{ hScheme }://{ _Request.Host }{ _Request.Path }";
            var hApiUrl = $"{ hScheme }://{ _Request.Host }/api/v1/categories";
            return hApiUrl;
        }

        public Category CategoryWithApiUrl(HttpRequest _Request)
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

        public Category CategoryWithParents(HttpRequest _Request)
        {
            parents = new Collection<string>();
            foreach(var hParent in parentCategories)
            {
                var parent = hParent.CategoryWithApiUrl(_Request);
                parents.Add(parent.apiUrl);
            }

            parentCategories = null;

            return this;
        }

        public Category CategoryWithoutId()
        {
            //id = null;
            return this;
        }

        public Category CategoryIncludeParameters(bool includeParents = true)
        {
            if(!includeParents)
            {
                parentCategories = null;
                parents = null;
            }
            
            return this;
        }


    }
}
