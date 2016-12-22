using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Optimization;

namespace VirtoCommerce.Storefront
{
    public class CssUrlTransform : IItemTransform
    {
        private static readonly Regex _urlRegex = new Regex(@"url\(['""]?(?<url>[^)]+?)['""]?\)");

        public virtual string Process(string includedVirtualPath, string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            // Transform arguments of the CSS url() function
            return _urlRegex.Replace(input, match => "url(" + ValidateAndTransformUrl(match.Groups["url"].Value) + ")");
        }


        protected virtual string ValidateAndTransformUrl(string url)
        {
            var result = url;

            // Don't modify empty, absolute or data: URLs
            if (!string.IsNullOrWhiteSpace(url) && !url.StartsWith("/", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                result = TransformUrl(url);
            }

            return result;
        }

        protected virtual string TransformUrl(string url)
        {
            return VirtualPathUtility.ToAbsolute("~/themes/assets/" + url);
        }
    }
}
