using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Tax;
using serviceModel = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class TaxEvaluationContextConverter
    {
        public static serviceModel.TaxEvaluationContext ToServiceModel(this TaxEvaluationContext taxContext)
        {
            var retVal = new serviceModel.TaxEvaluationContext();
            retVal.InjectFrom<NullableAndEnumValueInjecter>(taxContext);
            if (taxContext.Address != null)
            {
                retVal.Address = taxContext.Address.ToCoreServiceModel();
            }
            if (taxContext.Customer != null)
            {
                retVal.Customer = taxContext.Customer.ToCoreServiceModel();
            }
            if (taxContext.Currency != null)
            {
                retVal.Currency = taxContext.Currency.Code;
            }

            retVal.Lines = new List<serviceModel.TaxLine>();
            if (!taxContext.Lines.IsNullOrEmpty())
            {
                foreach(var line in taxContext.Lines)
                {
                    var serviceModelLine = new serviceModel.TaxLine();
                    serviceModelLine.InjectFrom<NullableAndEnumValueInjecter>(line);
                    serviceModelLine.Amount = (double)line.Amount.Amount;
                    serviceModelLine.Price = (double)line.Price.Amount;

                    retVal.Lines.Add(serviceModelLine);
                }
            }
            return retVal;
        }

        public static TaxLine[] ToTaxLines(this ShippingMethod shipmentMethod)
        {
            var retVal = new List<TaxLine>
            {
                new TaxLine(shipmentMethod.Currency)
                {
                    Id = string.Join("&", shipmentMethod.ShipmentMethodCode, shipmentMethod.OptionName),
                    Code = shipmentMethod.ShipmentMethodCode,
                    TaxType = shipmentMethod.TaxType,
                    Amount = shipmentMethod.Total
                }
            };
            return retVal.ToArray();
        }

        public static TaxLine[] ToTaxLines(this Product product)
        {
            var retVal = new List<TaxLine>
            {
                new TaxLine(product.Currency)
                {
                    Id = product.Id,
                    Code = product.Sku,
                    Name = product.Name,
                    TaxType = product.TaxType,    
                    Amount =  product.Price.ActualPrice
                }
            };

            //Need generate tax line for each tier price
            foreach (var tierPrice in product.Price.TierPrices)
            {
                retVal.Add(new TaxLine(tierPrice.Price.Currency)
                {
                    Id = product.Id,
                    Code = product.Sku,
                    Name = product.Name,
                    TaxType = product.TaxType,
                    Quantity = tierPrice.Quantity,
                    Amount = tierPrice.Price
                });              
            }
            return retVal.ToArray();
        }

        public static TaxEvaluationContext ToTaxEvaluationContext(this WorkContext workContext, IEnumerable<Product> products = null)
        {
            var retVal = new TaxEvaluationContext(workContext.CurrentStore.Id)
            {
                Id = workContext.CurrentStore.Id,
                Currency = workContext.CurrentCurrency,
                Type = "",
                Address = workContext.CurrentCustomer.DefaultBillingAddress,
                Customer = workContext.CurrentCustomer
            };

            if (products != null)
            {
                retVal.Lines = products.SelectMany(x => x.ToTaxLines()).ToList();
            }
            return retVal;
        }

        public static TaxEvaluationContext ToTaxEvalContext(this ShoppingCart cart)
        {
            var retVal = new TaxEvaluationContext(cart.StoreId)
            {
                Id = cart.Id,
                Code = cart.Name,
                Currency = cart.Currency,
                Type = "Cart",
            };
            foreach (var lineItem in cart.Items)
            {
                retVal.Lines.Add(new TaxLine(lineItem.Currency)
                {
                    Id = lineItem.Id,
                    Code = lineItem.Sku,
                    Name = lineItem.Name,
                    TaxType = lineItem.TaxType,
                    Amount = lineItem.ExtendedPrice
                });              
            }

            foreach (var shipment in cart.Shipments)
            {
                var totalTaxLine = new TaxLine(shipment.Currency)
                {
                    Id = shipment.Id,
                    Code = shipment.ShipmentMethodCode,
                    Name = shipment.ShipmentMethodCode,
                    TaxType = shipment.TaxType,
                    Amount = shipment.Total
                };
                retVal.Lines.Add(totalTaxLine);               

                if (shipment.DeliveryAddress != null)
                {
                    retVal.Address = shipment.DeliveryAddress;
                }
                retVal.Customer = cart.Customer;
            }

            return retVal;
        }
    }
}
