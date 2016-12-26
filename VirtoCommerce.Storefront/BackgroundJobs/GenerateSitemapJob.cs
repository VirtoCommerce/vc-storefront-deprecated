using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using VirtoCommerce.Storefront.AutoRestClients.SitemapsModuleApi;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Services;
using VirtoCommerce.Storefront.Model.Stores;
namespace VirtoCommerce.Storefront.BackgroundJobs
{
    /// <summary>
    /// type contains logic for pre-generation sitemaps files 
    /// This code might be run periodically in background process (as Hangfire job for example)
    /// </summary>
    public class GenerateSitemapJob
    {
        private readonly IContentBlobProvider _contentBlobProvider;
        private readonly ISitemapsModuleApiClient _siteMapApiClient;
        private readonly IStorefrontUrlBuilder _urlBuilder;
        public GenerateSitemapJob(IContentBlobProvider contentBlobProvider, ISitemapsModuleApiClient siteMapApiClient, IStorefrontUrlBuilder urlBuilder)
        {
            _contentBlobProvider = contentBlobProvider;
            _siteMapApiClient = siteMapApiClient;
            _urlBuilder = urlBuilder;
        }

        public void GenerateStoreSitemap(IEnumerable<Store> stores, Uri requestUri)
        {
            foreach (var store in stores)
            {
                var storeThemeName = string.IsNullOrEmpty(store.ThemeName) ? "default" : store.ThemeName;
                var storeThemePath = Path.Combine(store.Id, storeThemeName, "assets");

                var absUrl = _urlBuilder.ToAppAbsolute("~/", store, store.DefaultLanguage);
                var storeUrl = new Uri(requestUri, absUrl).ToString();
                var sitemapLocations = _siteMapApiClient.SitemapsModuleApiOperations.GetSitemapsSchema(store.Id);
                //remove language from base url SitemapAPI will add it automatically
                storeUrl = storeUrl.Replace("/" + store.DefaultLanguage.CultureName + "/", "/");
                //Download and save all sitemaps for each store in theme assets folder
                foreach (var sitemapLocation in sitemapLocations.Concat(new[] { "sitemap.xml" }))
                {
                    using (var sitemapStream = _siteMapApiClient.SitemapsModuleApiOperations.GenerateSitemap(store.Id, storeUrl, sitemapLocation))
                    using (var targetStream = _contentBlobProvider.OpenWrite(Path.Combine(storeThemePath, sitemapLocation)))
                    {
                        sitemapStream.CopyTo(targetStream);
                    }
                }
            }
        }
    }
}