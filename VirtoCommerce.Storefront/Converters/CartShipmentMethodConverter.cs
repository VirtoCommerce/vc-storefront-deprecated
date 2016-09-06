using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Common;
using cartModel = VirtoCommerce.Storefront.AutoRestClients.CartModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CartShipmentMethodConverter
    {
        public static ShippingMethod ToWebModel(this cartModel.ShippingMethod shippingMethod, Currency currency, IEnumerable<Currency> availCurrencies)
        {
            var shippingMethodCurrency = availCurrencies.FirstOrDefault(x => x.Equals(shippingMethod.Currency)) ?? new Currency(new Language(currency.CultureName), shippingMethod.Currency);
            var shipppingMethodPrice = new Money(shippingMethod.Price ?? 0, shippingMethodCurrency);
            if (shippingMethodCurrency != currency)
            {
                shipppingMethodPrice = shipppingMethodPrice.ConvertTo(currency);
            }
            var shippingMethodModel = new ShippingMethod(currency);
            shippingMethodModel.InjectFrom(shippingMethod);

            shippingMethodModel.Price = shipppingMethodPrice;

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
