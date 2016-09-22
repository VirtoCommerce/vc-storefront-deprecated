using System.Linq;
using Omu.ValueInjecter;
using PagedList;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.Storefront.Model.Common;
using StorefrontModel = VirtoCommerce.Storefront.Model.StaticContent;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class BlogSearchCriteriaConverter
    {
        public static BlogSearchCriteria ToShopifyModel(this StorefrontModel.BlogSearchCriteria blogSearchCriteria)
        {
            var retVal = new BlogSearchCriteria();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(blogSearchCriteria);
       
            return retVal;
        }
    }
}