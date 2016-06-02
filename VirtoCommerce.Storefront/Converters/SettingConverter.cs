using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.StoreModule.Client.Model;

namespace VirtoCommerce.Storefront.Converters
{
    public static class SettingConverter
    {
        public static SettingEntry ToWebModel(this Setting dto)
        {
            var retVal = new SettingEntry();
            retVal.InjectFrom<NullableAndEnumValueInjecter>(dto);
            retVal.AllowedValues = dto.AllowedValues;
            retVal.ArrayValues = dto.ArrayValues;
            return retVal;
        }
    }
}
