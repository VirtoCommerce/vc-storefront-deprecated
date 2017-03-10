using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Stores;
using coreDto = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;
using platformDto = VirtoCommerce.Storefront.AutoRestClients.PlatformModuleApi.Models;
using storeDto = VirtoCommerce.Storefront.AutoRestClients.StoreModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class StoreConverterExtension
    {
        public static StoreConverter StoreConverterInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<StoreConverter>();
            }
        }

        public static Store ToStore(this storeDto.Store storeDto)
        {
            return StoreConverterInstance.ToStore(storeDto);
        }

        public static DynamicProperty ToDynamicProperty(this storeDto.DynamicObjectProperty propertyDto)
        {
            return StoreConverterInstance.ToDynamicProperty(propertyDto);
        }

        public static SeoInfo ToSeoInfo(this storeDto.SeoInfo seoDto)
        {
            return StoreConverterInstance.ToSeoInfo(seoDto);
        }
    }

    public class StoreConverter
    {
        public virtual SeoInfo ToSeoInfo(storeDto.SeoInfo seoDto)
        {
            return seoDto.JsonConvert<coreDto.SeoInfo>().ToSeoInfo();
        }

        public virtual DynamicProperty ToDynamicProperty(storeDto.DynamicObjectProperty propertyDto)
        {
            return propertyDto.JsonConvert<coreDto.DynamicObjectProperty>().ToDynamicProperty();
        }

        public virtual Store ToStore(storeDto.Store storeDto)
        {
            var result = new Store();
            result.InjectFrom<NullableAndEnumValueInjecter>(storeDto);

            if (!storeDto.SeoInfos.IsNullOrEmpty())
            {
                result.SeoInfos = storeDto.SeoInfos.Select(ToSeoInfo).ToList();
            }

            result.DefaultLanguage = storeDto.DefaultLanguage != null ? new Language(storeDto.DefaultLanguage) : Language.InvariantLanguage;

            if (!storeDto.Languages.IsNullOrEmpty())
            {
                result.Languages = storeDto.Languages.Select(x => new Language(x)).ToList();
            }

            if (!storeDto.Currencies.IsNullOrEmpty())
            {
                result.Currencies.AddRange(storeDto.Currencies.Select(x => new Currency(Language.InvariantLanguage, x)));
            }

            result.DefaultCurrency = result.Currencies.FirstOrDefault(x => x.Equals(storeDto.DefaultCurrency));

            if (!storeDto.DynamicProperties.IsNullOrEmpty())
            {
                result.DynamicProperties = storeDto.DynamicProperties.Select(ToDynamicProperty).ToList();
                result.ThemeName = result.DynamicProperties.GetDynamicPropertyValue("DefaultThemeName");
            }

            if (!storeDto.Settings.IsNullOrEmpty())
            {
                result.Settings = storeDto.Settings.Where(x => !x.ValueType.EqualsInvariant("SecureString")).Select(x => x.JsonConvert<platformDto.Setting>().ToSettingEntry()).ToList();
            }

            result.TrustedGroups = storeDto.TrustedGroups;
            result.StoreState = EnumUtility.SafeParse(storeDto.StoreState, StoreStatus.Open);
            result.SeoLinksType = EnumUtility.SafeParse(result.Settings.GetSettingValue("Stores.SeoLinksType", ""), SeoLinksType.Collapsed);

            return result;
        }
    }
}
