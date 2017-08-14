using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.InventoryModuleApi;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Inventory.Services;

namespace VirtoCommerce.Storefront.Services
{
    public class InventoryServiceImpl : IInventoryService
    {
        private readonly IInventoryModuleApiClient _inventoryModuleApi;

        public InventoryServiceImpl(IInventoryModuleApiClient inventoryModuleApi)
        {
            _inventoryModuleApi = inventoryModuleApi;
        }

        public virtual async Task EvaluateProductInventoriesAsync(ICollection<Product> products, WorkContext workContext)
        {
            var inventories = await _inventoryModuleApi.InventoryModule.GetProductsInventoriesByPlentyIdsAsync(products.Select(x => x.Id).ToArray());
            var availFullfilmentCentersIds = workContext.CurrentStore.FulfilmentCenters.Select(x => x.Id).ToArray();
            foreach (var item in products)
            {
                item.InventoryAll = inventories.Where(x => x.ProductId == item.Id).Select(x => x.ToInventory()).Where(x => availFullfilmentCentersIds.Contains(x.FulfillmentCenterId)).ToList();
                item.Inventory = item.InventoryAll.OrderByDescending(x => Math.Max(0, (x.InStockQuantity ?? 0L) - (x.ReservedQuantity ?? 0L))).FirstOrDefault();
                if (workContext.CurrentStore.PrimaryFullfilmentCenter != null)
                {
                    item.Inventory = item.InventoryAll.FirstOrDefault(x => x.FulfillmentCenterId == workContext.CurrentStore.PrimaryFullfilmentCenter.Id);
                }
            }
        }
    }
}