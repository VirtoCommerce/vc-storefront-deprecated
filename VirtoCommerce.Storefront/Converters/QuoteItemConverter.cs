using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Quote;
using quoteModel = VirtoCommerce.Storefront.AutoRestClients.QuoteModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class QuoteItemConverter
    {
        public static QuoteItem ToWebModel(this quoteModel.QuoteItem serviceModel, Currency currency)
        {
            var webModel = new QuoteItem();

            webModel.InjectFrom<NullableAndEnumValueInjecter>(serviceModel);

            webModel.Currency = currency;
            webModel.ListPrice = new Money(serviceModel.ListPrice ?? 0, currency);
            webModel.SalePrice = new Money(serviceModel.SalePrice ?? 0, currency);

            if (serviceModel.ProposalPrices != null)
            {
                webModel.ProposalPrices = serviceModel.ProposalPrices.Select(pp => pp.ToWebModel(currency)).ToList();
            }

            if (serviceModel.SelectedTierPrice != null)
            {
                webModel.SelectedTierPrice = serviceModel.SelectedTierPrice.ToWebModel(currency);
            }

            return webModel;
        }

        public static quoteModel.QuoteItem ToQuoteServiceModel(this QuoteItem webModel)
        {
            var serviceModel = new quoteModel.QuoteItem();

            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(webModel);

            serviceModel.Currency = webModel.Currency.Code;
            serviceModel.ListPrice = (double)webModel.ListPrice.Amount;
            serviceModel.ProposalPrices = webModel.ProposalPrices.Select(pp => pp.ToQuoteServiceModel()).ToList();
            serviceModel.SalePrice = (double)webModel.SalePrice.Amount;

            if (webModel.SelectedTierPrice != null)
            {
                serviceModel.SelectedTierPrice = webModel.SelectedTierPrice.ToQuoteServiceModel();
            }

            return serviceModel;
        }
    }
}
