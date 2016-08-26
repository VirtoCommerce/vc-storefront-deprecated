using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Pricing.Services;
using coreModel = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;
using pricingModel = VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi.Models;

namespace VirtoCommerce.Storefront.Services
{
    public class PricingServiceImpl : IPricingService
    {
        private readonly IPricingModuleApiClient _pricingApi;
        private readonly ICoreModuleApiClient _commerceApi;
        private readonly Func<WorkContext> _workContextFactory;

        public PricingServiceImpl(Func<WorkContext> workContextFactory, IPricingModuleApiClient pricingApi, ICoreModuleApiClient commerceApi)
        {
            _pricingApi = pricingApi;
            _commerceApi = commerceApi;
            _workContextFactory = workContextFactory;
        }

        #region IPricingService Members

        public async Task EvaluateProductPricesAsync(ICollection<Product> products)
        {
            var workContext = _workContextFactory();
            //Evaluate products prices
            var evalContext = products.ToServiceModel(workContext);

            var pricesResponse = await _pricingApi.PricingModule.EvaluatePricesAsync(evalContext);
            ApplyProductPricesInternal(products, pricesResponse);
            //Evaluate product taxes
            var taxEvalContext = workContext.ToTaxEvaluationContext(products);
            var taxRates = await _commerceApi.Commerce.EvaluateTaxesAsync(workContext.CurrentStore.Id, taxEvalContext);
            ApplyProductTaxInternal(products, taxRates);
        }

        public void EvaluateProductPrices(ICollection<Product> products)
        {
            var workContext = _workContextFactory();
            //Evaluate products prices
            var evalContext = products.ToServiceModel(workContext);

            var pricesResponse = _pricingApi.PricingModule.EvaluatePrices(evalContext);
            ApplyProductPricesInternal(products, pricesResponse);

            //Evaluate product taxes
            var taxEvalContext = workContext.ToTaxEvaluationContext(products);
            var taxRates = _commerceApi.Commerce.EvaluateTaxes(workContext.CurrentStore.Id, taxEvalContext);
            ApplyProductTaxInternal(products, taxRates);
        }

        #endregion

        private void ApplyProductTaxInternal(IEnumerable<Product> products, IEnumerable<coreModel.TaxRate> taxes)
        {
            var workContext = _workContextFactory();
            var taxRates = taxes.Select(x => x.ToWebModel(workContext.CurrentCurrency)).ToList();

            foreach (var product in products)
            {
                product.ApplyTaxRates(taxRates);
            }
        }

        private void ApplyProductPricesInternal(IEnumerable<Product> products, IList<pricingModel.Price> prices)
        {
            var workContext = _workContextFactory();

            foreach (var product in products)
            {
                var productPrices = prices.Where(x => x.ProductId == product.Id)
                                          .Select(x => x.ToWebModel(workContext.AllCurrencies, workContext.CurrentLanguage));
                product.ApplyPrices(productPrices, workContext.CurrentCurrency, workContext.CurrentStore.Currencies);
            }
        }
    }
}
