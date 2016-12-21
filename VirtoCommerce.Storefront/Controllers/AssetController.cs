using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.LiquidThemeEngine;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Services;

namespace VirtoCommerce.Storefront.Controllers
{
    [OutputCache(CacheProfile = "AssetsCachingProfile")]
    public class AssetController : StorefrontControllerBase
    {
        private readonly ILiquidThemeEngine _themeEngine;
        private readonly IStaticContentBlobProvider _staticContentBlobProvider;

        public AssetController(WorkContext context, IStorefrontUrlBuilder urlBuilder, ILiquidThemeEngine themeEngine, IStaticContentBlobProvider staticContentBlobProvider)
            : base(context, urlBuilder)
        {
            _themeEngine = themeEngine;
            _staticContentBlobProvider = staticContentBlobProvider;
        }

        /// <summary>
        /// GET: /themes/localization.json
        /// Return localization resources for current theme
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetThemeLocalizationJson()
        {
            var retVal = _themeEngine.ReadLocalization();
            return Json(retVal, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// GET: /themes/assets/{*asset}
        /// Handle theme assets requests
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetThemeAssets(string path)
        {
            var stream = _themeEngine.GetAssetStream(path);
            return stream != null
                ? File(stream, MimeMapping.GetMimeMapping(path))
                : HandleStaticFiles(path);
        }

        /// <summary>
        /// GET: /themes/global/assets/{*asset}
        /// Handle global theme assets requests
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetGlobalThemeAssets(string path)
        {
            var stream = _themeEngine.GetAssetStream(path, searchInGlobalThemeOnly: true);
            if (stream != null)
            {
                return File(stream, MimeMapping.GetMimeMapping(path));
            }
            throw new HttpException(404, path);
        }

        /// <summary>
        /// GET: /assets/{*asset}
        /// Handle all static content assets requests
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetStaticContentAssets(string path)
        {
            var blobPath = _staticContentBlobProvider.Search(Path.Combine(WorkContext.CurrentStore.Id, "assets"), path, true).FirstOrDefault();
            if (!string.IsNullOrEmpty(blobPath))
            {
                var stream = _staticContentBlobProvider.OpenRead(blobPath);
                if (stream != null)
                {
                    return File(stream, MimeMapping.GetMimeMapping(blobPath));
                }
            }

            throw new HttpException(404, path);
        }

        /// <summary>
        /// Serve static files. This controller called from SeoRoute when it cannot find any other routes for request.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ActionResult HandleStaticFiles(string path)
        {
            path = Server.MapPath("~/" + path);
            var mimeType = MimeMapping.GetMimeMapping(path);
            if (System.IO.File.Exists(path) && mimeType != "application/octet-stream")
            {
                return File(path, MimeMapping.GetMimeMapping(path));
            }
            throw new HttpException(404, path);
        }
    }
}
