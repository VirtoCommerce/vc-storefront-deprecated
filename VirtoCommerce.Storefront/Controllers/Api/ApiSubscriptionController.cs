using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.StoreModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.SubscriptionModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Converters.Subscriptions;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Order;
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

        // GET: storefrontapi/subscriptions
        [HttpGet]
        public ActionResult GetCustomerSubscriptions(int pageNumber, int pageSize, IEnumerable<SortInfo> sortInfos)
        {
            var subscriptions = WorkContext.CurrentCustomer.Subscriptions;
            subscriptions.Slice(pageNumber, pageSize, sortInfos);

            return Json(new
            {
                Results = subscriptions.ToArray(),
                TotalCount = subscriptions.TotalItemCount,                
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
            var retVal = (await _subscriptionApi.SubscriptionModule.CancelSubscriptionAsync(new AutoRestClients.SubscriptionModuleApi.Models.SubscriptionCancelRequest
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
                Number = null,
                ResponseGroup = SubscriptionResponseGroup.Full.ToString()
            };
            var retVal = (await _subscriptionApi.SubscriptionModule.SearchAsync(criteria))
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