using Microsoft.Practices.ServiceLocation;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.LiquidThemeEngine.Objects.Factories;
using VirtoCommerce.Storefront.Model.Order;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class ShippingMethodConverter
    {
        public static ShippingMethod ToShopifyModel(this Shipment shipment)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidShippingMethod(shipment);
        }

        public static ShippingMethod ToShopifyModel(this Storefront.Model.ShippingMethod shippingMethod)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidShippingMethod(shippingMethod);
        }
    }

    public partial class ShopifyModelConverter
    {
        public virtual ShippingMethod ToLiquidShippingMethod(Shipment shipment)
        {
            var factory = ServiceLocator.Current.GetInstance<ShopifyModelFactory>();
            var result = factory.CreateShippingMethod();
            result.Price = shipment.Total.Amount * 100;
            result.PriceWithTax = shipment.TotalWithTax.Amount * 100;
            result.Title = shipment.ShipmentMethodCode;
            result.Handle = shipment.ShipmentMethodCode;

            return result;
        }

        public virtual ShippingMethod ToLiquidShippingMethod(Storefront.Model.ShippingMethod shippingMethod)
        {
            var factory = ServiceLocator.Current.GetInstance<ShopifyModelFactory>();
            var result = factory.CreateShippingMethod();

            result.Handle = shippingMethod.ShipmentMethodCode;
            result.Price = shippingMethod.Price.Amount;
            result.TaxType = shippingMethod.TaxType;
            result.Title = shippingMethod.Name;

            return result;
        }
    }
}