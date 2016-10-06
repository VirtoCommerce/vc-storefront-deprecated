using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Stores;
using VirtoCommerce.Storefront.Model.Stores.Factories;
using storeDTO = VirtoCommerce.Storefront.AutoRestClients.StoreModuleApi.Models;
using platformDTO = VirtoCommerce.Storefront.AutoRestClients.PlatformModuleApi.Models;
using coreDTO = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;

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

        public static Store ToStore(this storeDTO.Store storeDto)
        {
            return StoreConverterInstance.ToStore(storeDto);
        }

        public static DynamicProperty ToDynamicProperty(this storeDTO.DynamicObjectProperty propertyDTO)
        {
            return StoreConverterInstance.ToDynamicProperty(propertyDTO);
        }

        public static SeoInfo ToSeoInfo(this storeDTO.SeoInfo seoDTO)
        {
            return StoreConverterInstance.ToSeoInfo(seoDTO);
        }
    }

    public class StoreConverter
    {
        public virtual SeoInfo ToSeoInfo(storeDTO.SeoInfo seoDto)
        {
            return seoDto.JsonConvert<coreDTO.SeoInfo>().ToSeoInfo();
        }

        public virtual DynamicProperty ToDynamicProperty(storeDTO.DynamicObjectProperty propertyDTO)
        {
            return propertyDTO.JsonConvert<coreDTO.DynamicObjectProperty>().ToDynamicProperty();
        }

        public virtual Store ToStore(storeDTO.Store storeDto)
        {
            var result = ServiceLocator.Current.GetInstance<StoreFactory>().CreateStore();
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
                result.Settings = storeDto.Settings.Select(x => x.JsonConvert<platformDTO.Setting>().ToSettingEntry()).ToList();
            }

            result.TrustedGroups = storeDto.TrustedGroups;
            result.StoreState = EnumUtility.SafeParse(storeDto.StoreState, StoreStatus.Open);
            result.SeoLinksType = EnumUtility.SafeParse(result.Settings.GetSettingValue("Stores.SeoLinksType", ""), SeoLinksType.Collapsed);

            return result;
        }
    }
}
