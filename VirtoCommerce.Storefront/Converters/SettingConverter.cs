using Omu.ValueInjecter;
using platformDTO = VirtoCommerce.Storefront.AutoRestClients.PlatformModuleApi.Models;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Converters
{
    public static class SettingConverter
    {
        public static SettingEntry ToSettingEntry(this platformDTO.Setting settingDTO)
        {
            var retVal = new SettingEntry();
            retVal.InjectFrom<NullableAndEnumValueInjecter>(settingDTO);
            retVal.AllowedValues = settingDTO.AllowedValues;
            retVal.ArrayValues = settingDTO.ArrayValues;
            return retVal;
        }
    }
}
