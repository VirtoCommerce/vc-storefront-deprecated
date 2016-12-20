using Omu.ValueInjecter;
using System.Linq;
using VirtoCommerce.LiquidThemeEngine.Objects;
using StorefrontModel = VirtoCommerce.Storefront.Model;
using Microsoft.Practices.ServiceLocation;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class PageConverter
    {
        public static Page ToShopifyModel(this StorefrontModel.StaticContent.ContentItem contentItem)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidPage(contentItem);
        }
    }

    public partial class ShopifyModelConverter
    {
        public virtual Page ToLiquidPage(StorefrontModel.StaticContent.ContentItem contentItem)
        {
            var result = new Page();
            result.InjectFrom<StorefrontModel.Common.NullableAndEnumValueInjecter>(contentItem);
            result.Handle = contentItem.Url;
            if (contentItem.PublishedDate.HasValue)
            {
                result.PublishedAt = contentItem.PublishedDate.Value;
            }
            return result;
        }
    }
}