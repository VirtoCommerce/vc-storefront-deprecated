using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using pricingModel = VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class PriceEvaluationContextConverter
    {
        public static pricingModel.PriceEvaluationContext ToServiceModel(this IEnumerable<Product> products, WorkContext workContext)
        {
            if (products == null)
            {
                throw new ArgumentNullException("products");
            }

            //Evaluate products prices
            var retVal = new pricingModel.PriceEvaluationContext
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
