using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Common;
using cartModel = VirtoCommerce.Storefront.AutoRestClients.CartModuleApi.Models;
using orderModel = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class OrderShipmentConverter
    {       

        public static Model.Order.Shipment ToWebModel(this orderModel.Shipment shipment, ICollection<Currency> availCurrencies, Language language)
        {

            var currency = availCurrencies.FirstOrDefault(x => x.Equals(shipment.Currency)) ?? new Currency(language, shipment.Currency);
            var retVal = new Model.Order.Shipment(currency);

            retVal.InjectFrom<NullableAndEnumValueInjecter>(shipment);

            retVal.Currency = currency;

            if (shipment.DeliveryAddress != null)
            {
                retVal.DeliveryAddress = shipment.DeliveryAddress.ToWebModel();
            }

            if (!shipment.Discounts.IsNullOrEmpty())
            {
                retVal.Discounts.AddRange(shipment.Discounts.Select(x => x.ToWebModel(new[] { currency }, language)));
            }

            if (shipment.DynamicProperties != null)
            {
                retVal.DynamicProperties = shipment.DynamicProperties.Select(dp => dp.ToWebModel()).ToList();
            }

            if (shipment.InPayments != null)
            {
                retVal.InPayments = shipment.InPayments.Select(p => p.ToWebModel(availCurrencies, language)).ToList();
            }

            if (shipment.Items != null)
            {
                retVal.Items = shipment.Items.Select(i => i.ToWebModel(availCurrencies, language)).ToList();
            }

            if (shipment.Packages != null)
            {
                retVal.Packages = shipment.Packages.Select(p => p.ToWebModel(availCurrencies, language)).ToList();
            }

            retVal.Price = new Money(shipment.Price ?? 0, currency);
            retVal.PriceWithTax = new Money(shipment.PriceWithTax ?? 0, currency);
            retVal.DiscountAmount = new Money(shipment.DiscountAmount ?? 0, currency);
            retVal.DiscountAmountWithTax = new Money(shipment.DiscountAmountWithTax ?? 0, currency);
            retVal.Total = new Money(shipment.Total ?? 0, currency);
            retVal.TotalWithTax = new Money(shipment.TotalWithTax ?? 0, currency);
            retVal.TaxTotal = new Money(shipment.TaxTotal ?? 0, currency);
            retVal.TaxPercentRate = (decimal?)shipment.TaxPercentRate ?? 0m;
            if (shipment.TaxDetails != null)
            {
                retVal.TaxDetails = shipment.TaxDetails.Select(td => td.ToWebModel(currency)).ToList();
            }
            return retVal;
        }

    }
}
