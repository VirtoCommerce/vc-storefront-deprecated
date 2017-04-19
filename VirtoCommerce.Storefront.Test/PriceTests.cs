using System;
using System.Collections.Generic;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using Xunit;

namespace VirtoCommerce.Storefront.Test
{
    public class PriceTests : StorefrontTestBase
    {
        private Currency currency = new Currency(new Language("en-US"), "USD");

        [Fact]
        public void CreateNewShoppingCart_CheckTotalPrices()
        {
            var language = new Language("en-US");
            var cart = new ShoppingCart(currency, language);

            decimal discount = 10;
            decimal taxRate = 0.13M;
            int productQuantity = 11;

            decimal product1Price = 109.99M;
            decimal product2Price = 101.19M;
            decimal product3Price = 99.89M;

            Product product1 = GetProduct(product1Price, discount, "Product 1");
            Product product2 = GetProduct(product2Price, discount, "Product 2");
            Product product3 = GetProduct(product3Price, discount, "Product 3");

            decimal product1TaxRate = product1.Price.ActualPrice.Amount * taxRate;
            decimal product2TaxRate = product2.Price.ActualPrice.Amount * taxRate;
            decimal product3TaxRate = product3.Price.ActualPrice.Amount * taxRate;

            IEnumerable<TaxRate> taxRates = new List<TaxRate> {
                GetTaxRate(product1.Id, product1TaxRate),
                GetTaxRate(product2.Id, product2TaxRate),
                GetTaxRate(product3.Id, product3TaxRate)
            };

            product1.ApplyTaxRates(taxRates);
            product2.ApplyTaxRates(taxRates);
            product3.ApplyTaxRates(taxRates);

            cart.Items.Add(product1.ToLineItem(language, productQuantity));
            cart.Items.Add(product2.ToLineItem(language, productQuantity));
            cart.Items.Add(product3.ToLineItem(language, productQuantity));

            decimal subtotalRaw = (product1Price * 11) + (product2Price * 11) + (product3Price * 11);
            decimal subtotal = Round(subtotalRaw);

            /* Old version money class, where we use rounded amount instead InternalAmount in internal operations
            decimal totalDiscountRaw = (Round(product1Price / discount) * productQuantity) +
                (Round(product2Price / discount) * productQuantity) +
                (Round(product3Price / discount) * productQuantity);
            decimal totalDiscount = Round(totalDiscountRaw);

            decimal taxesRaw = Round(((product1Price - Round(product1Price / discount)) * productQuantity) * taxRate) +
                Round(((product2Price - Round(product2Price / discount)) * productQuantity) * taxRate) +
                Round(((product3Price - Round(product3Price / discount)) * productQuantity) * taxRate);
            decimal taxes = Round(taxesRaw);
            */

            decimal product1Discount = (product1Price / discount) * productQuantity;
            decimal product2Discount = (product2Price / discount) * productQuantity;
            decimal product3Discount = (product3Price / discount) * productQuantity;

            decimal totalDiscountRaw = product1Discount + product2Discount + product3Discount;
            decimal totalDiscount = Round(totalDiscountRaw);

            decimal product1Tax = (product1Price - (product1Price / discount)) * productQuantity * taxRate;
            decimal product2Tax = (product2Price - (product2Price / discount)) * productQuantity * taxRate;
            decimal product3Tax = (product3Price - (product3Price / discount)) * productQuantity * taxRate;

            decimal taxesRaw = product1Tax + product2Tax + product3Tax;
            decimal taxes = Round(taxesRaw);

            decimal totalPrice = subtotal - totalDiscount + taxes;

            Assert.True(cart.ItemsCount == 3);
            Assert.True(cart.ItemsQuantity == 33);

            Assert.Equal(cart.SubTotal.Amount, subtotal);
            Assert.Equal(cart.DiscountTotal.Amount, totalDiscount);
            Assert.Equal(cart.TaxTotal.Amount, taxes);
            Assert.Equal(cart.Total.Amount, totalPrice);
        }

        private Product GetProduct(decimal price, decimal discountPercent, string name)
        {
            var product = new Product
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Price = new ProductPrice(currency)
                {
                    ListPrice = new Money(price, currency),
                    SalePrice = new Money(price, currency),
                    DiscountAmount = new Money(price / discountPercent, currency)
                }
            };

            return product;
        }

        private TaxRate GetTaxRate(string productId, decimal rate)
        {
            var taxRate = new TaxRate(currency)
            {
                Line = new TaxLine(currency)
                {
                    Id = productId
                },
                Rate = new Money(rate, currency)
            };

            return taxRate;
        }

        private decimal Round(decimal value)
        {
            return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
        }
    }
}
