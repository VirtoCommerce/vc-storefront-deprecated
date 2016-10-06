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
using pricingDTO = VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi.Models;

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

        public static pricingDTO.PriceEvaluationContext ToPriceEvaluationContextDTO(this IEnumerable<Product> products, WorkContext workContext)
        {
            return PricingConverterInstance.ToPriceEvaluationContextDTO(products, workContext);
        }

        public static ProductPrice ToProductPrice(this pricingDTO.Price priceDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            return PricingConverterInstance.ToProductPrice(priceDTO, availCurrencies, language);
        }

        public static Pricelist ToPricelist(this pricingDTO.Pricelist pricelistDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            return PricingConverterInstance.ToPricelist(pricelistDTO, availCurrencies, language);
        }

        public static TierPrice ToTierPrice(this pricingDTO.Price priceDTO, Currency currency)
        {
            return PricingConverterInstance.ToTierPrice(priceDTO, currency);
        }

    }

    public class PricingConverter
    {
        public virtual TierPrice ToTierPrice(pricingDTO.Price priceDTO, Currency currency)
        {
            var listPrice = new Money(priceDTO.List ?? 0, currency);

            return new TierPrice(currency)
            {
                Quantity = priceDTO.MinQuantity ?? 1,
                Price = priceDTO.Sale.HasValue ? new Money(priceDTO.Sale.Value, currency) : listPrice
            };
        }

        public virtual Pricelist ToPricelist(pricingDTO.Pricelist pricelistDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(pricelistDTO.Currency)) ?? new Currency(language, pricelistDTO.Currency);
            var result = ServiceLocator.Current.GetInstance<PricingFactory>().CreatePricelist(currency);
            result.Id = pricelistDTO.Id;
            return result;
        }

        public virtual ProductPrice ToProductPrice(pricingDTO.Price priceDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(priceDTO.Currency)) ?? new Currency(language, priceDTO.Currency);
            var result = ServiceLocator.Current.GetInstance<PricingFactory>().CreateProductPrice(currency);
            result.InjectFrom<NullableAndEnumValueInjecter>(priceDTO);
            result.Currency = currency;
            result.ListPrice = new Money(priceDTO.List ?? 0d, currency);
            result.SalePrice = priceDTO.Sale == null ? result.ListPrice : new Money(priceDTO.Sale ?? 0d, currency);
            result.MinQuantity = priceDTO.MinQuantity;
            return result;
        }

        public virtual pricingDTO.PriceEvaluationContext ToPriceEvaluationContextDTO(IEnumerable<Product> products, WorkContext workContext)
        {
            if (products == null)
            {
                throw new ArgumentNullException("products");
            }

            //Evaluate products prices
            var retVal = new pricingDTO.PriceEvaluationContext
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
