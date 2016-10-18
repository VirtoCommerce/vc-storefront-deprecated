using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using inventoryDto = VirtoCommerce.Storefront.AutoRestClients.InventoryModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class InventoryConverter
    {
        public static Inventory ToInventory(this inventoryDto.InventoryInfo inventoryDto)
        {
            var result = new Inventory();

            result.InjectFrom<NullableAndEnumValueInjecter>(inventoryDto);
            result.Status = EnumUtility.SafeParse(inventoryDto.Status, InventoryStatus.Disabled);

            return result;
        }
    }
}
