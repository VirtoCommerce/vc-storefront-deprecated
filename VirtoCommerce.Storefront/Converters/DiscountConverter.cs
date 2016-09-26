using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;
using cartModel = VirtoCommerce.Storefront.AutoRestClients.CartModuleApi.Models;
using orderModel = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class DiscountConverter
    {
        public static Discount ToWebModel(this cartModel.Discount serviceModel, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(serviceModel.Currency)) ?? new Currency(language, serviceModel.Currency);
            var webModel = new Discount(currency);

            webModel.InjectFrom<NullableAndEnumValueInjecter>(serviceModel);
            webModel.Amount = new Money(serviceModel.DiscountAmount ?? 0, currency);

            return webModel;
        }

        public static cartModel.Discount ToServiceModel(this Discount webModel)
        {
            var serviceModel = new cartModel.Discount();

            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(webModel);

            serviceModel.Currency = webModel.Amount.Currency.Code;
            serviceModel.DiscountAmount = (double)webModel.Amount.Amount;

            return serviceModel;
        }

        public static Discount ToWebModel(this orderModel.Discount serviceModel, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(serviceModel.Currency)) ?? new Currency(language, serviceModel.Currency);
            var webModel = new Discount(currency);

            webModel.InjectFrom<NullableAndEnumValueInjecter>(serviceModel);

            webModel.Amount = new Money(serviceModel.DiscountAmount ?? 0, currency);

            return webModel;
        }
    }
}
