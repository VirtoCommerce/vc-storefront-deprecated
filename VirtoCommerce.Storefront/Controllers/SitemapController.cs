using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using VirtoCommerce.LiquidThemeEngine;
using VirtoCommerce.Storefront.AutoRestClients.SitemapsModuleApi;
using VirtoCommerce.Storefront.BackgroundJobs;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Services;

namespace VirtoCommerce.Storefront.Controllers
{
    public class SitemapController : StorefrontControllerBase
    {
        private readonly ISitemapsModuleApiClient _siteMapApiClient;
        private readonly ILiquidThemeEngine _liquidThemeEngine;
        private readonly GenerateSitemapJob _sitemapJob;

        public SitemapController(WorkContext context, IStorefrontUrlBuilder urlBuilder, ISitemapsModuleApiClient siteMapApiClient,
                                 ILiquidThemeEngine themeEngine, GenerateSitemapJob sitemapJob)
            : base(context, urlBuilder)
        {
            _liquidThemeEngine = themeEngine;
            _siteMapApiClient = siteMapApiClient;
            _sitemapJob = sitemapJob;
        }

        /// <summary>
        /// GET: /sitemap.xml
        /// Return generated sitemap index sitemap.xml
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetSitemapIndex()
        {
            var stream = await TryGetSitemapStream("sitemap.xml");
            if(stream != null)
            {
                return File(stream, "text/xml");
            }       
            throw new HttpException(404, "sitemap.xml");
        }

        /// <summary>
        /// GET: /sitemap/sitemap-1.xml
        /// Return generated sitemap by file
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> GetSitemap(string sitemapPath)
        {
            var stream = await TryGetSitemapStream("sitemap/" + sitemapPath);
            if (stream != null)
            {
                return File(stream, "text/xml");
            }
            throw new HttpException(404, sitemapPath);
        }


        private async Task<Stream> TryGetSitemapStream(string filePath)
        {
            //If sitemap files have big size for generation on the fly you might place already generated xml files in the theme/assets folder or schedule 
            // execution of GenerateSitemapJob.GenerateStoreSitemap method for pre-generation sitemaps  
            var stream = _liquidThemeEngine.GetAssetStream(filePath);          
            if(stream == null)
            {                
                var absUrl = UrlBuilder.ToAppAbsolute("~/", WorkContext.CurrentStore, WorkContext.CurrentLanguage);
                var storeUrl = new Uri(WorkContext.RequestUrl, absUrl).ToString(); 
                //remove language from base url SitemapAPI will add it automatically
                storeUrl = storeUrl.Replace("/" + WorkContext.CurrentLanguage.CultureName + "/", "/");
                stream = await _siteMapApiClient.SitemapsModuleApiOperations.GenerateSitemapAsync(WorkContext.CurrentStore.Id, storeUrl, filePath);
            }
            return stream;
        }

    }
}
