using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.SubscriptionModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Converters.Subscriptions;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Subscriptions;
using subscriptionDto = VirtoCommerce.Storefront.AutoRestClients.SubscriptionModuleApi.Models;

namespace VirtoCommerce.Storefront.Controllers.Api
{
    [HandleJsonError]
    public class ApiSubscriptionController : StorefrontControllerBase
    {
        private readonly ISubscriptionModuleApiClient _subscriptionApi;

        public ApiSubscriptionController(WorkContext workContext, IStorefrontUrlBuilder urlBuilder, ISubscriptionModuleApiClient subscriptionApi)
            : base(workContext, urlBuilder)
        {
            _subscriptionApi = subscriptionApi;
        }

        // POST: storefrontapi/subscriptions/search
        [HttpPost]
        public async Task<ActionResult> SearchCustomerSubscriptions(SubscriptionSearchCriteria searchCriteria)
        {
            if(searchCriteria == null)
            {
                searchCriteria = new SubscriptionSearchCriteria();
            }
            //Does not allow to see a other subscriptions
            searchCriteria.CustomerId = WorkContext.CurrentCustomer.Id;

            var result = await _subscriptionApi.SubscriptionModule.SearchSubscriptionsAsync(searchCriteria.ToSearchCriteriaDto());
        
            return Json(new
            {
                Results = result.Subscriptions.Select(x=>x.ToSubscription(WorkContext.AllCurrencies, WorkContext.CurrentLanguage)).ToArray(),
                TotalCount = result.TotalCount,                
            });
        }

        // GET: storefrontapi/subscriptions/{number}
        [HttpGet]
        public async Task<ActionResult> GetCustomerSubscription(string number)
        {
            var retVal = await GetSubscriptionByNumberAsync(number);
            return Json(retVal, JsonRequestBehavior.AllowGet);
        }

        // POST: storefrontapi/subscriptions/{number}/cancel
        [HttpPost]
        public async Task<ActionResult> CancelSubscription(SubscriptionCancelRequest cancelRequest)
        {
            var subscription = await GetSubscriptionByNumberAsync(cancelRequest.Number);
            var retVal = (await _subscriptionApi.SubscriptionModule.CancelSubscriptionAsync(new subscriptionDto.SubscriptionCancelRequest
            {
                CancelReason = cancelRequest.CancelReason,
                SubscriptionId = subscription.Id
            })).ToSubscription(WorkContext.AllCurrencies, WorkContext.CurrentLanguage);

            return Json(retVal, JsonRequestBehavior.AllowGet);
        }

        private async Task<Subscription> GetSubscriptionByNumberAsync(string number)
        {
            var criteria = new subscriptionDto.SubscriptionSearchCriteria
            {
                Number = number,
                ResponseGroup = SubscriptionResponseGroup.Full.ToString()
            };
            var retVal = (await _subscriptionApi.SubscriptionModule.SearchSubscriptionsAsync(criteria))
                                                .Subscriptions
                                                .Select(x=>x.ToSubscription(WorkContext.AllCurrencies, WorkContext.CurrentLanguage))
                                                .FirstOrDefault();

            if (retVal == null || retVal.CustomerId != WorkContext.CurrentCustomer.Id)
            {
                throw new System.Web.HttpException(404, "Subscription with number " + number + " not found (or not belongs to current user).");
            }
            return retVal;
        }      
    }
}