using System;
using System.Linq;
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
        private static Language language = new Language("en-US");
        private Currency currency = new Currency(language, "USD");

        [Fact]
        public void ShoppingCart_OneItemPerProduct_CheckTotalPrices()
        {
            var cart = new ShoppingCart(currency, language);

            decimal discount = 10;
            decimal taxRate = 0.13M;
            int productQuantity = 1;

            var productsInfo = new List<ProductInfo>
            {
                new ProductInfo("Product 1", 1195M, discount, productQuantity),
                new ProductInfo("Product 2", 159M, discount, productQuantity),
                new ProductInfo("Product 3", 1698M, discount, productQuantity),
                new ProductInfo("Product 4", 3295M, discount, productQuantity),
                new ProductInfo("Product 5", 399M, discount, productQuantity),
                new ProductInfo("Product 6", 1259M, discount, productQuantity),
                new ProductInfo("Product 7", 648M, discount, productQuantity),
                new ProductInfo("Product 8", 110M, discount, productQuantity),
                new ProductInfo("Product 9", 589.99M, discount, productQuantity),
                new ProductInfo("Product 10", 998M, discount, productQuantity),
                new ProductInfo("Product 11", 1199M, discount, productQuantity),
                new ProductInfo("Product 12", 995.99M, discount, productQuantity),
                new ProductInfo("Product 13", 101.19M, discount, productQuantity),
                new ProductInfo("Product 14", 4999M, discount, productQuantity),
                new ProductInfo("Product 15", 223.33M, discount, productQuantity),
                new ProductInfo("Product 16", 4997M, discount, productQuantity),
                new ProductInfo("Product 17", 109.99M, discount, productQuantity),
                new ProductInfo("Product 18", 327.99M, discount, productQuantity),
                new ProductInfo("Product 19", 797.99M, discount, productQuantity),
                new ProductInfo("Product 20", 99M, discount, productQuantity),
                new ProductInfo("Product 21", 399.99M, discount, productQuantity),
                new ProductInfo("Product 22", 109M, discount, productQuantity),
                new ProductInfo("Product 23", 142.75M, discount, productQuantity),
                new ProductInfo("Product 24", 388M, discount, productQuantity),
                new ProductInfo("Product 25", 220.52M, discount, productQuantity),
                new ProductInfo("Product 26", 1498.99M, discount, productQuantity),
                new ProductInfo("Product 27", 999M, discount, productQuantity),
                new ProductInfo("Product 28", 1799.99M, discount, productQuantity),
                new ProductInfo("Product 29", 99.89M, discount, productQuantity),
                new ProductInfo("Product 30", 99.99M, discount, productQuantity),
                new ProductInfo("Product 31", 6999M, discount, productQuantity),
                new ProductInfo("Product 32", 599M, discount, productQuantity),
                new ProductInfo("Product 33", 498M, discount, productQuantity)
            };

            var products = new List<Product>();
            var taxRates = new List<TaxRate>();

            foreach (ProductInfo productInfo in productsInfo)
            {
                Product product = GetProduct(productInfo);
                products.Add(product);

                decimal productTaxRateValue = product.Price.ActualPrice.Amount * taxRate;
                TaxRate productTaxRate = GetTaxRate(product.Id, productTaxRateValue);
                taxRates.Add(productTaxRate);

                product.ApplyTaxRates(taxRates);
                cart.Items.Add(product.ToLineItem(language, productInfo.Quantity));
            }

            Assert.True(cart.ItemsCount == products.Count);
            Assert.True(cart.ItemsQuantity == products.Count * productQuantity);

            decimal subtotalRaw = productsInfo.Sum(p => p.Price * p.Quantity);
            decimal subtotal = Round(subtotalRaw);

            decimal discountTotalRaw = productsInfo.Sum(p => (p.Price / discount) * p.Quantity);
            decimal discountTotal = Round(discountTotalRaw);

            decimal taxesRaw = productsInfo.Sum(p => (p.Price - (p.Price / discount)) * p.Quantity * taxRate);
            decimal taxes = Round(taxesRaw);

            decimal totalPriceRaw = subtotalRaw - discountTotalRaw + taxesRaw;
            decimal totalPrice = Round(totalPriceRaw);

            Assert.Equal(cart.SubTotal.Amount, subtotal);
            Assert.Equal(cart.DiscountTotal.Amount, discountTotal);
            Assert.Equal(cart.TaxTotal.Amount, taxes);
            Assert.Equal(cart.Total.Amount, totalPrice);
        }

        [Fact]
        public void ShoppingCart_ManyItemsPerProduct_CheckTotalPrices()
        {
            var cart = new ShoppingCart(currency, language);

            decimal discount = 10;
            decimal taxRate = 0.13M;
            int productQuantity = 11;

            var productsInfo = new List<ProductInfo>
            {
                new ProductInfo("Product 1", 109.99M, discount, productQuantity),
                new ProductInfo("Product 2", 101.19M, discount, productQuantity),
                new ProductInfo("Product 3", 99.89M, discount, productQuantity)
            };

            var products = new List<Product>();
            var taxRates = new List<TaxRate>();

            foreach (ProductInfo productInfo in productsInfo)
            {
                Product product = GetProduct(productInfo);
                products.Add(product);

                decimal productTaxRateValue = product.Price.ActualPrice.Amount * taxRate;
                TaxRate productTaxRate = GetTaxRate(product.Id, productTaxRateValue);
                taxRates.Add(productTaxRate);

                product.ApplyTaxRates(taxRates);
                cart.Items.Add(product.ToLineItem(language, productInfo.Quantity));
            }

            Assert.True(cart.ItemsCount == products.Count);
            Assert.True(cart.ItemsQuantity == products.Count * productQuantity);

            decimal subtotalRaw = productsInfo.Sum(p => p.Price * p.Quantity);
            decimal subtotal = Round(subtotalRaw);
            
            decimal discountTotalRaw = productsInfo.Sum(p => (p.Price / discount) * p.Quantity);
            decimal discountTotal = Round(discountTotalRaw);

            decimal taxesRaw = productsInfo.Sum(p => (p.Price - (p.Price / discount)) * p.Quantity * taxRate);
            decimal taxes = Round(taxesRaw);

            decimal totalPriceRaw = subtotalRaw - discountTotalRaw + taxesRaw;
            decimal totalPrice = Round(totalPriceRaw);

            Assert.Equal(cart.SubTotal.Amount, subtotal);
            Assert.Equal(cart.DiscountTotal.Amount, discountTotal);
            Assert.Equal(cart.TaxTotal.Amount, taxes);
            Assert.Equal(cart.Total.Amount, totalPrice);
        }

        private Product GetProduct(ProductInfo productInfo)
        {
            var product = new Product
            {
                Id = Guid.NewGuid().ToString(),
                Name = productInfo.Name,
                Price = new ProductPrice(currency)
                {
                    ListPrice = new Money(productInfo.Price, currency),
                    SalePrice = new Money(productInfo.Price, currency),
                    DiscountAmount = new Money(productInfo.Price / productInfo.DiscountPercent, currency)
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

        private class ProductInfo
        {
            public ProductInfo(string name, decimal price, decimal discountPercent, int quantity)
            {
                Name = name;
                Price = price;
                DiscountPercent = discountPercent;
                Quantity = quantity;
            }

            public string Name { get; set; }

            public decimal Price { get; set; }

            public decimal DiscountPercent { get; set; }

            public int Quantity { get; set; }
        }
    }
}
