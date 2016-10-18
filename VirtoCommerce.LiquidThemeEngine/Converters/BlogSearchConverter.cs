using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using PagedList;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.LiquidThemeEngine.Objects.Factories;
using VirtoCommerce.Storefront.Model.Common;
using StorefrontModel = VirtoCommerce.Storefront.Model.StaticContent;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class BlogSearchConverter
    {
        public static BlogSearch ToShopifyModel(this StorefrontModel.BlogSearchCriteria blogSearchCriteria)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidBlogSearch(blogSearchCriteria);
        }
    }

    public partial class ShopifyModelConverter
    {
        public virtual BlogSearch ToLiquidBlogSearch(StorefrontModel.BlogSearchCriteria blogSearchCriteria)
        {
            var factory = ServiceLocator.Current.GetInstance<ShopifyModelFactory>();
            var retVal = factory.CreateBlogSearch();
            retVal.InjectFrom<NullableAndEnumValueInjecter>(blogSearchCriteria);

            return retVal;
        }
    }
}