using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using platformDto = VirtoCommerce.Storefront.AutoRestClients.PlatformModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class SettingConverter
    {
        public static SettingEntry ToSettingEntry(this platformDto.Setting settingDto)
        {
            var retVal = new SettingEntry();
            retVal.InjectFrom<NullableAndEnumValueInjecter>(settingDto);
            retVal.AllowedValues = settingDto.AllowedValues;
            retVal.ArrayValues = settingDto.ArrayValues;
            return retVal;
        }
    }
}
