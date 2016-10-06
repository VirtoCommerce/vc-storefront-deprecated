using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using VirtoCommerce.Storefront.AutoRestClients.CartModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.JsonConverters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Cart.Services;
using VirtoCommerce.Storefront.Model.Cart.ValidationErrors;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Common.Events;
using VirtoCommerce.Storefront.Model.Common.Exceptions;
using VirtoCommerce.Storefront.Model.Order.Events;
using VirtoCommerce.Storefront.Model.Services;
using orderModel = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;

namespace VirtoCommerce.Storefront.Controllers.Api
{
    [HandleJsonError]
    public class ApiCartController : StorefrontControllerBase
    {
        private readonly ICartModuleApiClient _cartApi;
        private readonly ICartBuilder _cartBuilder;
        private readonly IOrdersModuleApiClient _orderApi;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly IEventPublisher<OrderPlacedEvent> _orderPlacedEventPublisher;

        public ApiCartController(WorkContext workContext, ICatalogSearchService catalogSearchService, ICartBuilder cartBuilder,
                                 IOrdersModuleApiClient orderApi, IStorefrontUrlBuilder urlBuilder,
                                 IEventPublisher<OrderPlacedEvent> orderPlacedEventPublisher, ICartModuleApiClient cartApi)
            : base(workContext, urlBuilder)
        {
            _cartBuilder = cartBuilder;
            _orderApi = orderApi;
            _catalogSearchService = catalogSearchService;
            _orderPlacedEventPublisher = orderPlacedEventPublisher;
            _cartApi = cartApi;
        }

        // Get current user shopping cart
        // GET: storefrontapi/cart
        [HttpGet]
        public async Task<ActionResult> GetCart()
        {
            EnsureThatCartExist();
            await _cartBuilder.ValidateAsync();                        
            return Json(_cartBuilder.Cart, JsonRequestBehavior.AllowGet);
        }

        // GET: storefrontapi/cart/itemscount
        [HttpGet]
        public ActionResult GetCartItemsCount()
        {
            EnsureThatCartExist();

            return Json(_cartBuilder.Cart.ItemsCount, JsonRequestBehavior.AllowGet);
        }

        // POST: storefrontapi/cart/items?id=...&quantity=...
        [HttpPost]
        public async Task<ActionResult> AddItemToCart(string id, int quantity = 1)
        {
            EnsureThatCartExist();

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
            return Json(new { _cartBuilder.Cart.ItemsCount });
        }

        // PUT: storefrontapi/cart/items/price?lineItemId=...&newPrice=...
        [HttpPut]
        public async Task<ActionResult> ChangeCartItemPrice(string lineItemId, decimal newPrice)
        {
            EnsureThatCartExist();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var lineItem = _cartBuilder.Cart.Items.FirstOrDefault(x => x.Id == lineItemId);
                if(lineItem != null)
                {
                    var newPriceMoney = new Money(newPrice, _cartBuilder.Cart.Currency); 
                    if (lineItem.ListPrice < newPriceMoney)
                    {
                        lineItem.ListPrice = newPriceMoney;
                    }
                    lineItem.SalePrice = newPriceMoney;
                }
               await _cartBuilder.SaveAsync();
          
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // PUT: storefrontapi/cart/items?lineItemId=...&quantity=...
        [HttpPut]
        public async Task<ActionResult> ChangeCartItem(string lineItemId, int quantity)
        {
            EnsureThatCartExist();

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
            EnsureThatCartExist();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.RemoveItem(lineItemId).SaveAsync();
            }

            return Json(new { _cartBuilder.Cart.ItemsCount });
        }

        // POST: storefrontapi/cart/clear
        [HttpPost]
        public async Task<ActionResult> ClearCart()
        {
            EnsureThatCartExist();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.Clear().SaveAsync();
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // GET: storefrontapi/cart/shipments/{shipmentId}/shippingmethods
        [HttpGet]
        public async Task<ActionResult> GetCartShipmentAvailShippingMethods(string shipmentId)
        {
            EnsureThatCartExist();

            var shippingMethods = await _cartBuilder.GetAvailableShippingMethodsAsync();
            return Json(shippingMethods, JsonRequestBehavior.AllowGet);
        }

        // GET: storefrontapi/cart/paymentmethods
        [HttpGet]
        public async Task<ActionResult> GetCartAvailPaymentMethods()
        {
            EnsureThatCartExist();

            var paymentMethods = await _cartBuilder.GetAvailablePaymentMethodsAsync();
            return Json(paymentMethods, JsonRequestBehavior.AllowGet);
        }

        // POST: storefrontapi/cart/coupons/{couponCode}
        [HttpPost]
        public async Task<ActionResult> AddCartCoupon(string couponCode)
        {
            EnsureThatCartExist();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.AddCoupon(couponCode).SaveAsync();
            }
            return Json(_cartBuilder.Cart.Coupon);
        }


        // DELETE: storefrontapi/cart/coupons
        [HttpDelete]
        public async Task<ActionResult> RemoveCartCoupon()
        {
            EnsureThatCartExist();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.RemoveCoupon().SaveAsync();
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // POST: storefrontapi/cart/shipments    
        [HttpPost]
        public async Task<ActionResult> AddOrUpdateCartShipment(Shipment shipment)
        {
            EnsureThatCartExist();

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
            EnsureThatCartExist();

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
            EnsureThatCartExist();

            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext.CurrentCart)).LockAsync())
            {
                var order = await  _orderApi.OrderModule.CreateOrderFromCartAsync(_cartBuilder.Cart.Id);

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

        private void EnsureThatCartExist()
        {
            if (WorkContext.CurrentCart == null)
            {
                throw new StorefrontException("Cart not found");
            }
            _cartBuilder.TakeCart(WorkContext.CurrentCart);
        }
    }
}
