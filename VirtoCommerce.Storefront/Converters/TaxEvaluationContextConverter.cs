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
                Id = shipmentMethod.ShipmentMethodCode,
                Code = shipmentMethod.ShipmentMethodCode,
                Name = shipmentMethod.ShipmentMethodCode,
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
                    Id = product.Id,
                    Code = "list",
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
                    Id = product.Id,
                    Code = "sale",
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
                    Id = product.Id,
                    Code = tierPrice.Quantity.ToString(),
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
                    Id = lineItem.Id,
                    Code = "extended",
                    Name = lineItem.Name,
                    TaxType = lineItem.TaxType,
                    Amount = (double)lineItem.ExtendedPrice.Amount
                };
                retVal.Lines.Add(extendedTaxLine);

                var listTaxLine = new coreModel.TaxLine
                {
                    Id = lineItem.Id,
                    Code = "list",
                    Name = lineItem.Name,
                    TaxType = lineItem.TaxType,
                    Amount = (double)lineItem.ListPrice.Amount
                };
                retVal.Lines.Add(listTaxLine);

                if (lineItem.ListPrice != lineItem.SalePrice)
                {
                    var saleTaxLine = new coreModel.TaxLine
                    {
                        Id = lineItem.Id,
                        Code = "sale",
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
                    Id = shipment.Id,
                    Code = "total",
                    Name = shipment.ShipmentMethodCode,
                    TaxType = shipment.TaxType,
                    Amount = (double)shipment.Total.Amount
                };
                retVal.Lines.Add(totalTaxLine);
                var priceTaxLine = new coreModel.TaxLine
                {
                    Id = shipment.Id,
                    Code = "price",
                    Name = shipment.ShipmentMethodCode,
                    TaxType = shipment.TaxType,
                    Amount = (double)shipment.ShippingPrice.Amount
                };
                retVal.Lines.Add(priceTaxLine);


                //*** alex fix shipping address & customerId to the taxevalcontext
                retVal.Address = new coreModel.Address
                {
                    FirstName = shipment.DeliveryAddress.FirstName,
                    LastName = shipment.DeliveryAddress.LastName,
                    Organization = shipment.DeliveryAddress.Organization,
                    Line1 = shipment.DeliveryAddress.Line1,
                    Line2 = shipment.DeliveryAddress.Line2,
                    City = shipment.DeliveryAddress.City,
                    PostalCode = shipment.DeliveryAddress.PostalCode,
                    RegionId = shipment.DeliveryAddress.RegionId,
                    RegionName = shipment.DeliveryAddress.RegionName,
                    CountryCode = shipment.DeliveryAddress.CountryCode,
                    CountryName = shipment.DeliveryAddress.CountryName,
                    Phone = shipment.DeliveryAddress.Phone,
                    AddressType = ((int)shipment.DeliveryAddress.Type).ToString()
                };

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
