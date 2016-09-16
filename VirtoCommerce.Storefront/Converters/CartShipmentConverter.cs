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
    public static class CartShipmentConverter
    {
        public static Shipment ToWebModel(this cartModel.Shipment shipment, ShoppingCart cart)
        {
            var webModel = new Shipment(cart.Currency);

            webModel.InjectFrom(shipment);
            webModel.Currency = cart.Currency;
            webModel.ShippingPrice = new Money(shipment.ShippingPrice ?? 0, cart.Currency);
            webModel.ShippingPriceWithTax = new Money(shipment.ShippingPriceWithTax ?? 0, cart.Currency);
            webModel.DiscountTotal = new Money(shipment.DiscountTotal ?? 0, cart.Currency);
            webModel.DiscountTotalWithTax = new Money(shipment.DiscountTotalWithTax ?? 0, cart.Currency);

            if (shipment.DeliveryAddress != null)
            {
                webModel.DeliveryAddress = shipment.DeliveryAddress.ToWebModel();
            }

            if (shipment.Items != null)
            {
                webModel.Items = shipment.Items.Select(i => i.ToWebModel(cart)).ToList();
            }

            if (shipment.TaxDetails != null)
            {
                webModel.TaxDetails = shipment.TaxDetails.Select(td => td.ToWebModel(cart.Currency)).ToList();
            }

            if (!shipment.Discounts.IsNullOrEmpty())
            {
                webModel.Discounts.AddRange(shipment.Discounts.Select(x => x.ToWebModel(new[] { cart.Currency }, cart.Language)));
            }
            return webModel;
        }

        public static cartModel.Shipment ToServiceModel(this Shipment shipment)
        {
            var retVal = new cartModel.Shipment();

            retVal.InjectFrom(shipment);
            retVal.Currency = shipment.Currency != null ? shipment.Currency.Code : null;
            retVal.DiscountTotal = shipment.DiscountTotal != null ? (double?)shipment.DiscountTotal.Amount : null;
            retVal.DiscountTotalWithTax = shipment.DiscountTotalWithTax != null ? (double?)shipment.DiscountTotalWithTax.Amount : null;
            retVal.ShippingPrice = shipment.ShippingPrice != null ? (double?)shipment.ShippingPrice.Amount : null;
            retVal.ShippingPriceWithTax = shipment.ShippingPriceWithTax != null ? (double?)shipment.ShippingPriceWithTax.Amount : null;

            if (shipment.DeliveryAddress != null)
            {
                retVal.DeliveryAddress = shipment.DeliveryAddress.ToCartServiceModel();
            }

            if (shipment.Discounts != null)
            {
                retVal.Discounts = shipment.Discounts.Select(d => d.ToServiceModel()).ToList();
            }

            if (shipment.Items != null)
            {
                retVal.Items = shipment.Items.Select(i => i.ToServiceModel()).ToList();
            }

            if (shipment.TaxDetails != null)
            {
                retVal.TaxDetails = shipment.TaxDetails.Select(td => td.ToCartApiModel()).ToList();
            }

            return retVal;
        }

        public static Model.Order.Shipment ToWebModel(this orderModel.Shipment shipment, ICollection<Currency> availCurrencies, Language language)
        {
         
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(shipment.Currency)) ?? new Currency(language, shipment.Currency);
            var retVal = new Model.Order.Shipment(currency);

            retVal.InjectFrom(shipment);

            retVal.Currency = currency;

            if (shipment.DeliveryAddress != null)
            {
                retVal.DeliveryAddress = shipment.DeliveryAddress.ToWebModel();
            }

            if (shipment.Discount != null)
            {
                retVal.Discount = shipment.Discount.ToWebModel(availCurrencies, language);
            }

            retVal.DiscountAmount = new Money(shipment.DiscountAmount ?? 0, currency);

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
            retVal.ItemsSubtotal = new Money(shipment.ItemsSubtotal ?? 0, currency);
            retVal.ItemsSubtotalWithTax = new Money(shipment.ItemsSubtotalWithTax ?? 0, currency);

            if (shipment.TaxDetails != null)
            {
                retVal.TaxDetails = shipment.TaxDetails.Select(td => td.ToWebModel(currency)).ToList();
            }        
            return retVal;
        }

    

    }
}
