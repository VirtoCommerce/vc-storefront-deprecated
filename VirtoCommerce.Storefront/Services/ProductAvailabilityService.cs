using System;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Catalog.Services;

namespace VirtoCommerce.Storefront.Services
{
    public class ProductAvailabilityService : IProductAvailabilityService
    {
        public long? GetAvailableQuantity(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            long? availableQuantity = null;

            if (product.TrackInventory && product.Inventory != null)
            {
                if (product.Inventory.InStockQuantity.HasValue)
                {
                    availableQuantity = product.Inventory.InStockQuantity.Value;
                }
                if (product.Inventory.ReservedQuantity.HasValue)
                {
                    availableQuantity -= product.Inventory.ReservedQuantity.Value;
                }
            }

            return availableQuantity;
        }

        public bool IsAvailable(Product product, long requestedQuantity)
        {
            if (product == null)
            {
                throw new ArgumentNullException("product");
            }

            var available = product.IsActive && product.IsBuyable;
            var availableQuantity = GetAvailableQuantity(product);
            if (availableQuantity.HasValue)
            {
                available = available && (availableQuantity > requestedQuantity ||
                    product.Inventory.AllowBackorder.HasValue && product.Inventory.AllowBackorder.Value ||
                    product.Inventory.AllowPreorder.HasValue && product.Inventory.AllowPreorder.Value);
            }

            return available;
        }
    }
}