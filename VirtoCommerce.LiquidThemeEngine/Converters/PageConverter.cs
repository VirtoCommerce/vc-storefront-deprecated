using Omu.ValueInjecter;
using System.Linq;
using VirtoCommerce.LiquidThemeEngine.Objects;
using StorefrontModel = VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class PageConverter
    {
        public static Page ToShopifyModel(this StorefrontModel.StaticContent.ContentItem contentItem)
        {
            var retVal = new Page();
            retVal.InjectFrom<StorefrontModel.Common.NullableAndEnumValueInjecter>(contentItem);
            retVal.Handle = contentItem.Url;
            if (contentItem.PublishedDate.HasValue)
            {
                retVal.PublishedAt = contentItem.PublishedDate.Value;
            }
            return retVal;
        }
    }
}