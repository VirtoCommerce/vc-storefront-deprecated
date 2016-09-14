using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;
using pricingModel = VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class PriceConverter
    {
        public static ProductPrice ToWebModel(this pricingModel.Price price, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(price.Currency)) ?? new Currency(language, price.Currency);
            var retVal = new ProductPrice(currency);
            retVal.InjectFrom<NullableAndEnumValueInjecter>(price);
            retVal.Currency = currency;
            retVal.ListPrice = new Money(price.List ?? 0d, currency);
            retVal.SalePrice = price.Sale == null ? retVal.ListPrice : new Money(price.Sale ?? 0d, currency);
            retVal.ActiveDiscount = new Discount(currency);
            retVal.MinQuantity = price.MinQuantity;
            return retVal;
        }
    }
}
