using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CartShipmentMethodConverter
    {
        public static ShippingMethod ToWebModel(this CartModule.Client.Model.ShippingMethod shippingMethod, Currency currency)
        {
            var shippingMethodModel = new ShippingMethod(currency);
            shippingMethodModel.InjectFrom(shippingMethod);
            shippingMethodModel.Price = new Money(shippingMethod.Price ?? 0, currency);
            return shippingMethodModel;
        }

        public static Shipment ToShipmentModel(this ShippingMethod shippingMethod, Currency currency)
        {
            var shipmentWebModel = new Shipment(currency)
            {
                ShipmentMethodCode = shippingMethod.ShipmentMethodCode,
                ShippingPrice = shippingMethod.Price,
                TaxType = shippingMethod.TaxType
            };

            return shipmentWebModel;
        }
    }
}
