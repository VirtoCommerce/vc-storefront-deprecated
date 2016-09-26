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
            var retVal = new Shipment(cart.Currency);

            retVal.InjectFrom(shipment);
            retVal.Currency = cart.Currency;
            retVal.Price = new Money(shipment.Price ?? 0, cart.Currency);
            retVal.DiscountAmount = new Money(shipment.DiscountAmount ?? 0, cart.Currency);
            retVal.TaxPercentRate = (decimal?)shipment.TaxPercentRate ?? 0m;

            if (shipment.DeliveryAddress != null)
            {
                retVal.DeliveryAddress = shipment.DeliveryAddress.ToWebModel();
            }

            if (shipment.Items != null)
            {
                retVal.Items = shipment.Items.Select(i => i.ToWebModel(cart)).ToList();
            }

            if (shipment.TaxDetails != null)
            {
                retVal.TaxDetails = shipment.TaxDetails.Select(td => td.ToWebModel(cart.Currency)).ToList();
            }

            if (!shipment.Discounts.IsNullOrEmpty())
            {
                retVal.Discounts.AddRange(shipment.Discounts.Select(x => x.ToWebModel(new[] { cart.Currency }, cart.Language)));
            }
            return retVal;
        }

        public static cartModel.Shipment ToServiceModel(this Shipment shipment)
        {
            var retVal = new cartModel.Shipment();

            retVal.InjectFrom(shipment);
            retVal.Currency = shipment.Currency != null ? shipment.Currency.Code : null;
            retVal.DiscountAmount = shipment.DiscountAmount != null ? (double?)shipment.DiscountAmount.Amount : null;
            retVal.Price = shipment.Price != null ? (double?)shipment.Price.Amount : null;
            retVal.TaxPercentRate = (double)shipment.TaxPercentRate;

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

    }
}
