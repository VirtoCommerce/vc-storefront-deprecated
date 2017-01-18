using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PagedList;
using VirtoCommerce.Storefront.AutoRestClients.CustomerModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.QuoteModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.StoreModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.SubscriptionModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Converters.Subscriptions;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Common.Events;
using VirtoCommerce.Storefront.Model.Customer;
using VirtoCommerce.Storefront.Model.Customer.Services;
using VirtoCommerce.Storefront.Model.Order;
using VirtoCommerce.Storefront.Model.Order.Events;
using VirtoCommerce.Storefront.Model.Quote;
using VirtoCommerce.Storefront.Model.Quote.Events;
using VirtoCommerce.Storefront.Model.Stores;
using VirtoCommerce.Storefront.Model.Subscriptions;
using customerDto = VirtoCommerce.Storefront.AutoRestClients.CustomerModuleApi.Models;
using orderDto = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;
using quoteDto = VirtoCommerce.Storefront.AutoRestClients.QuoteModuleApi.Models;
using subscriptionDto = VirtoCommerce.Storefront.AutoRestClients.SubscriptionModuleApi.Models;

namespace VirtoCommerce.Storefront.Services
{
    public class CustomerServiceImpl : ICustomerService, IAsyncObserver<OrderPlacedEvent>, IAsyncObserver<QuoteRequestUpdatedEvent>
    {
        private readonly ICustomerModuleApiClient _customerApi;
        private readonly IOrdersModuleApiClient _orderApi;
        private readonly Func<WorkContext> _workContextFactory;
        private readonly IQuoteModuleApiClient _quoteApi;
        private readonly IStoreModuleApiClient _storeApi;
        private readonly ISubscriptionModuleApiClient _subscriptionApi;
        private readonly ILocalCacheManager _cacheManager;
        private const string _customerOrdersCacheRegionFormat = "customer/{0}/orders/region";
        private const string _customerQuotesCacheRegionFormat = "customer/{0}/quotes/region";
        private const string _customerSubscriptionCacheRegionFormat = "customer/{0}/subscriptions/region";
        private const string _customerCacheKeyFormat = "customer/{0}";
        private const string _customerCacheRegionFormat = "customer/{0}/region";
        public CustomerServiceImpl(Func<WorkContext> workContextFactory, ICustomerModuleApiClient customerApi, IOrdersModuleApiClient orderApi,
            IQuoteModuleApiClient quoteApi, IStoreModuleApiClient storeApi, ISubscriptionModuleApiClient subscriptionApi, ILocalCacheManager cacheManager)
        {
            _workContextFactory = workContextFactory;
            _customerApi = customerApi;
            _orderApi = orderApi;
            _quoteApi = quoteApi;
            _storeApi = storeApi;
            _cacheManager = cacheManager;
            _subscriptionApi = subscriptionApi;
        }

        #region ICustomerService Members

        public virtual async Task<CustomerInfo> GetCustomerByIdAsync(string customerId)
        {
            var workContext = _workContextFactory();
            var retVal = await _cacheManager.GetAsync(string.Format(_customerCacheKeyFormat, customerId), string.Format(_customerCacheRegionFormat, customerId), async () =>
            {
                //TODO: Make parallels call
                var contact = await _customerApi.CustomerModule.GetContactByIdAsync(customerId);
                CustomerInfo result = null;
                if (contact != null)
                {
                    result = contact.ToCustomerInfo();
                }
                return result;
            });

            if (retVal != null)
            {
                retVal = retVal.JsonClone();
                retVal.Orders = GetCustomerOrders(retVal);
                if (workContext.CurrentStore.QuotesEnabled)
                {
                    retVal.QuoteRequests = GetCustomerQuotes(retVal);
                }
                retVal.Subscriptions = GetCustomerSubscriptions(retVal);
            }

            return retVal;
        }

        public virtual async Task CreateCustomerAsync(CustomerInfo customer)
        {
            var contact = customer.ToCustomerContactDto();
            await _customerApi.CustomerModule.CreateContactAsync(contact);
        }

