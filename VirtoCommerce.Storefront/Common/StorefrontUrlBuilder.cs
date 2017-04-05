using System.Web;
using System.Web.Hosting;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Stores;
using VirtoCommerce.Tools;
using toolsDto = VirtoCommerce.Tools.Models;

namespace VirtoCommerce.Storefront.Common
{
    /// <summary>
    /// Create storefront url contains language and store information
    /// </summary>
    public class StorefrontUrlBuilder : IStorefrontUrlBuilder
    {
        private readonly IUrlBuilder _urlBuilder;
        private readonly WorkContext _workContext;
        private readonly toolsDto.UrlBuilderContext _urlBuilderContext;

        public StorefrontUrlBuilder(IUrlBuilder urlBuilder, WorkContext workContext)
        {
            _urlBuilder = urlBuilder;
            _workContext = workContext;
            _urlBuilderContext = workContext.ToToolsContext();
        }

        #region IStorefrontUrlBuilder members

        public string ToAppAbsolute(string virtualPath)
        {
            return ToAppAbsolute(virtualPath, _workContext.CurrentStore, _workContext.CurrentLanguage);
        }

        public string ToAppAbsolute(string virtualPath, Store store, Language language)
        {
            var appRelativePath = ToAppRelative(virtualPath, store, language);
            var result = appRelativePath != null && appRelativePath.StartsWith("~")
                ? VirtualPathUtility.ToAbsolute(appRelativePath)
                : appRelativePath;
            return result;
        }

        public string ToAppRelative(string virtualPath)
        {
            return ToAppRelative(virtualPath, _workContext.CurrentStore, _workContext.CurrentLanguage);
        }

        public string ToAppRelative(string virtualPath, Store store, Language language)
        {
            return _urlBuilder.BuildStoreUrl(_urlBuilderContext, virtualPath, store.ToToolsStore(), language?.CultureName);
        }

        public string ToLocalPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }
        #endregion
    }
}
