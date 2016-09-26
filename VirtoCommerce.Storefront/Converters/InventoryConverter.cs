using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using inventoryModel = VirtoCommerce.Storefront.AutoRestClients.InventoryModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class InventoryConverter
    {
        public static Inventory ToWebModel(this inventoryModel.InventoryInfo inventoryInfo)
        {
            var result = new Inventory();

            result.InjectFrom<NullableAndEnumValueInjecter>(inventoryInfo);
            result.Status = EnumUtility.SafeParse(inventoryInfo.Status, InventoryStatus.Disabled);

            return result;
        }
    }
}
