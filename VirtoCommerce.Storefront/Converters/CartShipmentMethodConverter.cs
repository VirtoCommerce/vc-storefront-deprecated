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
        public static ShippingMethod ToWebModel(this cartModel.ShippingRate shippingRate, Currency currency, IEnumerable<Currency> availCurrencies)
        {
            var rateCurrency = availCurrencies.FirstOrDefault(x => x.Equals(shippingRate.Currency)) ?? new Currency(new Language(currency.CultureName), shippingRate.Currency);
            var ratePrice = new Money(shippingRate.Rate ?? 0, rateCurrency);
            var ratePriceWithTax = new Money(shippingRate.RateWithTax ?? 0, rateCurrency);
            var rateDiscount = new Money(shippingRate.DiscountAmount ?? 0, rateCurrency);
            var rateDiscountWithTax = new Money(shippingRate.DiscountAmountWithTax ?? 0, rateCurrency);
            if (rateCurrency != currency)
            {
                ratePrice = ratePrice.ConvertTo(currency);
                ratePriceWithTax = ratePriceWithTax.ConvertTo(currency);
                rateDiscount = rateDiscount.ConvertTo(currency);
                rateDiscountWithTax = rateDiscountWithTax.ConvertTo(currency);
            }
            var retVal = new ShippingMethod(currency);
            retVal.InjectFrom(shippingRate);

            retVal.Price = ratePrice;
            retVal.PriceWithTax = ratePriceWithTax;
            retVal.DiscountTotal = rateDiscount;
            retVal.DiscountTotalWithTax = rateDiscountWithTax;

            if(shippingRate.ShippingMethod != null)
            {
                retVal.InjectFrom(shippingRate.ShippingMethod);
                retVal.ShipmentMethodCode = shippingRate.ShippingMethod.Code;
            }
            return retVal;
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
