using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Order;
using orderModel = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class OrderConverter
    {
        public static CustomerOrder ToWebModel(this orderModel.CustomerOrder order, ICollection<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(order.Currency)) ?? new Currency(language, order.Currency);

            var retVal = new CustomerOrder(currency);

            retVal.InjectFrom<NullableAndEnumValueInjecter>(order);

            if (order.Addresses != null)
            {
                retVal.Addresses = order.Addresses.Select(a => a.ToWebModel()).ToList();
            }

           
            retVal.Currency = currency;

          

            if (order.DynamicProperties != null)
            {
                retVal.DynamicProperties = order.DynamicProperties.Select(dp => dp.ToWebModel()).ToList();
            }

            if (order.InPayments != null)
            {
                retVal.InPayments = order.InPayments.Select(p => p.ToWebModel(availCurrencies, language)).ToList();
            }

            if (order.Items != null)
            {
                retVal.Items = order.Items.Select(i => i.ToWebModel(availCurrencies, language)).ToList();
            }

            if (order.Shipments != null)
            {
                retVal.Shipments = order.Shipments.Select(s => s.ToWebModel(availCurrencies, language)).ToList();
            }

            if (!order.Discounts.IsNullOrEmpty())
            {
                retVal.Discounts.AddRange(order.Discounts.Select(x => x.ToWebModel(new[] { currency }, language)));
            }
            if (order.TaxDetails != null)
            {
                retVal.TaxDetails = order.TaxDetails.Select(td => td.ToWebModel(currency)).ToList();
            }

            retVal.DiscountAmount = new Money(order.DiscountAmount ?? 0, currency);
            retVal.Total = new Money(order.Total ?? 0, currency);
            retVal.SubTotal = new Money(order.SubTotal ?? 0, currency);
            retVal.SubTotalWithTax = new Money(order.SubTotalWithTax ?? 0, currency);
            retVal.TaxTotal = new Money(order.TaxTotal ?? 0, currency);
            retVal.ShippingTotal = new Money(order.ShippingTotal ?? 0, currency);
            retVal.ShippingTotalWithTax = new Money(order.ShippingTotalWithTax ?? 0, currency);
            retVal.ShippingTaxTotal = new Money(order.ShippingTaxTotal ?? 0, currency);
            retVal.SubTotalTaxTotal = new Money(order.SubTotalTaxTotal ?? 0, currency);
            retVal.SubTotalDiscount = new Money(order.SubTotalDiscount ?? 0, currency);
            retVal.SubTotalDiscountWithTax = new Money(order.SubTotalDiscountWithTax ?? 0, currency);


            return retVal;
        }
    }
}
