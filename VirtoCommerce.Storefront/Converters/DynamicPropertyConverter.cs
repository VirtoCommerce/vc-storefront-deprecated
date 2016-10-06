using System.Linq;
using Newtonsoft.Json.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using platformDTO = VirtoCommerce.Storefront.AutoRestClients.PlatformModuleApi.Models;
using coreDTO = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class DynamicPropertyConverter
    {
        public static DynamicProperty ToDynamicProperty(this coreDTO.DynamicObjectProperty propertyDTO)
        {
            var result = new DynamicProperty();

            result.InjectFrom<NullableAndEnumValueInjecter>(propertyDTO);

            if (propertyDTO.DisplayNames != null)
            {
                result.DisplayNames = propertyDTO.DisplayNames.Select(x => new LocalizedString(new Language(x.Locale), x.Name)).ToList();
            }

            if (propertyDTO.Values != null)
            {
                if (result.IsDictionary)
                {
                    var dictValues = propertyDTO.Values.Where(x => x.Value != null)
                        .Select(x => x.Value)
                        .Cast<JObject>()
                        .Select(x => x.ToObject<platformDTO.DynamicPropertyDictionaryItem>())
                        .ToArray();

                    result.DictionaryValues = dictValues.Select(x => x.ToDictItem()).ToList();
                }
                else
                {
                    result.Values = propertyDTO.Values.Select(x => x.ToLocalizedString()).ToList();
                }
            }

            return result;
        }


        public static coreDTO.DynamicObjectProperty ToDynamicPropertyDTO(this DynamicProperty dynamicProperty)
        {
            var result = new coreDTO.DynamicObjectProperty();

            result.InjectFrom<NullableAndEnumValueInjecter>(dynamicProperty);

            if (dynamicProperty.Values != null)
            {
                result.Values = dynamicProperty.Values.Select(v => v.ToPropertyValueDTO()).ToList();
            }
            else if (dynamicProperty.DictionaryValues != null)
            {
                result.Values = dynamicProperty.DictionaryValues.Select(x => x.ToPropertyValueDTO()).ToList();
            }

            return result;
        }

        private static DynamicPropertyDictionaryItem ToDictItem(this platformDTO.DynamicPropertyDictionaryItem dto)
        {
            var result = new DynamicPropertyDictionaryItem();
            result.InjectFrom<NullableAndEnumValueInjecter>(dto);
            if (dto.DisplayNames != null)
            {
                result.DisplayNames = dto.DisplayNames.Select(x => new LocalizedString(new Language(x.Locale), x.Name)).ToList();
            }
            return result;
        }

        private static LocalizedString ToLocalizedString(this coreDTO.DynamicPropertyObjectValue dto)
        {
            return new LocalizedString(new Language(dto.Locale), dto.Value.ToString());
        }

        private static coreDTO.DynamicPropertyObjectValue ToPropertyValueDTO(this DynamicPropertyDictionaryItem dictItem)
        {
            var result = new coreDTO.DynamicPropertyObjectValue { Value = dictItem };
            return result;
        }

        private static coreDTO.DynamicPropertyObjectValue ToPropertyValueDTO(this LocalizedString dynamicPropertyObjectValue)
        {
            var result = new coreDTO.DynamicPropertyObjectValue
            {
                Value = dynamicPropertyObjectValue.Value,
                Locale = dynamicPropertyObjectValue.Language.CultureName
            };

            return result;
        }     
    }
}
