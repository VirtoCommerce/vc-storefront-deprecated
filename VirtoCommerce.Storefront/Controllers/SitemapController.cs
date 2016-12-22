using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.LiquidThemeEngine;
using VirtoCommerce.Storefront.AutoRestClients.SitemapsModuleApi;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Services;

namespace VirtoCommerce.Storefront.Controllers
{
    public class SitemapController : StorefrontControllerBase
    {
        private readonly ILiquidThemeEngine _themeEngine;
        private readonly ISitemapsModuleApiClient _siteMapApiClient;

        public SitemapController(WorkContext context, IStorefrontUrlBuilder urlBuilder, ISitemapsModuleApiClient siteMapApiClient, ILiquidThemeEngine themeEngine)
            : base(context, urlBuilder)
        {
            _themeEngine = themeEngine;
            _siteMapApiClient = siteMapApiClient;
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
            var stream = _themeEngine.GetAssetStream("sitemap.xml");
            if(stream == null)
            {
                var path = Server.MapPath("~/" + filePath);
                if (System.IO.File.Exists(path))
                {
                    stream = System.IO.File.OpenRead(path);
                }
            }
            if(stream == null)
            {
                var storeUrl = UrlBuilder.ToAppAbsolute("~/", WorkContext.CurrentStore, WorkContext.CurrentLanguage);
                //remove language from base url SitemapAPI will add it automatically
                storeUrl = storeUrl.Replace("/" + WorkContext.CurrentLanguage.CultureName + "/", "/");
                stream = await _siteMapApiClient.SitemapsModuleApiOperations.GenerateSitemapAsync(WorkContext.CurrentStore.Id, storeUrl, filePath);
            }
            return stream;
        }

    }
}
