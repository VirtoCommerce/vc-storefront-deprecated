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
using orderModel = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;

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

            return Json(new
            {
                Results = orders.ToArray(),
                TotalCount = orders.TotalItemCount
            });
        }

        // GET: storefrontapi/orders:number
        [HttpGet]
        public async Task<ActionResult> GetCustomerOrder(string number)
        {
            var retVal = await GetOrderByNumber(number);
            return Json(retVal, JsonRequestBehavior.AllowGet);
        }

        // GET: storefrontapi/orders/{number}/newpaymentdata
        [HttpGet]
        public async Task<ActionResult> GetNewPaymentData(string number)
        {
            var order = await GetOrderByNumber(number);

            var storeDto = await _storeApi.StoreModule.GetStoreByIdAsync(order.StoreId);
            var paymentMethods = storeDto.PaymentMethods
                                        .Where(x => x.IsActive.Value)
                                        .Select(x => x.ToPaymentMethod(order));

            var paymentDto = await _orderApi.OrderModule.GetNewPaymentAsync(order.Id);
            var payment = paymentDto.ToOrderInPayment(WorkContext.AllCurrencies, WorkContext.CurrentLanguage);

            return Json(new
            {
                Payment = payment,
                PaymentMethods = paymentMethods,
                Order = order
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: storefrontapi/orders/{number}/payments
        [HttpPost]
        public async Task<ActionResult> AddOrUpdatePayment(PaymentIn payment /*, orderModel.BankCardInfo bankCardInfo*/)
        {
            var number = (string)RouteData.Values["number"];
            orderModel.BankCardInfo bankCardInfo = null;
            orderModel.ProcessPaymentResult processingResult = null;

            if (payment.Sum.Amount == decimal.Zero)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Valid payment amount is required");
            }
            payment.CustomerId = WorkContext.CurrentCustomer.Id;
            payment.CustomerName = WorkContext.CurrentCustomer.FullName;
            var paymentDto = payment.ToOrderPaymentInDto();

            //Need lock to prevent concurrent access to same object
            using (await AsyncLock.GetLockByKey(GetAsyncLockKey(number, WorkContext)).LockAsync())
            {
                var orderDto = await GetOrderDtoByNumber(number);
                orderDto.InPayments.Add(paymentDto);
                await _orderApi.OrderModule.UpdateAsync(orderDto);

                orderDto = await _orderApi.OrderModule.GetByIdAsync(orderDto.Id);
                paymentDto = orderDto.InPayments
                    .Where(x => x.Sum.Value == paymentDto.Sum.Value && paymentDto.GatewayCode == x.GatewayCode)
                    .OrderByDescending(x => x.CreatedDate)
                    .First();

                processingResult = await _orderApi.OrderModule.ProcessOrderPaymentsAsync(orderDto.Id, paymentDto.Id, bankCardInfo);
            }

            return Json(new { orderProcessingResult = processingResult, paymentMethod = paymentDto.PaymentMethod });
        }


        private async Task<CustomerOrder> GetOrderByNumber(string number)
        {
            var order = await GetOrderDtoByNumber(number);

            WorkContext.CurrentOrder = order.ToCustomerOrder(WorkContext.AllCurrencies, WorkContext.CurrentLanguage);
            return WorkContext.CurrentOrder;
        }

        private async Task<orderModel.CustomerOrder> GetOrderDtoByNumber(string number)
        {
            var order = await _orderApi.OrderModule.GetByNumberAsync(number);

            if (order == null || order.CustomerId != WorkContext.CurrentCustomer.Id)
            {
                throw new System.Web.HttpException(404, "Order with number " + number + " not found (or not belongs to current user).");
            }

            return order;
        }

        private static string GetAsyncLockKey(string orderNumber, WorkContext ctx)
        {
            return string.Join(":", "Order", orderNumber, ctx.CurrentStore.Id, ctx.CurrentCustomer.Id);
        }
    }
}