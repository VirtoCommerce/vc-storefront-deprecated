using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.ProductRecommendationsModuleApi;
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
            string productRecomendationsCacheKey = string.Format("CognitiveRecomendationService.GetRecommendationIds:{0}:{1}:{2}:{3}", context.UserId, type, take, string.Join(":", context.ProductIds));
            IList<string> recomendedProductIds = await _cacheManager.GetAsync(productRecomendationsCacheKey, "ApiRegion", async () => await _productRecommendationsApi.Products.GetAsync());

            var productsCacheKey = string.Format("CognitiveRecomendationService.GetRecomendationProducts:{0}", string.Join(":", recomendedProductIds));
            Product[] products = await _cacheManager.GetAsync(productsCacheKey, "ApiRegion", async () => await _catalogSearchService.GetProductsAsync(recomendedProductIds.ToArray(), ItemResponseGroup.ItemLarge));
            
            return products;
        }
    }
}