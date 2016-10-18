using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Common.Exceptions;
using orderModel = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;

namespace VirtoCommerce.Storefront.Controllers
{
    public class CartController : StorefrontControllerBase
    {
        private readonly IOrdersModuleApiClient _orderApi;

        public CartController(WorkContext workContext, IOrdersModuleApiClient orderApi, IStorefrontUrlBuilder urlBuilder)
            : base(workContext, urlBuilder)
        {
            _orderApi = orderApi;
        }

        // GET: /cart
        [HttpGet]
        public ActionResult Index()
        {
            return View("cart", WorkContext);
        }


        // GET: /cart/checkout
        [HttpGet]
        public ActionResult Checkout()
        {
            return View("checkout", "checkout_layout", WorkContext);
        }


        // GET: /cart/checkout/paymentform?orderNumber=...
        [HttpGet]
        public async Task<ActionResult> PaymentForm(string orderNumber)
        {
            var order = await _orderApi.OrderModule.GetByNumberAsync(orderNumber);

            var incomingPayment = order.InPayments != null ? order.InPayments.FirstOrDefault(x => string.Equals(x.PaymentMethod.PaymentMethodType, "PreparedForm", System.StringComparison.InvariantCultureIgnoreCase)) : null;
            if (incomingPayment == null)
            {
                throw new StorefrontException("Order doesn't have any payment of type: PreparedForm");
            }
            var processingResult = await _orderApi.OrderModule.ProcessOrderPaymentsAsync(order.Id, incomingPayment.Id);

            WorkContext.PaymentFormHtml = processingResult.HtmlForm;

            return View("payment-form", WorkContext);
        }

        // GET/POST: /cart/externalpaymentcallback
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> ExternalPaymentCallback()
        {
            var callback = new orderModel.PaymentCallbackParameters
            {
                Parameters = new List<orderModel.KeyValuePair>()
            };

            foreach (var key in HttpContext.Request.QueryString.AllKeys)
            {
                callback.Parameters.Add(new orderModel.KeyValuePair
                {
                    Key = key,
                    Value = HttpContext.Request.QueryString[key]
                });
            }

            foreach (var key in HttpContext.Request.Form.AllKeys)
            {
                callback.Parameters.Add(new orderModel.KeyValuePair
                {
                    Key = key,
                    Value = HttpContext.Request.Form[key]
                });
            }

            var postProcessingResult = await _orderApi.OrderModule.PostProcessPaymentAsync(callback);
            if (postProcessingResult.IsSuccess.HasValue && postProcessingResult.IsSuccess.Value)
            {
                return StoreFrontRedirect("~/cart/thanks/" + postProcessingResult.OrderId);
            }
            else
            {
                return View("error");
            }
        }

        // GET: /cart/thanks/{orderNumber}
        [HttpGet]
        public async Task<ActionResult> Thanks(string orderNumber)
        {
            var order = await _orderApi.OrderModule.GetByNumberAsync(orderNumber);

            if (order == null || order.CustomerId != WorkContext.CurrentCustomer.Id)
            {
                throw new HttpException(404, "Order with number " + orderNumber + " not found.");
            }

            WorkContext.CurrentOrder = order.ToCustomerOrder(WorkContext.AllCurrencies, WorkContext.CurrentLanguage);

            return View("thanks", WorkContext);
        }
    }
}
