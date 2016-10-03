using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Marketing.Services;
using VirtoCommerce.Storefront.Model.Pricing.Services;
using VirtoCommerce.Storefront.Model.Tax.Services;
using pricingModel = VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi.Models;

namespace VirtoCommerce.Storefront.Services
{
    public class PricingServiceImpl : IPricingService
    {
        private readonly Func<WorkContext> _workContextFactory;
        private readonly IPricingModuleApiClient _pricingApi;
        private readonly ITaxEvaluator _taxEvaluator;
        private readonly IPromotionEvaluator _promotionEvaluator;
        private readonly PromotionEvaluationContextConverter _promotionEvaluationContextConverter;

        public PricingServiceImpl(
            Func<WorkContext> workContextFactory,
            IPricingModuleApiClient pricingApi,
            ITaxEvaluator taxEvaluator,
            IPromotionEvaluator promotionEvaluator,
            PromotionEvaluationContextConverter promotionEvaluationContextConverter)
        {
            _pricingApi = pricingApi;
            _taxEvaluator = taxEvaluator;
            _workContextFactory = workContextFactory;
            _promotionEvaluator = promotionEvaluator;
            _promotionEvaluationContextConverter = promotionEvaluationContextConverter;
        }

        #region IPricingService Members

        public virtual async Task EvaluateProductPricesAsync(ICollection<Product> products)
        {
            var workContext = _workContextFactory();
            //Evaluate products prices
            var evalContext = products.ToServiceModel(workContext);

            var pricesResponse = await _pricingApi.PricingModule.EvaluatePricesAsync(evalContext);
            ApplyProductPricesInternal(products, pricesResponse);
            //Evaluate product discounts
            var promoEvalContext = _promotionEvaluationContextConverter.ToPromotionEvaluationContext(workContext, products);
            await _promotionEvaluator.EvaluateDiscountsAsync(promoEvalContext, products);
            //Evaluate product taxes
            var taxEvalContext = workContext.ToTaxEvaluationContext(products);
            await _taxEvaluator.EvaluateTaxesAsync(taxEvalContext, products);
        }

        public virtual void EvaluateProductPrices(ICollection<Product> products)
        {
            var workContext = _workContextFactory();
            //Evaluate products prices
            var evalContext = products.ToServiceModel(workContext);

            var pricesResponse = _pricingApi.PricingModule.EvaluatePrices(evalContext);
            ApplyProductPricesInternal(products, pricesResponse);

            //Evaluate product taxes
            var taxEvalContext = workContext.ToTaxEvaluationContext(products);
            _taxEvaluator.EvaluateTaxes(taxEvalContext, products);
        }

        #endregion

        protected virtual void ApplyProductPricesInternal(IEnumerable<Product> products, IList<pricingModel.Price> prices)
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
