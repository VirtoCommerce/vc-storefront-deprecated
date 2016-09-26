using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using pricingModel = VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi.Models;
using quoteModel = VirtoCommerce.Storefront.AutoRestClients.QuoteModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class TierPriceConverter
    {
        public static TierPrice ToTierPrice(this pricingModel.Price serviceModel, Currency currency)
        {
            var listPrice = new Money(serviceModel.List ?? 0, currency);

            return new TierPrice(currency)
            {
                Quantity = serviceModel.MinQuantity ?? 1,
                Price = serviceModel.Sale.HasValue ? new Money(serviceModel.Sale.Value, currency) : listPrice
            };
        }

        public static TierPrice ToWebModel(this quoteModel.TierPrice serviceModel, Currency currency)
        {
            var webModel = new TierPrice(currency);
            webModel.InjectFrom<NullableAndEnumValueInjecter>(serviceModel);
            webModel.Price = new Money(serviceModel.Price ?? 0, currency);
            return webModel;
        }

        public static quoteModel.TierPrice ToQuoteServiceModel(this TierPrice webModel)
        {
            var serviceModel = new quoteModel.TierPrice();
            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(webModel);
            serviceModel.Price = (double)webModel.Price.Amount;
            return serviceModel;
        }
    }
}
