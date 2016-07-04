using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Catalog;
using coreModel = VirtoCommerce.CoreModule.Client.Model;

namespace VirtoCommerce.Storefront.Converters
{
    public static class TaxEvaluationContextConverter
    {
        public static coreModel.TaxLine ToTaxLine(this ShippingMethod shipmentMethod)
        {
            var retVal = new coreModel.TaxLine
            {
                Id = string.Join("&", shipmentMethod.ShipmentMethodCode, shipmentMethod.OptionName),
                Code = string.Join("&", shipmentMethod.ShipmentMethodCode, shipmentMethod.OptionName),
                Name = string.Join("&", shipmentMethod.Name, shipmentMethod.OptionDescription),
                TaxType = shipmentMethod.TaxType,
                Amount = (double)shipmentMethod.Price.Amount
            };
            return retVal;
        }

        public static coreModel.TaxLine[] ToListAndSaleTaxLines(this Product product)
        {
            var retVal = new List<coreModel.TaxLine>
            {
                new coreModel.TaxLine
                {
                    Id = product.Id + "&list",
                    Code = product.Sku,
                    Name = product.Name,
                    TaxType = product.TaxType,
                    Amount = (double) product.Price.ListPrice.Amount
                }
            };

            //Need generate two tax line for List and Sale price to have tax amount for list price also
            if (product.Price.SalePrice != product.Price.ListPrice)
            {
                retVal.Add(new coreModel.TaxLine
                {
                    Id = product.Id + "&sale",
                    Code = product.Sku,
                    Name = product.Name,
                    TaxType = product.TaxType,
                    Amount = (double)product.Price.SalePrice.Amount
                });
            }

            //Need generate tax line for each tier price
            foreach (var tierPrice in product.Price.TierPrices)
            {
                retVal.Add(new coreModel.TaxLine
                {
                    Id = product.Id + "&" + tierPrice.Quantity.ToString(),
                    Code = product.Sku,
                    Name = product.Name,
                    TaxType = product.TaxType,
                    Amount = (double)tierPrice.Price.Amount
                });
            }
            return retVal.ToArray();
        }

        public static coreModel.TaxEvaluationContext ToTaxEvaluationContext(this WorkContext workContext, IEnumerable<Product> products = null)
        {
            var retVal = new coreModel.TaxEvaluationContext
            {
                Id = workContext.CurrentStore.Id,
                Currency = workContext.CurrentCurrency.Code,
                Type = "",
                Address = workContext.CurrentCustomer.DefaultBillingAddress.ToCoreServiceModel(),
                Customer = new coreModel.Contact
                {
                    Id = workContext.CurrentCustomer.Id,
                    Name = workContext.CurrentCustomer.UserName
                }
            };

            if (products != null)
            {
                retVal.Lines = products.SelectMany(x => x.ToListAndSaleTaxLines()).ToList();
            }
            return retVal;
        }

        public static coreModel.TaxEvaluationContext ToTaxEvalContext(this ShoppingCart cart)
        {
            var retVal = new coreModel.TaxEvaluationContext
            {
                Id = cart.Id,
                Code = cart.Name,
                Currency = cart.Currency.Code,
                Type = "Cart",
                Lines = new List<coreModel.TaxLine>()
            };

            foreach (var lineItem in cart.Items)
            {
                var extendedTaxLine = new coreModel.TaxLine
                {
                    Id = lineItem.Id + "&extended",
                    Code = lineItem.Sku,
                    Name = lineItem.Name,
                    TaxType = lineItem.TaxType,
                    Amount = (double)lineItem.ExtendedPrice.Amount
                };
                retVal.Lines.Add(extendedTaxLine);

                var listTaxLine = new coreModel.TaxLine
                {
                    Id = lineItem.Id + "&list",
                    Code = lineItem.Sku,
                    Name = lineItem.Name,
                    TaxType = lineItem.TaxType,
                    Amount = (double)lineItem.ListPrice.Amount
                };
                retVal.Lines.Add(listTaxLine);

                if (lineItem.ListPrice != lineItem.SalePrice)
                {
                    var saleTaxLine = new coreModel.TaxLine
                    {
                        Id = lineItem.Id + "&sale",
                        Code = lineItem.Sku,
                        Name = lineItem.Name,
                        TaxType = lineItem.TaxType,
                        Amount = (double)lineItem.SalePrice.Amount
                    };
                    retVal.Lines.Add(saleTaxLine);
                }

            }
            foreach (var shipment in cart.Shipments)
            {
                var totalTaxLine = new coreModel.TaxLine
                {
                    Id = shipment.Id + "&total",
                    Code = shipment.ShipmentMethodCode,
                    Name = shipment.ShipmentMethodCode,
                    TaxType = shipment.TaxType,
                    Amount = (double)shipment.Total.Amount
                };
                retVal.Lines.Add(totalTaxLine);
                var priceTaxLine = new coreModel.TaxLine
                {
                    Id = shipment.Id + "&price",
                    Code = shipment.ShipmentMethodCode,
                    Name = shipment.ShipmentMethodCode,
                    TaxType = shipment.TaxType,
                    Amount = (double)shipment.ShippingPrice.Amount
                };
                retVal.Lines.Add(priceTaxLine);

                if (shipment.DeliveryAddress != null)
                {
                    //*** alex fix shipping address & customerId to the taxevalcontext
                    retVal.Address = shipment.DeliveryAddress.ToCoreServiceModel();
                    retVal.Address.AddressType = ((int)shipment.DeliveryAddress.Type).ToString();
                }

                retVal.Customer = new coreModel.Contact
                {
                    Id = cart.CustomerId,
                    Name = cart.CustomerName
                };
                //*** end alex fix shipping address & customerId to the taxevalcontext
            }

            return retVal;
        }
    }
}
