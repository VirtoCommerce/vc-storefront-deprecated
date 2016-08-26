using System.Linq;
using Newtonsoft.Json.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using cartModel = VirtoCommerce.Storefront.AutoRestClients.CartModuleApi.Models;
using storeModel = VirtoCommerce.Storefront.AutoRestClients.StoreModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class DynamicPropertyConverter
    {
        public static DynamicProperty ToWebModel(this cartModel.DynamicObjectProperty dto)
        {
            return dto.JsonConvert<OrderModule.Client.Model.DynamicObjectProperty>().ToWebModel();
        }

        public static DynamicProperty ToWebModel(this CustomerModule.Client.Model.DynamicObjectProperty dto)
        {
            return dto.JsonConvert<OrderModule.Client.Model.DynamicObjectProperty>().ToWebModel();
        }

        public static DynamicProperty ToWebModel(this MarketingModule.Client.Model.DynamicObjectProperty dto)
        {
            return dto.JsonConvert<OrderModule.Client.Model.DynamicObjectProperty>().ToWebModel();
        }

        public static DynamicProperty ToWebModel(this QuoteModule.Client.Model.DynamicObjectProperty dto)
        {
            return dto.JsonConvert<OrderModule.Client.Model.DynamicObjectProperty>().ToWebModel();
        }

        public static DynamicProperty ToWebModel(this storeModel.DynamicObjectProperty dto)
        {
            return dto.JsonConvert<OrderModule.Client.Model.DynamicObjectProperty>().ToWebModel();
        }

        public static QuoteModule.Client.Model.DynamicObjectProperty ToQuoteApiModel(this DynamicProperty dto)
        {
            return dto.ToCartApiModel().JsonConvert<QuoteModule.Client.Model.DynamicObjectProperty>();
        }

        public static DynamicProperty ToWebModel(this OrderModule.Client.Model.DynamicObjectProperty dto)
        {
            var result = new DynamicProperty();

            result.InjectFrom<NullableAndEnumValueInjecter>(dto);

            if (dto.DisplayNames != null)
            {
                result.DisplayNames = dto.DisplayNames.Select(x => new LocalizedString(new Language(x.Locale), x.Name)).ToList();
            }

            if (dto.Values != null)
            {
                if (result.IsDictionary)
                {
                    var dictValues = dto.Values.Where(x => x.Value != null)
                        .Select(x => x.Value)
                        .Cast<JObject>()
                        .Select(x => x.ToObject<Platform.Client.Model.DynamicPropertyDictionaryItem>())
                        .ToArray();

                    result.DictionaryValues = dictValues.Select(x => x.ToWebModel()).ToList();
                }
                else
                {
                    result.Values = dto.Values.Select(x => x.ToWebModel()).ToList();
                }
            }

            return result;
        }

        public static cartModel.DynamicObjectProperty ToCartApiModel(this DynamicProperty dynamicProperty)
        {
            var result = new cartModel.DynamicObjectProperty();

            result.InjectFrom<NullableAndEnumValueInjecter>(dynamicProperty);

            if (dynamicProperty.Values != null)
            {
                result.Values = dynamicProperty.Values.Select(v => v.ToCartApiModel()).ToList();
            }
            else if (dynamicProperty.DictionaryValues != null)
            {
                result.Values = dynamicProperty.DictionaryValues.Select(x => x.ToCartApiModel()).ToList();
            }

            return result;
        }


        private static DynamicPropertyDictionaryItem ToWebModel(this Platform.Client.Model.DynamicPropertyDictionaryItem dto)
        {
            var result = new DynamicPropertyDictionaryItem();
            result.InjectFrom<NullableAndEnumValueInjecter>(dto);
            if (dto.DisplayNames != null)
            {
                result.DisplayNames = dto.DisplayNames.Select(x => new LocalizedString(new Language(x.Locale), x.Name)).ToList();
            }
            return result;
        }

        private static LocalizedString ToWebModel(this OrderModule.Client.Model.DynamicPropertyObjectValue dto)
        {
            return new LocalizedString(new Language(dto.Locale), dto.Value.ToString());
        }

        private static cartModel.DynamicPropertyObjectValue ToCartApiModel(this DynamicPropertyDictionaryItem dictItem)
        {
            var result = new cartModel.DynamicPropertyObjectValue { Value = dictItem };
            return result;
        }

        private static cartModel.DynamicPropertyObjectValue ToCartApiModel(this LocalizedString dynamicPropertyObjectValue)
        {
            var result = new cartModel.DynamicPropertyObjectValue
            {
                Value = dynamicPropertyObjectValue.Value,
                Locale = dynamicPropertyObjectValue.Language.CultureName
            };

            return result;
        }
    }
}
