using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using DotLiquid;
using Microsoft.Practices.ServiceLocation;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.LiquidThemeEngine.Filters
{
    public class BundleFilters
    {
        private static readonly bool _optimizeStaticContent = ConfigurationManager.AppSettings.GetValue("VirtoCommerce:Storefront:OptimizeStaticContent", false);

        public static string ScriptBundleTag(string input)
        {
            return GetBundleTag(HtmlFilters.ScriptTag, input);
        }

        public static string StylesheetBundleTag(string input)
        {
            return GetBundleTag(HtmlFilters.StylesheetTag, input);
        }


        private static string GetBundleTag(Func<string, string> tagFunc, string bundleName)
        {
            var storeId = GetCurrentStoreId();
            var bundleType = tagFunc.Method.Name;
            var cacheKey = string.Join(":", "Bundle", storeId, bundleType, bundleName);
            var cacheManager = ServiceLocator.Current.GetInstance<ILocalCacheManager>();

            var retVal = cacheManager.Get(cacheKey, "LiquidThemeRegion", () =>
            {
                var result = string.Empty;

                var bundle = BundleTable.Bundles.GetBundleFor(bundleName);

                if (bundle != null)
                {
                    if (_optimizeStaticContent)
                    {
                        var url = BundleTable.Bundles.ResolveBundleUrl(bundleName);
                        result = tagFunc(url);
                    }
                    else
                    {
                        var response = bundle.GenerateBundleResponse(new BundleContext(new HttpContextWrapper(HttpContext.Current), BundleTable.Bundles, string.Empty));
                        result = string.Join("\r\n", response.Files.Select(f => tagFunc(GetAssetUrl(f.IncludedVirtualPath))));
                    }
                }

                return result;
            });

            return retVal;
        }

        private static string GetCurrentStoreId()
        {
            var themeEngine = (ShopifyLiquidThemeEngine)Template.FileSystem;
            var workContext = themeEngine.WorkContext;
            return workContext.CurrentStore.Id;
        }

        private static string GetAssetUrl(string virtualPath)
        {
            var assetName = virtualPath.Split('/').Last();
            var isStaticAsset = virtualPath.IndexOf("/static/", StringComparison.OrdinalIgnoreCase) >= 0;

            var result = isStaticAsset
                ? UrlFilters.StaticAssetUrl(assetName)
                : UrlFilters.AssetUrl(assetName);

            return result;
        }
    }
}
