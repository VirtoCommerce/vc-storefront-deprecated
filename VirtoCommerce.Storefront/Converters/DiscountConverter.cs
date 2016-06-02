using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;

namespace VirtoCommerce.Storefront.Converters
{
    public static class DiscountConverter
    {
        public static Discount ToWebModel(this CartModule.Client.Model.Discount serviceModel, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(serviceModel.Currency)) ?? new Currency(language, serviceModel.Currency);
            var webModel = new Discount(currency);

            webModel.InjectFrom(serviceModel);
            webModel.Amount = new Money(serviceModel.DiscountAmount ?? 0, currency);

            return webModel;
        }

        public static CartModule.Client.Model.Discount ToServiceModel(this Discount webModel)
        {
            var serviceModel = new CartModule.Client.Model.Discount();

            serviceModel.InjectFrom(webModel);

            serviceModel.Currency = webModel.Amount.Currency.Code;
            serviceModel.DiscountAmount = (double)webModel.Amount.Amount;

            return serviceModel;
        }

        public static Discount ToWebModel(this OrderModule.Client.Model.Discount serviceModel, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(serviceModel.Currency)) ?? new Currency(language, serviceModel.Currency);
            var webModel = new Discount(currency);

            webModel.InjectFrom(serviceModel);

            webModel.Amount = new Money(serviceModel.DiscountAmount ?? 0, currency);

            return webModel;
        }
    }
}
