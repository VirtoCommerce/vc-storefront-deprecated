using System;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Catalog.Services;

namespace VirtoCommerce.Storefront.Services
{
    public class ProductAvailabilityService : IProductAvailabilityService
    {
        public virtual async Task<bool> IsAvailable(Product product, long requestedQuantity)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var isAvailable = product.IsActive && product.IsBuyable;

            if (isAvailable && product.TrackInventory && product.Inventory != null)
            {
                isAvailable = product.Inventory.AllowPreorder == true ||
                              product.Inventory.AllowBackorder == true ||
                              await GetAvailableQuantity(product) >= requestedQuantity;
            }

            return isAvailable;
        }

        public virtual Task<long> GetAvailableQuantity(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            long availableQuantity = 0;

            if (product.TrackInventory && product.Inventory != null)
            {
                availableQuantity = Math.Max(0, (product.Inventory.InStockQuantity ?? 0L) - (product.Inventory.ReservedQuantity ?? 0L));
            }

            return Task.FromResult(availableQuantity);
        }
    }
}