        public virtual async Task UpdateCustomerAsync(CustomerInfo customer)
        {
            var contact = customer.ToCustomerContactDto();
            await _customerApi.CustomerModule.UpdateContactAsync(contact);
            //Invalidate cache
            _cacheManager.ClearRegion(string.Format(_customerCacheRegionFormat, customer.Id));
        }

        public virtual async Task<bool> CanLoginOnBehalfAsync(string storeId, string customerId)
        {
            var info = await _storeApi.StoreModule.GetLoginOnBehalfInfoAsync(storeId, customerId);
            return info.CanLoginOnBehalf == true;
        }

        public virtual async Task<Vendor[]> GetVendorsByIdsAsync(Store store, Language language, params string[] vendorIds)
        {
            return (await _customerApi.CustomerModule.GetVendorsByIdsAsync(vendorIds)).Select(x => x.ToVendor(language, store)).ToArray();
        }

        public virtual Vendor[] GetVendorsByIds(Store store, Language language, params string[] vendorIds)
        {
            var retVal = _customerApi.CustomerModule.GetVendorsByIds(vendorIds).Select(x => x.ToVendor(language, store)).ToArray();
            return retVal;
        }

        public virtual IPagedList<Vendor> SearchVendors(string keyword, int pageNumber, int pageSize, IEnumerable<SortInfo> sortInfos)
        {
            // TODO: implement indexed search for vendors
            var workContext = _workContextFactory();
            var criteria = new customerDto.MembersSearchCriteria
            {
                Keyword = keyword,
                DeepSearch = true,
                Skip = (pageNumber - 1) * pageSize,
                Take = pageSize
            };

            if (!sortInfos.IsNullOrEmpty())
            {
                criteria.Sort = SortInfo.ToString(sortInfos);
            }
            var result = _customerApi.CustomerModule.SearchVendors(criteria);
            return new StaticPagedList<Vendor>(result.Vendors.Select(x => x.ToVendor(workContext.CurrentLanguage, workContext.CurrentStore)), pageNumber, pageSize, result.TotalCount.Value);
        }
        #endregion

        #region IObserver<CreateOrderEvent> Members
        public virtual async Task OnNextAsync(OrderPlacedEvent eventArgs)
        {
            if (eventArgs.Order != null)
            {
                //Invalidate cache
                _cacheManager.ClearRegion(string.Format(_customerOrdersCacheRegionFormat, eventArgs.Order.CustomerId));
                _cacheManager.ClearRegion(string.Format(_customerSubscriptionCacheRegionFormat, eventArgs.Order.CustomerId));

                var workContext = _workContextFactory();
                //Add addresses to contact profile
                if (workContext.CurrentCustomer.IsRegisteredUser)
                {
                    workContext.CurrentCustomer.Addresses.AddRange(eventArgs.Order.Addresses);
                    workContext.CurrentCustomer.Addresses.AddRange(eventArgs.Order.Shipments.Select(x => x.DeliveryAddress));

                    foreach (var address in workContext.CurrentCustomer.Addresses)
                    {
                        address.Name = address.ToString();
                    }

                    await UpdateCustomerAsync(workContext.CurrentCustomer);
                }

            }
        }
        #endregion

        #region IAsyncObserver<QuoteRequestUpdatedEvent> Members

        public virtual Task OnNextAsync(QuoteRequestUpdatedEvent quoteRequestCreatedEvent)
        {
            if (quoteRequestCreatedEvent.QuoteRequest != null)
            {
                //Invalidate cache
                _cacheManager.ClearRegion(string.Format(_customerQuotesCacheRegionFormat, quoteRequestCreatedEvent.QuoteRequest.CustomerId));
            }

            return Task.Factory.StartNew(() => { });
        }

        #endregion

