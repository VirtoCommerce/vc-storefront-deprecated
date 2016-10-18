using Microsoft.Practices.ServiceLocation;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.LiquidThemeEngine.Objects.Factories;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class TierPriceConverter
    {
        public static TierPrice ToShopifyModel(this Storefront.Model.TierPrice tierPrice)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidTierPrice(tierPrice);
        }
    }

    public partial class ShopifyModelConverter
    {
        public virtual TierPrice ToLiquidTierPrice(Storefront.Model.TierPrice tierPrice)
        {
            var factory = ServiceLocator.Current.GetInstance<ShopifyModelFactory>();
            var result = factory.CreateTierPrice();
            result.Price = tierPrice.Price.Amount * 100;
            result.Quantity = tierPrice.Quantity;
            return result;
        }
    }
}