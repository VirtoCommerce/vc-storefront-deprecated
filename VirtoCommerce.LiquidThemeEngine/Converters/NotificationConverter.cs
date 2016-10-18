using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.LiquidThemeEngine.Objects.Factories;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class NotificationConverter
    {
        public static Notification ToShopifyModel(this StorefrontNotification notification)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidNotification(notification);
        }
    }
    public partial class ShopifyModelConverter
    {
        public virtual Notification ToLiquidNotification(StorefrontNotification notification)
        {
            var factory = ServiceLocator.Current.GetInstance<ShopifyModelFactory>();
            var result = factory.CreateNotification();
            result.InjectFrom<NullableAndEnumValueInjecter>(notification);
            return result;
        }
    }
}