using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.ProductRecommendationsModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.ProductRecommendationsModuleApi.Models;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Recommendations;
using VirtoCommerce.Storefront.Model.Services;

namespace VirtoCommerce.Storefront.Services.Recommendations
{
    public class CognitiveRecommendationsService : IRecommendationsService
    {
        private readonly Func<WorkContext> _workContextFactory;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly IProductRecommendationsModuleApiClient _productRecommendationsApi;
        private readonly ILocalCacheManager _cacheManager;

        public CognitiveRecommendationsService(
            Func<WorkContext> workContextFactory,
            ICatalogSearchService catalogSearchService,
            IProductRecommendationsModuleApiClient productRecommendationsApi,
            ILocalCacheManager cacheManager)
        {
            _workContextFactory = workContextFactory;
            _catalogSearchService = catalogSearchService;
            _productRecommendationsApi = productRecommendationsApi;
            _cacheManager = cacheManager;
        }

        public async Task<Product[]> GetRecommendationsAsync(RecommendationsContext context, string type, int skip, int take)
        {
            var result = new List<Product>();

            var customerRecommendationsContext = new CustomerRecommendationsContext
            {
                StoreId = context.StoreId,
                CustomerId = context.CustomerId,
                ProductIds = context.ProductIds,
                NumberOfResults = take
            };

            var recommendedProductIds = await _productRecommendationsApi.Recommendations.GetCustomerRecommendationsAsync(customerRecommendationsContext);
            if (recommendedProductIds != null)
            {
                result.AddRange(await _catalogSearchService.GetProductsAsync(recommendedProductIds.ToArray(), ItemResponseGroup.ItemLarge));
            }

            return result.ToArray();
        }
    }
}