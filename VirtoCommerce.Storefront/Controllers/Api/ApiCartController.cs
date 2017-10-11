﻿using System.Linq;
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
            var cartBuilder = await LoadOrCreateCartAsync();
            await cartBuilder.ValidateAsync();
            return Json(cartBuilder.Cart, JsonRequestBehavior.AllowGet);
        }

        // GET: storefrontapi/cart/itemscount
        [HttpGet]
        public async Task<ActionResult> GetCartItemsCount()
        {
            EnsureCartExists();

            var cartBuilder = await LoadOrCreateCartAsync();

            return Json(cartBuilder.Cart.ItemsQuantity, JsonRequestBehavior.AllowGet);
        }

        // PUT: storefrontapi/cart/comment
        [HttpPut]
        public async Task<ActionResult> UpdateCartComment(string comment)
        {
            EnsureCartExists();

            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();

                await cartBuilder.UpdateCartComment(comment);
                await cartBuilder.SaveAsync();
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // POST: storefrontapi/cart/items?id=...&quantity=...
        [HttpPost]
        public async Task<ActionResult> AddItemToCart(string id, int quantity = 1)
        {
            EnsureCartExists();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();

                var products = await _catalogSearchService.GetProductsAsync(new[] { id }, Model.Catalog.ItemResponseGroup.ItemLarge);
                if (products != null && products.Any())
                {
                    await cartBuilder.AddItemAsync(products.First(), quantity);
                    await cartBuilder.SaveAsync();
                }
                return Json(new { ItemsCount = cartBuilder.Cart.ItemsQuantity });
            }
        }

        // PUT: storefrontapi/cart/items/price?lineItemId=...&newPrice=...
        [HttpPut]
        public async Task<ActionResult> ChangeCartItemPrice(string lineItemId, decimal newPrice)
        {
            EnsureCartExists();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();

                var lineItem = cartBuilder.Cart.Items.FirstOrDefault(x => x.Id == lineItemId);
                if (lineItem != null)
                {
                    var newPriceMoney = new Money(newPrice, cartBuilder.Cart.Currency);
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
                await cartBuilder.SaveAsync();

            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // PUT: storefrontapi/cart/items?lineItemId=...&quantity=...
        [HttpPut]
        public async Task<ActionResult> ChangeCartItem(string lineItemId, int quantity)
        {
            EnsureCartExists();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();

                var lineItem = cartBuilder.Cart.Items.FirstOrDefault(i => i.Id == lineItemId);
                if (lineItem != null)
                {
                    await cartBuilder.ChangeItemQuantityAsync(lineItemId, quantity);
                    await cartBuilder.SaveAsync();
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // DELETE: storefrontapi/cart/items?lineItemId=...
        [HttpDelete]
        public async Task<ActionResult> RemoveCartItem(string lineItemId)
        {
            EnsureCartExists();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();
                await cartBuilder.RemoveItemAsync(lineItemId);
                await cartBuilder.SaveAsync();
                return Json(new { ItemsCount = cartBuilder.Cart.ItemsQuantity });
            }

        }

        // POST: storefrontapi/cart/clear
        [HttpPost]
        public async Task<ActionResult> ClearCart()
        {
            EnsureCartExists();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();
                await cartBuilder.ClearAsync();
                await cartBuilder.SaveAsync();
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // GET: storefrontapi/cart/shipments/{shipmentId}/shippingmethods
        [HttpGet]
        public async Task<ActionResult> GetCartShipmentAvailShippingMethods(string shipmentId)
        {
            EnsureCartExists();

            var cartBuilder = await LoadOrCreateCartAsync();

            var shippingMethods = await cartBuilder.GetAvailableShippingMethodsAsync();
            return Json(shippingMethods, JsonRequestBehavior.AllowGet);
        }

        // GET: storefrontapi/cart/paymentmethods
        [HttpGet]
        public async Task<ActionResult> GetCartAvailPaymentMethods()
        {
            EnsureCartExists();

            var cartBuilder = await LoadOrCreateCartAsync();

            var paymentMethods = await cartBuilder.GetAvailablePaymentMethodsAsync();
            return Json(paymentMethods, JsonRequestBehavior.AllowGet);
        }

        // POST: storefrontapi/cart/coupons/{couponCode}
        [HttpPost]
        public async Task<ActionResult> AddCartCoupon(string couponCode)
        {
            EnsureCartExists();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();

                await cartBuilder.AddCouponAsync(couponCode);
                await cartBuilder.SaveAsync();
                return Json(cartBuilder.Cart.Coupon);
            }
        }


        // DELETE: storefrontapi/cart/coupons
        [HttpDelete]
        public async Task<ActionResult> RemoveCartCoupon()
        {
            EnsureCartExists();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();
                await cartBuilder.RemoveCouponAsync();
                await cartBuilder.SaveAsync();
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        // POST: storefrontapi/cart/paymentPlan    
        [HttpPost]
        public async Task<ActionResult> AddOrUpdateCartPaymentPlan(PaymentPlan paymentPlan)
        {
            EnsureCartExists();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();
                paymentPlan.Id = cartBuilder.Cart.Id;
                var paymentPlanDto = paymentPlan.ToPaymentPlanDto();

                await _subscriptionApi.SubscriptionModule.UpdatePaymentPlanAsync(paymentPlanDto);
                // await _cartBuilder.SaveAsync();
                cartBuilder.Cart.PaymentPlan = paymentPlan;
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // DELETE: storefrontapi/cart/paymentPlan    
        [HttpDelete]
        public async Task<ActionResult> DeleteCartPaymentPlan()
        {
            EnsureCartExists();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();
                await _subscriptionApi.SubscriptionModule.DeletePlansByIdsAsync(new[] { cartBuilder.Cart.Id });
                // await _cartBuilder.SaveAsync();
                cartBuilder.Cart.PaymentPlan = null;
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        // POST: storefrontapi/cart/shipments    
        [HttpPost]
        public async Task<ActionResult> AddOrUpdateCartShipment(Shipment shipment)
        {
            EnsureCartExists();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();
                await cartBuilder.AddOrUpdateShipmentAsync(shipment);
                await cartBuilder.SaveAsync();
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // POST: storefrontapi/cart/payments
        [HttpPost]
        public async Task<ActionResult> AddOrUpdateCartPayment(Payment payment)
        {
            EnsureCartExists();

            if (payment.Amount.Amount == decimal.Zero)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Valid payment amount is required");
            }

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();
                await cartBuilder.AddOrUpdatePaymentAsync(payment);
                await cartBuilder.SaveAsync();
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // POST: storefrontapi/cart/createorder
        [HttpPost]
        public async Task<ActionResult> CreateOrder(orderModel.BankCardInfo bankCardInfo)
        {
            EnsureCartExists();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();

                var order = await _orderApi.OrderModule.CreateOrderFromCartAsync(cartBuilder.Cart.Id);

                //Raise domain event
                await _orderPlacedEventPublisher.PublishAsync(new OrderPlacedEvent(order.ToCustomerOrder(WorkContext.AllCurrencies, WorkContext.CurrentLanguage), cartBuilder.Cart));


                orderModel.ProcessPaymentResult processingResult = null;
                var incomingPayment = order.InPayments != null ? order.InPayments.FirstOrDefault() : null;
                if (incomingPayment != null)
                {
                    processingResult = await _orderApi.OrderModule.ProcessOrderPaymentsAsync(order.Id, incomingPayment.Id, bankCardInfo);
                }

                await cartBuilder.RemoveCartAsync();

                return Json(new { order, orderProcessingResult = processingResult, paymentMethod = incomingPayment != null ? incomingPayment.PaymentMethod : null });
            }
        }


        private static string GetAsyncLockCartKey(ShoppingCart cart)
        {
            return string.Join(":", "Cart", cart.Id, cart.Name, cart.CustomerId);
        }

        private void EnsureCartExists()
        {
            if (WorkContext.CurrentCart == null)
            {
                throw new StorefrontException("Cart not found");
            }
        }
        private async Task<ICartBuilder> LoadOrCreateCartAsync()
        {
            var cart = WorkContext.CurrentCart;
            //Need to try load fresh cart from cache or service to prevent parallel update conflict
            //because WorkContext.CurrentCart may contains old cart
            await _cartBuilder.LoadOrCreateNewTransientCartAsync(cart.Name, WorkContext.CurrentStore, cart.Customer, cart.Language, cart.Currency);
            return _cartBuilder;
        }
    }
}
