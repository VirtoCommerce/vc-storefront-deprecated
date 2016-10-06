using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using inventoryDTO = VirtoCommerce.Storefront.AutoRestClients.InventoryModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class InventoryConverter
    {
        public static Inventory ToInventory(this inventoryDTO.InventoryInfo inventoryDTO)
        {
            var result = new Inventory();

            result.InjectFrom<NullableAndEnumValueInjecter>(inventoryDTO);
            result.Status = EnumUtility.SafeParse(inventoryDTO.Status, InventoryStatus.Disabled);

            return result;
        }
    }
}
