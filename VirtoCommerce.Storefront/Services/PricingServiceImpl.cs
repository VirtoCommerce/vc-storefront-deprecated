using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Inventory.Services;
using VirtoCommerce.Storefront.Model.Marketing.Services;
using VirtoCommerce.Storefront.Model.Pricing.Services;
using VirtoCommerce.Storefront.Model.Tax.Services;
using pricingModel = VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi.Models;

namespace VirtoCommerce.Storefront.Services
{
    public class PricingServiceImpl : IPricingService
    {
        private readonly IPricingModuleApiClient _pricingApi;
        private readonly ITaxEvaluator _taxEvaluator;
        private readonly IPromotionEvaluator _promotionEvaluator;
        private readonly IInventoryService _inventoryService;

        public PricingServiceImpl(
            IPricingModuleApiClient pricingApi,
            ITaxEvaluator taxEvaluator,
            IPromotionEvaluator promotionEvaluator,
            IInventoryService inventoryService)
        {
            _pricingApi = pricingApi;
            _taxEvaluator = taxEvaluator;
            _promotionEvaluator = promotionEvaluator;
            _inventoryService = inventoryService;
        }

        #region IPricingService Members

        public virtual async Task EvaluateProductPricesAsync(ICollection<Product> products, WorkContext workContext)
        {
            //Evaluate products prices
            var evalContext = workContext.ToPriceEvaluationContextDto(products);

            var pricesResponse = await _pricingApi.PricingModule.EvaluatePricesAsync(evalContext);
            ApplyProductPricesInternal(products, pricesResponse, workContext);

            //Evaluate product discounts
            //Fill product inventories for getting InStockQuantity data for promotion evaluation
            await _inventoryService.EvaluateProductInventoriesAsync(products, workContext);
            var promoEvalContext = workContext.ToPromotionEvaluationContext(products);
            await _promotionEvaluator.EvaluateDiscountsAsync(promoEvalContext, products);

            //Evaluate product taxes
            var taxEvalContext = workContext.ToTaxEvaluationContext(products);
            await _taxEvaluator.EvaluateTaxesAsync(taxEvalContext, products);
        }   

        #endregion

        protected virtual void ApplyProductPricesInternal(IEnumerable<Product> products, IList<pricingModel.Price> prices, WorkContext workContext)
        {
            foreach (var product in products)
            {
                var productPrices = prices.Where(x => x.ProductId == product.Id)
                                          .Select(x => x.ToProductPrice(workContext.AllCurrencies, workContext.CurrentLanguage));
                product.ApplyPrices(productPrices, workContext.CurrentCurrency, workContext.CurrentStore.Currencies);
            }
        }
    }
}
