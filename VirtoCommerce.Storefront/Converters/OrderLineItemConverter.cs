using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Order;
using orderModel = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class OrderLineItemConverter
    {
        public static LineItem ToWebModel(this orderModel.LineItem lineItem, IEnumerable<Currency> availCurrencies, Language language)
        {
        
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(lineItem.Currency)) ?? new Currency(language, lineItem.Currency);

            var retVal = new LineItem(currency);
            retVal.InjectFrom<NullableAndEnumValueInjecter>(lineItem);

            retVal.Currency = currency;
            retVal.DiscountAmount = new Money(lineItem.DiscountAmount ?? 0, currency);

            if (lineItem.DynamicProperties != null)
            {
                retVal.DynamicProperties = lineItem.DynamicProperties.Select(dp => dp.ToWebModel()).ToList();
            }
            retVal.Price = new Money(lineItem.Price ?? 0, currency);
            retVal.PriceWithTax = new Money(lineItem.PriceWithTax ?? 0, currency);
            retVal.DiscountAmount = new Money(lineItem.DiscountAmount ?? 0, currency);
            retVal.DiscountAmountWithTax = new Money(lineItem.DiscountAmountWithTax ?? 0, currency);
            retVal.PlacedPrice = new Money(lineItem.PlacedPrice ?? 0, currency);
            retVal.PlacedPriceWithTax = new Money(lineItem.PlacedPriceWithTax ?? 0, currency);
            retVal.ExtendedPrice = new Money(lineItem.ExtendedPrice ?? 0, currency);
            retVal.ExtendedPriceWithTax = new Money(lineItem.ExtendedPriceWithTax ?? 0, currency);
            retVal.DiscountTotal = new Money(lineItem.DiscountTotal ?? 0, currency);
            retVal.DiscountTotalWithTax = new Money(lineItem.DiscountTotalWithTax ?? 0, currency);
            retVal.Tax = new Money(lineItem.Tax ?? 0, currency);

            if (lineItem.TaxDetails != null)
            {
                retVal.TaxDetails = lineItem.TaxDetails.Select(td => td.ToWebModel(currency)).ToList();
            }

            return retVal;
        }
    }
}
