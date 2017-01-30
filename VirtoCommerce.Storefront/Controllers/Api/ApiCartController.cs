using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.SubscriptionModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Converters.Subscriptions;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Cart.Services;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Common.Events;
using VirtoCommerce.Storefront.Model.Common.Exceptions;
using VirtoCommerce.Storefront.Model.Order.Events;
using VirtoCommerce.Storefront.Model.Services;
using VirtoCommerce.Storefront.Model.Subscriptions;
using orderModel = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;

namespace VirtoCommerce.Storefront.Controllers.Api
{
    [HandleJsonError]
    public class ApiCartController : StorefrontControllerBase
    {
        private readonly ICartBuilder _cartBuilder;
        private readonly IOrdersModuleApiClient _orderApi;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly IEventPublisher<OrderPlacedEvent> _orderPlacedEventPublisher;
        private readonly ISubscriptionModuleApiClient _subscriptionApi;

        public ApiCartController(WorkContext workContext, ICatalogSearchService catalogSearchService, ICartBuilder cartBuilder,
                                 IOrdersModuleApiClient orderApi, IStorefrontUrlBuilder urlBuilder,
                                 IEventPublisher<OrderPlacedEvent> orderPlacedEventPublisher, ISubscriptionModuleApiClient subscriptionApi)
            : base(workContext, urlBuilder)
        {
            _cartBuilder = cartBuilder;
            _orderApi = orderApi;
            _catalogSearchService = catalogSearchService;
            _orderPlacedEventPublisher = orderPlacedEventPublisher;
            _subscriptionApi = subscriptionApi;
        }

        // Get current user shopping cart
        // GET: storefrontapi/cart
        [HttpGet]
        public async Task<ActionResult> GetCart()
        {
            await EnsureCartExistsAsync();
            await _cartBuilder.ValidateAsync();
            return Json(_cartBuilder.Cart, JsonRequestBehavior.AllowGet);
        }

        // GET: storefrontapi/cart/itemscount
        [HttpGet]
        public async Task<ActionResult> GetCartItemsCount()
        {
            await EnsureCartExistsAsync();

            return Json(_cartBuilder.Cart.ItemsQuantity, JsonRequestBehavior.AllowGet);
        }

        // POST: storefrontapi/cart/items?id=...&quantity=...
        [HttpPost]
        public async Task<ActionResult> AddItemToCart(string id, int quantity = 1)
        {
            await EnsureCartExistsAsync();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var products = await _catalogSearchService.GetProductsAsync(new[] { id }, Model.Catalog.ItemResponseGroup.ItemLarge);
                if (products != null && products.Any())
                {
                    await _cartBuilder.AddItemAsync(products.First(), quantity);
                    await _cartBuilder.SaveAsync();
                }
            }
            return Json(new { ItemsCount = _cartBuilder.Cart.ItemsQuantity });
        }

