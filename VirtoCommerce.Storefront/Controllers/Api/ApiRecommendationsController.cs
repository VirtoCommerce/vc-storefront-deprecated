using System;
using System.Threading.Tasks;
using System.Web.Mvc;
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

        public ApiRecommendationsController(WorkContext workContext, IStorefrontUrlBuilder urlBuilder, Func<string, IRecommendationsService> recommendationsServiceFactory)
            : base(workContext, urlBuilder)
        {
            _recommendationServiceFactory = recommendationsServiceFactory;
        }

        [HttpPost]
        public async Task<ActionResult> GetRecommendations(string provider, string type, int skip, int take, string[] productIds)
        {
            var recommendationService =_recommendationServiceFactory(provider);

            var context = new RecommendationsContext()
            {
                UserId = WorkContext.CurrentCustomer.UserId,
                ProductIds = productIds
            };

            var result = await recommendationService.GetRecommendationsAsync(context, type, skip, take);

            return Json(result);
        }
    }
}