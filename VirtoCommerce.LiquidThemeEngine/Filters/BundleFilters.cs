using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Optimization;
using Microsoft.Practices.ServiceLocation;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.LiquidThemeEngine.Filters
{
    public class BundleFilters
    {
        private static readonly bool OptimizeStaticContent = ConfigurationManager.AppSettings.GetValue("VirtoCommerce:Storefront:OptimizeStaticContent", false);

        public static string ScriptBundleTag(string input)
        {
            var cacheManager = ServiceLocator.Current.GetInstance<ILocalCacheManager>();
            var retVal = cacheManager.Get("ScriptBundleTag:" + input, "LiquidThemeRegion", () =>
            {
                var result = string.Empty;
                var bundle = BundleTable.Bundles.GetBundleFor(input);
                if (bundle != null)
                {
                    if (OptimizeStaticContent)
                    {
                        var url = BundleTable.Bundles.ResolveBundleUrl(input);
                        result = HtmlFilters.ScriptTag(url);
                    }
                    else
                    {
                        var response = bundle.GenerateBundleResponse(new BundleContext(new HttpContextWrapper(HttpContext.Current), BundleTable.Bundles, string.Empty));
                        result = string.Join("\r\n", response.Files.Select(f => HtmlFilters.ScriptTag(UrlFilters.AssetUrl(f.IncludedVirtualPath.Split('/').Last()))));
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
                    if (OptimizeStaticContent)
                    {
                        var url = BundleTable.Bundles.ResolveBundleUrl(input);
                        result = HtmlFilters.StylesheetTag(url);
                    }
                    else
                    {
                        var response = bundle.GenerateBundleResponse(new BundleContext(new HttpContextWrapper(HttpContext.Current), BundleTable.Bundles, string.Empty));
                        result = string.Join("\r\n", response.Files.Select(f => HtmlFilters.StylesheetTag(UrlFilters.AssetUrl(f.IncludedVirtualPath.Split('/').Last()))));
                    }
                }
                return result;
            });

            return retVal;
        }
    }
}