        // PUT: storefrontapi/cart/items/price?lineItemId=...&newPrice=...
        [HttpPut]
        public async Task<ActionResult> ChangeCartItemPrice(string lineItemId, decimal newPrice)
        {
            await EnsureCartExistsAsync();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var lineItem = _cartBuilder.Cart.Items.FirstOrDefault(x => x.Id == lineItemId);
                if (lineItem != null)
                {
                    var newPriceMoney = new Money(newPrice, _cartBuilder.Cart.Currency);
                    //do not allow to set less price via this API call
                    if (lineItem.ListPrice < newPriceMoney)
                    {
                        lineItem.ListPrice = newPriceMoney;
                    }
                    if (lineItem.SalePrice < newPriceMoney)
                    {
                        lineItem.SalePrice = newPriceMoney;
                    }
                }
                await _cartBuilder.SaveAsync();

            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // PUT: storefrontapi/cart/items?lineItemId=...&quantity=...
        [HttpPut]
        public async Task<ActionResult> ChangeCartItem(string lineItemId, int quantity)
        {
            await EnsureCartExistsAsync();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {

                var lineItem = _cartBuilder.Cart.Items.FirstOrDefault(i => i.Id == lineItemId);
                if (lineItem != null)
                {
                    await _cartBuilder.ChangeItemQuantityAsync(lineItemId, quantity);
                    await _cartBuilder.SaveAsync();
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // DELETE: storefrontapi/cart/items?lineItemId=...
        [HttpDelete]
        public async Task<ActionResult> RemoveCartItem(string lineItemId)
        {
            await EnsureCartExistsAsync();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.RemoveItemAsync(lineItemId);
                await _cartBuilder.SaveAsync();
            }

            return Json(new { ItemsCount = _cartBuilder.Cart.ItemsQuantity });
        }

        // POST: storefrontapi/cart/clear
        [HttpPost]
        public async Task<ActionResult> ClearCart()
        {
            await EnsureCartExistsAsync();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.ClearAsync();
                await _cartBuilder.SaveAsync();
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // GET: storefrontapi/cart/shipments/{shipmentId}/shippingmethods
        [HttpGet]
        public async Task<ActionResult> GetCartShipmentAvailShippingMethods(string shipmentId)
        {
            await EnsureCartExistsAsync();

            var shippingMethods = await _cartBuilder.GetAvailableShippingMethodsAsync();
            return Json(shippingMethods, JsonRequestBehavior.AllowGet);
        }

        // GET: storefrontapi/cart/paymentmethods
        [HttpGet]
        public async Task<ActionResult> GetCartAvailPaymentMethods()
        {
            await EnsureCartExistsAsync();

            var paymentMethods = await _cartBuilder.GetAvailablePaymentMethodsAsync();
            return Json(paymentMethods, JsonRequestBehavior.AllowGet);
        }

        // POST: storefrontapi/cart/coupons/{couponCode}
        [HttpPost]
        public async Task<ActionResult> AddCartCoupon(string couponCode)
        {
            await EnsureCartExistsAsync();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.AddCouponAsync(couponCode);
                await _cartBuilder.SaveAsync();
            }
            return Json(_cartBuilder.Cart.Coupon);
        }


        // DELETE: storefrontapi/cart/coupons
        [HttpDelete]
        public async Task<ActionResult> RemoveCartCoupon()
        {
            await EnsureCartExistsAsync();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.RemoveCouponAsync();
                await _cartBuilder.SaveAsync();
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        // POST: storefrontapi/cart/paymentPlan    
        [HttpPost]
        public async Task<ActionResult> AddOrUpdateCartPaymentPlan(PaymentPlan paymentPlan)
        {
            await EnsureCartExistsAsync();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                paymentPlan.Id = _cartBuilder.Cart.Id;
                var paymentPlanDto = paymentPlan.ToPaymentPlanDto();

                await _subscriptionApi.SubscriptionModule.UpdatePaymentPlanAsync(paymentPlanDto);
                // await _cartBuilder.SaveAsync();
                _cartBuilder.Cart.PaymentPlan = paymentPlan;
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // DELETE: storefrontapi/cart/paymentPlan    
        [HttpDelete]
        public async Task<ActionResult> DeleteCartPaymentPlan()
        {
            await EnsureCartExistsAsync();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                await _subscriptionApi.SubscriptionModule.DeletePlansByIdsAsync(new[] { _cartBuilder.Cart.Id });
                // await _cartBuilder.SaveAsync();
                _cartBuilder.Cart.PaymentPlan = null;
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        // POST: storefrontapi/cart/shipments    
        [HttpPost]
        public async Task<ActionResult> AddOrUpdateCartShipment(Shipment shipment)
        {
            await EnsureCartExistsAsync();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.AddOrUpdateShipmentAsync(shipment);
                await _cartBuilder.SaveAsync();
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // POST: storefrontapi/cart/payments
        [HttpPost]
        public async Task<ActionResult> AddOrUpdateCartPayment(Payment payment)
        {
            await EnsureCartExistsAsync();
            if (payment.Amount.Amount == decimal.Zero)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Valid payment amount is required");
            }

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.AddOrUpdatePaymentAsync(payment);
                await _cartBuilder.SaveAsync();
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // POST: storefrontapi/cart/createorder
        [HttpPost]
        public async Task<ActionResult> CreateOrder(orderModel.BankCardInfo bankCardInfo)
        {
            await EnsureCartExistsAsync();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var order = await _orderApi.OrderModule.CreateOrderFromCartAsync(_cartBuilder.Cart.Id);

                //Raise domain event
                await _orderPlacedEventPublisher.PublishAsync(new OrderPlacedEvent(order.ToCustomerOrder(WorkContext.AllCurrencies, WorkContext.CurrentLanguage), _cartBuilder.Cart));


                orderModel.ProcessPaymentResult processingResult = null;
                var incomingPayment = order.InPayments != null ? order.InPayments.FirstOrDefault() : null;
                if (incomingPayment != null)
                {
                    processingResult = await _orderApi.OrderModule.ProcessOrderPaymentsAsync(order.Id, incomingPayment.Id, bankCardInfo);
                }

                await _cartBuilder.RemoveCartAsync();

                return Json(new { order, orderProcessingResult = processingResult, paymentMethod = incomingPayment != null ? incomingPayment.PaymentMethod : null });
            }
        }


        private static string GetAsyncLockCartKey(ShoppingCart cart)
        {
            return string.Join(":", "Cart", cart.Id, cart.Name, cart.CustomerId);
        }

        private async Task EnsureCartExistsAsync()
        {
            if (WorkContext.CurrentCart == null)
            {
                throw new StorefrontException("Cart not found");
            }

            await _cartBuilder.TakeCartAsync(WorkContext.CurrentCart);
        }
    }
}
