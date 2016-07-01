using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using VirtoCommerce.OrderModule.Client.Api;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Common.Exceptions;
using orderApiModel = VirtoCommerce.OrderModule.Client.Model;

namespace VirtoCommerce.Storefront.Controllers
{
    public class CartController : StorefrontControllerBase
    {
        private readonly IVirtoCommerceOrdersApi _orderApi;

        public CartController(WorkContext workContext, IVirtoCommerceOrdersApi orderApi, IStorefrontUrlBuilder urlBuilder)
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
            var order = await _orderApi.OrderModuleGetByNumberAsync(orderNumber);

            var incomingPayment = order.InPayments != null ? order.InPayments.FirstOrDefault(x => string.Equals(x.PaymentMethod.PaymentMethodType, "PreparedForm", System.StringComparison.InvariantCultureIgnoreCase)) : null;
            if (incomingPayment == null)
            {
                throw new StorefrontException("Order doesn't have any payment of type: PreparedForm");
            }
            var processingResult = await _orderApi.OrderModuleProcessOrderPaymentsAsync(order.Id, incomingPayment.Id);

            WorkContext.PaymentFormHtml = processingResult.HtmlForm;

            return View("payment-form", WorkContext);
        }

        // GET: /cart/externalpaymentcallback
        [HttpGet]
        public async Task<ActionResult> ExternalPaymentCallback()
        {
            var callback = new orderApiModel.PaymentCallbackParameters
            {
                Parameters = new List<orderApiModel.KeyValuePair>()
            };

            foreach (var key in HttpContext.Request.QueryString.AllKeys)
            {
                callback.Parameters.Add(new orderApiModel.KeyValuePair
                {
                    Key = key,
                    Value = HttpContext.Request.QueryString[key]
                });
            }

            foreach (var key in HttpContext.Request.Form.AllKeys)
            {
                callback.Parameters.Add(new orderApiModel.KeyValuePair
                {
                    Key = key,
                    Value = HttpContext.Request.Form[key]
                });
            }

            var postProcessingResult = await _orderApi.OrderModulePostProcessPaymentAsync(callback);
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
            var order = await _orderApi.OrderModuleGetByNumberAsync(orderNumber);

            if (order == null || order.CustomerId != WorkContext.CurrentCustomer.Id)
            {
                return HttpNotFound();
            }

            WorkContext.CurrentOrder = order.ToWebModel(WorkContext.AllCurrencies, WorkContext.CurrentLanguage);

            return View("thanks", WorkContext);
        }
    }
}
