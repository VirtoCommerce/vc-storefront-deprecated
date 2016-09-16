using System;
using System.Web;
using System.Linq;

namespace VirtoCommerce.LiquidThemeEngine.Extensions
{
    public static class UriExtensions
    {
        /// <summary>
        /// Sets the given parameter value in the query string.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="name">Name of the parameter to set.</param>
        /// <param name="value">Value for the parameter to set. Pass null to remove the parameter with given name.</param>
        /// <returns>Url with given parameter value.</returns>
        public static Uri SetQueryParameter(this Uri url, string name, string value)
        {
            var nameValues = HttpUtility.ParseQueryString(url.Query);

            if (value != null)
            {
                nameValues[name] = value;
            }
            else
            {
                nameValues.Remove(name);
            }
            var retVal = new UriBuilder(url);

            if (nameValues.HasKeys())
            {
                retVal.Query = string.Join("&", nameValues.AllKeys.Select(x => string.Format("{0}={1}", HttpUtility.UrlEncode(x), HttpUtility.UrlEncode(nameValues[x]))));
            }          

            return retVal.Uri;
        }
    }
}
