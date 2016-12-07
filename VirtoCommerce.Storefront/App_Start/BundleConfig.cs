using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Optimization;

namespace VirtoCommerce.Storefront
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region JS

            bundles.Add(
                new ScriptBundle("~/default-theme/scripts")
                    .Include("~/App_Data/Themes/default/assets/modernizr.min.js")
                    .Include("~/App_Data/Themes/default/assets/ideal-image-slider.min.js")
                    .Include("~/App_Data/Themes/default/assets/ideal-image-slider-bullet-nav.js")
                    .Include("~/App_Data/Themes/default/assets/ideal-image-slider-captions.js")
                    .IncludeDirectory("~/App_Data/Themes/default/assets/js/", "*.js"));

            bundles.Add(
                new ScriptBundle("~/default-theme/checkout/scripts")
                    .Include("~/App_Data/Themes/default/assets/js/app.js")
                    .Include("~/App_Data/Themes/default/assets/js/services.js")
                    .Include("~/App_Data/Themes/default/assets/js/directives.js")
                    .Include("~/App_Data/Themes/default/assets/js/main.js")
                    .IncludeDirectory("~/App_Data/Themes/default/assets/js/checkout/", "*.js"));

            bundles.Add(
                new ScriptBundle("~/default-theme/account/scripts")
                    .Include("~/App_Data/Themes/default/assets/modernizr.min.js")
                    //.Include("~/App_Data/Themes/default/assets/ideal-image-slider.min.js")
                    //.Include("~/App_Data/Themes/default/assets/ideal-image-slider-bullet-nav.js")
                    //.Include("~/App_Data/Themes/default/assets/ideal-image-slider-captions.js")
                    .Include("~/App_Data/Themes/default/assets/js/app.js")
                    .Include("~/App_Data/Themes/default/assets/js/services.js")
                    .Include("~/App_Data/Themes/default/assets/js/main.js")
                    .Include("~/App_Data/Themes/default/assets/js/cart.js")
                    .Include("~/App_Data/Themes/default/assets/js/quote-request.js")
                    .Include("~/App_Data/Themes/default/assets/js/checkout/checkout-address.js")
                    .Include("~/App_Data/Themes/default/assets/js/checkout/checkout-paymentMethods.js")
                    .IncludeDirectory("~/App_Data/Themes/default/assets/js/account/", "*.js"));

            #endregion

            #region CSS

            bundles.Add(
                new StyleBundle("~/default-theme/css")
                    .Include("~/App_Data/Themes/default/assets/storefront.css", new CustomCssRewriteUrlTransform())
                    .Include("~/App_Data/Themes/default/assets/ideal-image-slider.css", new CustomCssRewriteUrlTransform())
                    .Include("~/App_Data/Themes/default/assets/ideal-image-slider-default-theme.css", new CustomCssRewriteUrlTransform()));

            #endregion
        }
    }

    public class CustomCssRewriteUrlTransform : IItemTransform
    {
        public string Process(string includedVirtualPath, string input)
        {
            return ConvertUrlsToAbsolute("~/themes/assets/", input);
        }


        private static string ConvertUrlsToAbsolute(string baseUrl, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return content;
            }

            // Replace all URLs with absolute URLs
            var url = new Regex(@"url\(['""]?(?<url>[^)]+?)['""]?\)");
            return url.Replace(content, match => "url(" + RebaseUrlToAbsolute(baseUrl, match.Groups["url"].Value) + ")");
        }

        private static string RebaseUrlToAbsolute(string baseUrl, string url)
        {
            // Don't modify absolute URLs
            if (string.IsNullOrWhiteSpace(url) || url.StartsWith("/", StringComparison.OrdinalIgnoreCase) || url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            return VirtualPathUtility.ToAbsolute(baseUrl + url);
        }
    }
}
