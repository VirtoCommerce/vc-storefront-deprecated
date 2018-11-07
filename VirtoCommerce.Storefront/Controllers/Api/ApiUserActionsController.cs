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
using VirtoCommerce.Storefront.Model.Interaction;

namespace VirtoCommerce.Storefront.Controllers.Api
{
    [HandleJsonError]
    public class ApiUserActionsController : StorefrontControllerBase
    {
        private readonly IProductRecommendationsModuleApiClient _productRecommendationsApi;

        public ApiUserActionsController(WorkContext workContext, IStorefrontUrlBuilder urlBuilder,
            IProductRecommendationsModuleApiClient productRecommendationsApi) : base(workContext, urlBuilder)
        {
            _productRecommendationsApi = productRecommendationsApi;
        }

        /// <summary>
        /// POST /storefrontapi/useractions/eventinfo
        /// Record user actions events in VC platform
        /// </summary>
        /// <param name="userSession"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> SaveEventInfo(UserSession userSession)
        {
            //TODO: need to replace to other special detected VC API for usage
            if (userSession.Interactions != null)
            {
                IList<UsageEvent> usageEvents = userSession.Interactions.Select(i => new UsageEvent
                {
                    EventType = i.Type,
                    ItemId = i.Content,
                    CreatedDate = i.CreatedAt,
                    CustomerId = WorkContext.CurrentCustomer.Id,
                    StoreId = WorkContext.CurrentStore.Id
                }).ToList();

                await _productRecommendationsApi.Recommendations.AddEventAsync(usageEvents);
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}