        protected virtual IMutablePagedList<QuoteRequest> GetCustomerQuotes(CustomerInfo customer)
        {
            var workContext = _workContextFactory();
            Func<int, int, IEnumerable<SortInfo>, IPagedList<QuoteRequest>> quotesGetter = (pageNumber, pageSize, sortInfos) =>
            {
                var quoteSearchCriteria = new quoteDto.QuoteRequestSearchCriteria
                {
                    Take = pageSize,
                    CustomerId = customer.Id,
                    Skip = (pageNumber - 1) * pageSize,
                    StoreId = workContext.CurrentStore.Id
                };
                var cacheKey = "GetCustomerQuotes-" + quoteSearchCriteria.GetHashCode();
                var quoteRequestsResponse = _cacheManager.Get(cacheKey, string.Format(_customerQuotesCacheRegionFormat, customer.Id), () => _quoteApi.QuoteModule.Search(quoteSearchCriteria));
                return new StaticPagedList<QuoteRequest>(quoteRequestsResponse.QuoteRequests.Select(x => x.ToQuoteRequest(workContext.AllCurrencies, workContext.CurrentLanguage)),
                                                         pageNumber, pageSize, quoteRequestsResponse.TotalCount.Value);
            };

            return new MutablePagedList<QuoteRequest>(quotesGetter, 1, QuoteSearchCriteria.DefaultPageSize);
        }

        protected virtual IMutablePagedList<CustomerOrder> GetCustomerOrders(CustomerInfo customer)
        {
            var workContext = _workContextFactory();
            var orderSearchcriteria = new orderDto.CustomerOrderSearchCriteria
            {
                CustomerId = customer.Id,
                ResponseGroup = "full"
            };

            Func<int, int, IEnumerable<SortInfo>, IPagedList<CustomerOrder>> ordersGetter = (pageNumber, pageSize, sortInfos) =>
            {
                //TODO: add caching
                orderSearchcriteria.Skip = (pageNumber - 1) * pageSize;
                orderSearchcriteria.Take = pageSize;
                var cacheKey = "GetCustomerOrders-" + orderSearchcriteria.GetHashCode();
                var ordersResponse = _cacheManager.Get(cacheKey, string.Format(_customerOrdersCacheRegionFormat, customer.Id), () => _orderApi.OrderModule.Search(orderSearchcriteria));
                return new StaticPagedList<CustomerOrder>(ordersResponse.CustomerOrders.Select(x => x.ToCustomerOrder(workContext.AllCurrencies, workContext.CurrentLanguage)), pageNumber, pageSize,
                                                          ordersResponse.TotalCount.Value);
            };
            return new MutablePagedList<CustomerOrder>(ordersGetter, 1, OrderSearchCriteria.DefaultPageSize);
        }

        protected virtual IMutablePagedList<Subscription> GetCustomerSubscriptions(CustomerInfo customer)
        {
            var workContext = _workContextFactory();
            var  subscriptionSearchcriteria = new subscriptionDto.SubscriptionSearchCriteria
            {
                CustomerId = customer.Id,
                ResponseGroup = SubscriptionResponseGroup.Full.ToString()               
            };

            Func<int, int, IEnumerable<SortInfo>, IPagedList<Subscription>> subscriptionGetter = (pageNumber, pageSize, sortInfos) =>
            {
                subscriptionSearchcriteria.Skip = (pageNumber - 1) * pageSize;
                subscriptionSearchcriteria.Take = pageSize;
                var cacheKey = "GetSubscriptions-" + subscriptionSearchcriteria.GetHashCode();
                var retVal = _cacheManager.Get(cacheKey, string.Format(_customerSubscriptionCacheRegionFormat, customer.Id), () =>
                {
                    var searchResult = _subscriptionApi.SubscriptionModule.SearchSubscriptions(subscriptionSearchcriteria);
                    return new StaticPagedList<Subscription>(searchResult.Subscriptions.Select(x => x.ToSubscription(workContext.AllCurrencies, workContext.CurrentLanguage)), pageNumber, pageSize,
                                                             searchResult.TotalCount.Value);
                });
               return retVal;
            };
            return new MutablePagedList<Subscription>(subscriptionGetter, 1, SubscriptionSearchCriteria.DefaultPageSize);
        }
    }
}
