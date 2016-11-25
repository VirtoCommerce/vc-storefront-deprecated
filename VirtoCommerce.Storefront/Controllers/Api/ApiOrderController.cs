using PagedList;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.StoreModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Order;

namespace VirtoCommerce.Storefront.Controllers.Api
{
    [HandleJsonError]
    public class ApiOrderController : StorefrontControllerBase
    {
        private readonly IOrdersModuleApiClient _orderApi;
        private readonly IStoreModuleApiClient _storeApi;

        public ApiOrderController(WorkContext workContext, IStorefrontUrlBuilder urlBuilder, IOrdersModuleApiClient orderApi, IStoreModuleApiClient storeApi)
            : base(workContext, urlBuilder)
        {
            _orderApi = orderApi;
            _storeApi = storeApi;
        }

        // GET: storefrontapi/orders
        [HttpGet]
        public ActionResult GetCustomerOrders(int pageNumber, int pageSize, IEnumerable<SortInfo> sortInfos)
        {
            var orders = WorkContext.CurrentCustomer.Orders;
            orders.Slice(pageNumber, pageSize, sortInfos);
            var retVal = new StaticPagedList<CustomerOrder>(orders.Select(x => x), orders);

            return Json(new
            {
                Results = retVal,
                TotalCount = retVal.TotalItemCount
            });
        }

        // GET: storefrontapi/orders:number
        [HttpGet]
        public ActionResult GetCustomerOrder(string number)
        {
            var retVal = GetOrderByNumber(number);
            return Json(retVal, JsonRequestBehavior.AllowGet);
        }

        // GET: storefrontapi/orders/{number}/paymentmethods
        [HttpGet]
        public async Task<ActionResult> GetAvailPaymentMethods(string number)
        {
            var order = GetOrderByNumber(number);
            var store = await _storeApi.StoreModule.GetStoreByIdAsync(order.StoreId);
            var paymentMethods = store.PaymentMethods.Where(x => x.IsActive.Value).ToList();
            return Json(paymentMethods, JsonRequestBehavior.AllowGet);
        }

        // POST: storefrontapi/orders/{number}/payments
        [HttpPost]
        public async Task<ActionResult> AddOrUpdatePayment(string number, PaymentIn payment)
        {
            var order = GetOrderByNumber(number);
            //Need lock to prevent concurrent access to same object
            using (await AsyncLock.GetLockByKey(GetAsyncLockKey(order)).LockAsync())
            {
                var orderDto = await _orderApi.OrderModule.GetByNumberAsync(number);
                orderDto.InPayments.Add(payment.ToOrderPaymentInDto());
                await _orderApi.OrderModule.UpdateAsync(orderDto);
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        private CustomerOrder GetOrderByNumber(string number)
        {
            return WorkContext.CurrentCustomer.Orders.FirstOrDefault(x => x.Number == number);
        }

        private static string GetAsyncLockKey(CustomerOrder entry)
        {
            return string.Join(":", "Order", entry.Id, entry.StoreId, entry.CustomerId);
        }
    }
}