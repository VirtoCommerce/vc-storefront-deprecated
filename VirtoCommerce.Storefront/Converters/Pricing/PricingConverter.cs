using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Pricing;
using VirtoCommerce.Storefront.Model.Pricing.Factories;
using pricingDto = VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class PricingConverterExtension
    {
        public static PricingConverter PricingConverterInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PricingConverter>();
            }
        }

        public static pricingDto.PriceEvaluationContext ToPriceEvaluationContextDto(this IEnumerable<Product> products, WorkContext workContext)
        {
            return PricingConverterInstance.ToPriceEvaluationContextDto(products, workContext);
        }

        public static ProductPrice ToProductPrice(this pricingDto.Price priceDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            return PricingConverterInstance.ToProductPrice(priceDto, availCurrencies, language);
        }

        public static Pricelist ToPricelist(this pricingDto.Pricelist pricelistDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            return PricingConverterInstance.ToPricelist(pricelistDto, availCurrencies, language);
        }

        public static TierPrice ToTierPrice(this pricingDto.Price priceDto, Currency currency)
        {
            return PricingConverterInstance.ToTierPrice(priceDto, currency);
        }

    }

    public class PricingConverter
    {
        public virtual TierPrice ToTierPrice(pricingDto.Price priceDto, Currency currency)
        {
            var listPrice = new Money(priceDto.List ?? 0, currency);

            return new TierPrice(currency)
            {
                Quantity = priceDto.MinQuantity ?? 1,
                Price = priceDto.Sale.HasValue ? new Money(priceDto.Sale.Value, currency) : listPrice
            };
        }

        public virtual Pricelist ToPricelist(pricingDto.Pricelist pricelistDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(pricelistDto.Currency)) ?? new Currency(language, pricelistDto.Currency);
            var result = ServiceLocator.Current.GetInstance<PricingFactory>().CreatePricelist(currency);
            result.Id = pricelistDto.Id;
            return result;
        }

        public virtual ProductPrice ToProductPrice(pricingDto.Price priceDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(priceDto.Currency)) ?? new Currency(language, priceDto.Currency);
            var result = ServiceLocator.Current.GetInstance<PricingFactory>().CreateProductPrice(currency);
            result.InjectFrom<NullableAndEnumValueInjecter>(priceDto);
            result.Currency = currency;
            result.ListPrice = new Money(priceDto.List ?? 0d, currency);
            result.SalePrice = priceDto.Sale == null ? result.ListPrice : new Money(priceDto.Sale ?? 0d, currency);
            result.MinQuantity = priceDto.MinQuantity;
            return result;
        }

        public virtual pricingDto.PriceEvaluationContext ToPriceEvaluationContextDto(IEnumerable<Product> products, WorkContext workContext)
        {
            if (products == null)
            {
                throw new ArgumentNullException("products");
            }

            //Evaluate products prices
            var retVal = new pricingDto.PriceEvaluationContext
            {
                ProductIds = products.Select(p => p.Id).ToList(),
                PricelistIds = workContext.CurrentPricelists.Select(p => p.Id).ToList(),
                CatalogId = workContext.CurrentStore.Catalog,
                CustomerId = workContext.CurrentCustomer.Id,
                Language = workContext.CurrentLanguage.CultureName,
                CertainDate = workContext.StorefrontUtcNow,
                StoreId = workContext.CurrentStore.Id
            };

            return retVal;
        }
    }
}
