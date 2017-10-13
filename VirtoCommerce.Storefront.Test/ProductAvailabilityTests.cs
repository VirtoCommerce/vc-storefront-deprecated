using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Storefront.Model.Catalog;
using Xunit;

namespace VirtoCommerce.Storefront.Test
{
    [Trait("Category", "Unit")]
    public class ProductAvailabilityTests : StorefrontTestBase
    {
        [Theory]
        [InlineData(false, false, false, null, null, null, null, 1, false)]
        [InlineData(true, true, false, null, null, null, null, 1, true)]
        [InlineData(true, true, true, null, null, null, null, 1, true)]
        [InlineData(true, true, true, false, null, null, null, 1, false)]
        [InlineData(true, true, true, true, null, null, null, 1, true)]
        [InlineData(true, true, true, null, false, null, null, 1, false)]
        [InlineData(true, true, true, null, true, null, null, 1, true)]
        [InlineData(true, true, true, false, false, 0L, null, 1, false)]
        [InlineData(true, true, true, false, false, 1L, null, 1, true)]
        [InlineData(true, true, true, false, false, 1L, null, 2, false)]
        [InlineData(true, true, true, false, false, 1L, 0L, 1, true)]
        [InlineData(true, true, true, false, false, 1L, 1L, 1, false)]
        [InlineData(true, true, true, false, false, 1L, 2L, 1, false)]
        public void TestIsAvailable(bool isActive, bool isBuyable, bool trackInventory, bool? allowPreorder, bool? allowBackorder, long? inStockQuantity, long? reservedQuantity, long requestedQuantity, bool expectedResult)
        {
            var service = GetProductAvailabilityService();
            var product = CreateProduct(isActive, isBuyable, trackInventory, allowPreorder, allowBackorder, new List<Tuple<long?, long?>> { new Tuple<long?, long?>(inStockQuantity, reservedQuantity) });

            var isAvailable = service.IsAvailable(product, requestedQuantity).Result;
            Assert.Equal(expectedResult, isAvailable);
        }

        [Theory]
        [InlineData(false, null, null, null, null, false)]
        [InlineData(false, null, null, null, null, false)]
        [InlineData(true, null, null, null, null, false)]
        [InlineData(true, false, null, null, null, false)]
        [InlineData(true, true, null, null, null, true)]
        [InlineData(true, null, false, null, null, false)]
        [InlineData(true, null, true, null, null, true)]
        [InlineData(true, false, false, 0L, null, false)]
        [InlineData(true, false, false, 1L, null, true)]
        [InlineData(true, false, false, 1L, 0L, true)]
        [InlineData(true, false, false, 1L, 1L, false)]
        [InlineData(true, false, false, 1L, 2L, false)]
        public void TestIsInStock(bool trackInventory, bool? allowPreorder, bool? allowBackorder, long? inStockQuantity, long? reservedQuantity, bool expectedResult)
        {
            var service = GetProductAvailabilityService();
            var product = CreateProduct(true, true, trackInventory, allowPreorder, allowBackorder, new List<Tuple<long?, long?>> { new Tuple<long?, long?>(inStockQuantity, reservedQuantity) });
            var inStock = service.IsInStock(product).Result;
            Assert.Equal(expectedResult, inStock);
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        public void TestIsBuyable(bool isActive, bool isBuyable, bool expectedResult)
        {
            var service = GetProductAvailabilityService();
            var product = CreateProduct(isActive, isBuyable, false, null, null, Enumerable.Empty<Tuple<long?, long?>>().ToList());
            var buyable = service.IsBuyable(product);
            Assert.Equal(expectedResult, buyable);
        }

        [Theory]
        [InlineData(false, false, false, null, null, null, null, 0L)]
        [InlineData(true, true, false, null, null, null, null, 0L)]
        [InlineData(true, true, true, null, null, null, null, 0L)]
        [InlineData(true, true, true, false, null, null, null, 0L)]
        [InlineData(true, true, true, true, null, null, null, 0L)]
        [InlineData(true, true, true, null, false, null, null, 0L)]
        [InlineData(true, true, true, null, true, null, null, 0L)]
        [InlineData(true, true, true, false, false, 0L, null, 0L)]
        [InlineData(true, true, true, false, false, 1L, null, 1L)]
        [InlineData(true, true, true, false, false, 1L, 0L, 1L)]
        [InlineData(true, true, true, false, false, 1L, 1L, 0L)]
        [InlineData(true, true, true, false, false, 1L, 2L, 0L)]
        public void TestAvailableQuantity(bool isActive, bool isBuyable, bool trackInventory, bool? allowPreorder, bool? allowBackorder, long? inStockQuantity, long? reservedQuantity, long? expectedResult)
        {
            var service = GetProductAvailabilityService();
            var product = CreateProduct(isActive, isBuyable, trackInventory, allowPreorder, allowBackorder, new List<Tuple<long?, long?>> { new Tuple<long?, long?>(inStockQuantity, reservedQuantity) });

            var availableQuantity = service.GetAvailableQuantity(product).Result;
            Assert.Equal(expectedResult, availableQuantity);
        }

        [Theory]
        [InlineData(true, true, true, false, false, 0L, 0L, 0L, 0L, 0L)]
        [InlineData(true, true, true, false, false, 1L, 0L, 1L, 0L, 2L)]
        [InlineData(true, true, true, false, false, 1L, 0L, 0L, 0L, 1L)]
        public void TestAvailableQuantityWithMultipleFulfillmentCenters(bool isActive, bool isBuyable, bool trackInventory, bool? allowPreorder, bool? allowBackorder, long? inStockQuantity1, long? reservedQuantity1, long? inStockQuantity2, long? reservedQuantity2, long? expectedResult)
        {
            var service = GetProductAvailabilityService();
            var product = CreateProduct(isActive, isBuyable, trackInventory, allowPreorder, allowBackorder, new List<Tuple<long?, long?>>{new Tuple<long?, long?>(inStockQuantity1, reservedQuantity1), new Tuple<long?, long?>(inStockQuantity2, reservedQuantity2) });
            var availableQuantity = service.GetAvailableQuantity(product).Result;
            Assert.Equal(expectedResult, availableQuantity);
        }


        private static Product CreateProduct(bool isActive, bool isBuyable, bool trackInventory, bool? allowPreorder, bool? allowBackorder, List<Tuple<long?, long?>> inventoriesList)
        {
            var result = new Product
            {
                IsActive = isActive,
                IsBuyable = isBuyable,
                TrackInventory = trackInventory,
            };
            
                foreach (var tuple in inventoriesList)
                {
                    if (allowPreorder != null || allowBackorder != null ||tuple.Item1 != null || tuple.Item2 != null)
                    {
                        if (result.InventoryAll == null)
                        {
                            result.InventoryAll = new List<Inventory>();
                        }

                        var inventory = new Inventory
                        {
                            AllowPreorder = allowPreorder,
                            AllowBackorder = allowBackorder,
                            InStockQuantity = tuple.Item1,
                            ReservedQuantity = tuple.Item2
                        };

                        result.Inventory = inventory;
                        result.InventoryAll.Add(inventory);
                    }
                }   
            

            return result;
        }
    }
}
