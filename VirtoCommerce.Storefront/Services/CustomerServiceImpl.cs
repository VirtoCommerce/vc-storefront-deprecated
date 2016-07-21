using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PagedList;
using VirtoCommerce.CustomerModule.Client.Api;
using VirtoCommerce.OrderModule.Client.Api;
using VirtoCommerce.QuoteModule.Client.Api;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Common.Events;
using VirtoCommerce.Storefront.Model.Customer;
using VirtoCommerce.Storefront.Model.Customer.Services;
using VirtoCommerce.Storefront.Model.Order;
using VirtoCommerce.Storefront.Model.Order.Events;
using VirtoCommerce.Storefront.Model.Quote;
using VirtoCommerce.Storefront.Model.Quote.Events;
using VirtoCommerce.StoreModule.Client.Api;

namespace VirtoCommerce.Storefront.Services
{
    public class CustomerServiceImpl : ICustomerService, IAsyncObserver<OrderPlacedEvent>, IAsyncObserver<QuoteRequestUpdatedEvent>
    {
        private readonly IVirtoCommerceCustomerApi _customerApi;
        private readonly IVirtoCommerceOrdersApi _orderApi;
        private readonly Func<WorkContext> _workContextFactory;
        private readonly IVirtoCommerceQuoteApi _quoteApi;
        private readonly IVirtoCommerceStoreApi _storeApi;
        private readonly ILocalCacheManager _cacheManager;
        private const string _customerOrdersCacheRegionFormat = "customer/{0}/orders/region";
        private const string _customerQuotesCacheRegionFormat = "customer/{0}/quotes/region";
        private const string _customerCacheKeyFormat = "customer/{0}";
        private const string _customerCacheRegionFormat = "customer/{0}/region";
        public CustomerServiceImpl(Func<WorkContext> workContextFactory, IVirtoCommerceCustomerApi customerApi, IVirtoCommerceOrdersApi orderApi,
            IVirtoCommerceQuoteApi quoteApi, IVirtoCommerceStoreApi storeApi, ILocalCacheManager cacheManager)
        {
            _workContextFactory = workContextFactory;
            _customerApi = customerApi;
            _orderApi = orderApi;
            _quoteApi = quoteApi;
            _storeApi = storeApi;
            _cacheManager = cacheManager;
        }

        #region ICustomerService Members

        public async Task<CustomerInfo> GetCustomerByIdAsync(string customerId)
        {
            var workContext = _workContextFactory();
            var retVal = await _cacheManager.GetAsync(string.Format(_customerCacheKeyFormat, customerId), string.Format(_customerCacheRegionFormat, customerId), async () =>
            {
                //TODO: Make parallels call
                var contact = await _customerApi.CustomerModuleGetContactByIdAsync(customerId);
                CustomerInfo result = null;
                if (contact != null)
                {
                    result = contact.ToWebModel();
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
            }

            return retVal;
        }

        public async Task CreateCustomerAsync(CustomerInfo customer)
        {
            var contact = customer.ToServiceModel();
            await _customerApi.CustomerModuleCreateContactAsync(contact);
        }

        public async Task UpdateCustomerAsync(CustomerInfo customer)
        {
            var contact = customer.ToServiceModel();
            await _customerApi.CustomerModuleUpdateContactAsync(contact);
            //Invalidate cache
            _cacheManager.ClearRegion(string.Format(_customerCacheRegionFormat, customer.Id));
        }

        public async Task<bool> CanLoginOnBehalfAsync(string storeId, string customerId)
        {
            var info = await _storeApi.StoreModuleGetLoginOnBehalfInfoAsync(storeId, customerId);
            return info.CanLoginOnBehalf == true;
        }

        public async Task<Vendor> GetVendorByIdAsync(string vendorId)
        {
            return (await _customerApi.CustomerModuleGetVendorByIdAsync(vendorId)).ToWebModel();
        }

        #endregion

        #region IObserver<CreateOrderEvent> Members
        public async Task OnNextAsync(OrderPlacedEvent eventArgs)
        {
            if (eventArgs.Order != null)
            {
                //Invalidate cache
                _cacheManager.ClearRegion(string.Format(_customerOrdersCacheRegionFormat, eventArgs.Order.CustomerId));

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

        public Task OnNextAsync(QuoteRequestUpdatedEvent quoteRequestCreatedEvent)
        {
            if (quoteRequestCreatedEvent.QuoteRequest != null)
            {
                //Invalidate cache
                _cacheManager.ClearRegion(string.Format(_customerQuotesCacheRegionFormat, quoteRequestCreatedEvent.QuoteRequest.CustomerId));
            }

            return Task.Factory.StartNew(() => { });
        }

        #endregion

        private IMutablePagedList<QuoteRequest> GetCustomerQuotes(CustomerInfo customer)
        {
            var workContext = _workContextFactory();
            Func<int, int, IEnumerable<SortInfo>, IPagedList<QuoteRequest>> quotesGetter = (pageNumber, pageSize, sortInfos) =>
            {
                var quoteSearchCriteria = new QuoteModule.Client.Model.QuoteRequestSearchCriteria
                {
                    Count = pageSize,
                    CustomerId = customer.Id,
                    Start = (pageNumber - 1) * pageSize,
                    StoreId = workContext.CurrentStore.Id
                };
                var cacheKey = "GetCustomerQuotes-" + quoteSearchCriteria.GetHashCode();
                var quoteRequestsResponse = _cacheManager.Get(cacheKey, string.Format(_customerQuotesCacheRegionFormat, customer.Id), () => _quoteApi.QuoteModuleSearch(quoteSearchCriteria));
                return new StaticPagedList<QuoteRequest>(quoteRequestsResponse.QuoteRequests.Select(x => x.ToWebModel(workContext.AllCurrencies, workContext.CurrentLanguage)),
                                                         pageNumber, pageSize, quoteRequestsResponse.TotalCount.Value);
            };

            return new MutablePagedList<QuoteRequest>(quotesGetter);
        }

        private IMutablePagedList<CustomerOrder> GetCustomerOrders(CustomerInfo customer)
        {
            var workContext = _workContextFactory();
            var orderSearchcriteria = new OrderModule.Client.Model.SearchCriteria
            {
                CustomerId = customer.Id,
                ResponseGroup = "full"
            };

            Func<int, int, IEnumerable<SortInfo>, IPagedList<CustomerOrder>> ordersGetter = (pageNumber, pageSize, sortInfos) =>
            {
                //TODO: add caching
                orderSearchcriteria.Start = (pageNumber - 1) * pageSize;
                orderSearchcriteria.Count = pageSize;             
                var cacheKey = "GetCustomerOrders-" + orderSearchcriteria.GetHashCode();
                var ordersResponse = _cacheManager.Get(cacheKey, string.Format(_customerOrdersCacheRegionFormat, customer.Id), () => _orderApi.OrderModuleSearch(orderSearchcriteria));
                return new StaticPagedList<CustomerOrder>(ordersResponse.CustomerOrders.Select(x => x.ToWebModel(workContext.AllCurrencies, workContext.CurrentLanguage)), pageNumber, pageSize,
                                                          ordersResponse.TotalCount.Value);
            };
            return new MutablePagedList<CustomerOrder>(ordersGetter);
        }
    }
}
