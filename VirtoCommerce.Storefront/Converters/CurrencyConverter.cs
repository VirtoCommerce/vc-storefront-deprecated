using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using coreModel = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CurrencyConverter
    {
        public static Currency ToWebModel(this coreModel.Currency currency, Language language)
        {
            var retVal = new Currency(language, currency.Code, currency.Name, currency.Symbol, (decimal)currency.ExchangeRate.Value)
            {
                CustomFormatting = currency.CustomFormatting
            };
            return retVal;
        }
    }
}
