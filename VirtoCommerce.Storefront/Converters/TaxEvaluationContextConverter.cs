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

        public static TaxLine ToTaxLine(this ShippingMethod shipmentMethod)
        {
            var retVal = new TaxLine(shipmentMethod.Currency)
            {
                Id = string.Join("&", shipmentMethod.ShipmentMethodCode, shipmentMethod.OptionName),
                Code = string.Join("&", shipmentMethod.ShipmentMethodCode, shipmentMethod.OptionName),
                Name = string.Join("&", shipmentMethod.Name, shipmentMethod.OptionDescription),
                TaxType = shipmentMethod.TaxType,
                Amount = shipmentMethod.Price
            };
            return retVal;
        }

        public static TaxLine[] ToListAndSaleTaxLines(this Product product)
        {
            var retVal = new List<TaxLine>
            {
                new TaxLine(product.Currency)
                {
                    Id = product.Id + "&list",
                    Code = product.Sku,
                    Name = product.Name,
                    TaxType = product.TaxType,
                    Amount =  product.Price.ListPrice
                }
            };

            //Need generate two tax line for List and Sale price to have tax amount for list price also
            if (product.Price.SalePrice != product.Price.ListPrice)
            {
                retVal.Add(new TaxLine(product.Currency)
                {
                    Id = product.Id + "&sale",
                    Code = product.Sku,
                    Name = product.Name,
                    TaxType = product.TaxType,
                    Amount = product.Price.SalePrice
                });
            }

            //Need generate tax line for each tier price
            foreach (var tierPrice in product.Price.TierPrices)
            {
                retVal.Add(new TaxLine(tierPrice.Price.Currency)
                {
                    Id = product.Id + "&" + tierPrice.Quantity.ToString(),
                    Code = product.Sku,
                    Name = product.Name,
                    TaxType = product.TaxType,
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
                retVal.Lines = products.SelectMany(x => x.ToListAndSaleTaxLines()).ToList();
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
                var extendedTaxLine = new TaxLine(lineItem.Currency)
                {
                    Id = lineItem.Id + "&extended",
                    Code = lineItem.Sku,
                    Name = lineItem.Name,
                    TaxType = lineItem.TaxType,
                    Amount = lineItem.ExtendedPrice
                };
                retVal.Lines.Add(extendedTaxLine);

                var listTaxLine = new TaxLine(lineItem.Currency)
                {
                    Id = lineItem.Id + "&list",
                    Code = lineItem.Sku,
                    Name = lineItem.Name,
                    TaxType = lineItem.TaxType,
                    Amount = lineItem.ListPrice
                };
                retVal.Lines.Add(listTaxLine);

                if (lineItem.ListPrice != lineItem.SalePrice)
                {
                    var saleTaxLine = new TaxLine(lineItem.Currency)
                    {
                        Id = lineItem.Id + "&sale",
                        Code = lineItem.Sku,
                        Name = lineItem.Name,
                        TaxType = lineItem.TaxType,
                        Amount = lineItem.SalePrice
                    };
                    retVal.Lines.Add(saleTaxLine);
                }

            }
            foreach (var shipment in cart.Shipments)
            {
                var totalTaxLine = new TaxLine(shipment.Currency)
                {
                    Id = shipment.Id + "&total",
                    Code = shipment.ShipmentMethodCode,
                    Name = shipment.ShipmentMethodCode,
                    TaxType = shipment.TaxType,
                    Amount = shipment.Total
                };
                retVal.Lines.Add(totalTaxLine);
                var priceTaxLine = new TaxLine(shipment.Currency)
                {
                    Id = shipment.Id + "&price",
                    Code = shipment.ShipmentMethodCode,
                    Name = shipment.ShipmentMethodCode,
                    TaxType = shipment.TaxType,
                    Amount = shipment.ShippingPrice
                };
                retVal.Lines.Add(priceTaxLine);

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
