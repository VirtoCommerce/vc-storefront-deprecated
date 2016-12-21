using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using Microsoft.Practices.ServiceLocation;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.LiquidThemeEngine.Filters
{
    public class BundleFilters
    {
        private static readonly bool _optimizeStaticContent = ConfigurationManager.AppSettings.GetValue("VirtoCommerce:Storefront:OptimizeStaticContent", false);

        public static string ScriptBundleTag(string input)
        {
            var cacheManager = ServiceLocator.Current.GetInstance<ILocalCacheManager>();
            var retVal = cacheManager.Get("ScriptBundleTag:" + input, "LiquidThemeRegion", () =>
            {
                var result = string.Empty;
                var bundle = BundleTable.Bundles.GetBundleFor(input);
                if (bundle != null)
                {
                    if (_optimizeStaticContent)
                    {
                        var url = BundleTable.Bundles.ResolveBundleUrl(input);
                        result = HtmlFilters.ScriptTag(url);
                    }
                    else
                    {
                        var response = bundle.GenerateBundleResponse(new BundleContext(new HttpContextWrapper(HttpContext.Current), BundleTable.Bundles, string.Empty));
                        result = string.Join("\r\n", response.Files.Select(f => HtmlFilters.ScriptTag(GetAssetUrl(f.IncludedVirtualPath))));
                    }
                }
                return result;
            });
            return retVal;
        }

        public static string StylesheetBundleTag(string input)
        {
            var cacheManager = ServiceLocator.Current.GetInstance<ILocalCacheManager>();
            var retVal = cacheManager.Get("StylesheetBundleTag:" + input, "LiquidThemeRegion", () =>
            {
                var result = string.Empty;
                var bundle = BundleTable.Bundles.GetBundleFor(input);
                if (bundle != null)
                {
                    if (_optimizeStaticContent)
                    {
                        var url = BundleTable.Bundles.ResolveBundleUrl(input);
                        result = HtmlFilters.StylesheetTag(url);
                    }
                    else
                    {
                        var response = bundle.GenerateBundleResponse(new BundleContext(new HttpContextWrapper(HttpContext.Current), BundleTable.Bundles, string.Empty));
                        result = string.Join("\r\n", response.Files.Select(f => HtmlFilters.StylesheetTag(GetAssetUrl(f.IncludedVirtualPath))));
                    }
                }
                return result;
            });

            return retVal;
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
