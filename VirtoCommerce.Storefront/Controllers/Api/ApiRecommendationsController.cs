using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using VirtoCommerce.Storefront.AutoRestClients.ProductRecommendationsModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.ProductRecommendationsModuleApi.Models;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Recommendations;

namespace VirtoCommerce.Storefront.Controllers.Api
{
    [HandleJsonError]
    public class ApiRecommendationsController : StorefrontControllerBase
    {
        private readonly Func<string, IRecommendationsService> _recommendationServiceFactory;
        private readonly IProductRecommendationsModuleApiClient _productRecommendationsApi;

        public ApiRecommendationsController(WorkContext workContext, IStorefrontUrlBuilder urlBuilder,
            Func<string, IRecommendationsService> recommendationsServiceFactory,
            IProductRecommendationsModuleApiClient productRecommendationsApi) : base(workContext, urlBuilder)
        {
            _recommendationServiceFactory = recommendationsServiceFactory;
            _productRecommendationsApi = productRecommendationsApi;
        }

        [HttpPost]
        public async Task<ActionResult> GetRecommendations(string provider, string type, int skip, int take, string[] productIds)
        {
            var recommendationService = _recommendationServiceFactory(provider);

            var context = new RecommendationsContext()
            {
                StoreId = WorkContext.CurrentStore.Id,
                CustomerId = WorkContext.CurrentCustomer.Id,
                ProductIds = productIds
            };

            var result = await recommendationService.GetRecommendationsAsync(context, type, skip, take);

            return Json(result);
        }

        [HttpPost]
        public async Task<ActionResult> SaveEventInfo(string[] productIds, string eventType)
        {
            if (WorkContext.CurrentCustomer.IsRegisteredUser)
            {
                IList<UsageEvent> usageEvents = productIds.Select(id => new UsageEvent
                {
                    EventType = eventType,
                    ItemId = id,
                    CustomerId = WorkContext.CurrentCustomer.Id,
                    StoreId = WorkContext.CurrentStore.Id
                }).ToList();

                await _productRecommendationsApi.Recommendations.AddEventAsync(usageEvents);
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}