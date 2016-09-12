using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.CartModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Cart.Services;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Common.Events;
using VirtoCommerce.Storefront.Model.Common.Exceptions;
using VirtoCommerce.Storefront.Model.Customer;
using VirtoCommerce.Storefront.Model.Customer.Services;
using VirtoCommerce.Storefront.Model.Marketing;
using VirtoCommerce.Storefront.Model.Marketing.Services;
using VirtoCommerce.Storefront.Model.Order.Events;
using VirtoCommerce.Storefront.Model.Quote;
using VirtoCommerce.Storefront.Model.Services;
using VirtoCommerce.Storefront.Model.Stores;

namespace VirtoCommerce.Storefront.Builders
{
    public class CartBuilder : ICartBuilder, IAsyncObserver<UserLoginEvent>
    {
        private readonly ICoreModuleApiClient _commerceApi;
        private readonly ICartModuleApiClient _cartApi;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly ILocalCacheManager _cacheManager;
        private readonly ICustomerService _customerService;
        private readonly Func<WorkContext> _workContextFactory;
        private ShoppingCart _cart;
        private const string _cartCacheRegion = "CartRegion";

        [CLSCompliant(false)]
        public CartBuilder(ICartModuleApiClient cartApi, ICatalogSearchService catalogSearchService, ICoreModuleApiClient commerceApi, ILocalCacheManager cacheManager, Func<WorkContext> workContextFactory, ICustomerService customerService)
        {
            _cartApi = cartApi;
            _catalogSearchService = catalogSearchService;
            _cacheManager = cacheManager;
            _commerceApi = commerceApi;
            _workContextFactory = workContextFactory;
            _customerService = customerService;
        }

        public string CartCacheKey
        {
            get
            {
                if (_cart == null)
                {
                    throw new StorefrontException("Cart is not set");
                }
                return GetCartCacheKey(_cart.StoreId, _cart.Name, _cart.CustomerId, _cart.Currency.Code);
            }
        }

        #region ICartBuilder Members

        public ICartBuilder TakeCart(ShoppingCart cart)
        {
            _cart = cart;
            return this;
        }

        public virtual async Task<ICartBuilder> LoadDefaultCart(Store store, CustomerInfo customer, Language language, Currency currency)
        {
            var cacheKey = GetCartCacheKey(store.Id, "default", customer.Id, currency.Code);

            _cart = await _cacheManager.GetAsync(cacheKey, _cartCacheRegion, async () =>
            {
                var cart = await _cartApi.CartModule.GetCartAsync(store.Id, customer.Id, "default", currency.Code, language.CultureName);

                var retVal = cart.ToWebModel(currency, language, customer);             
                return retVal;
            });

            return this;
        }

        public virtual async Task<ICartBuilder> AddItemAsync(Product product, int quantity)
        {
            var lineItem = product.ToLineItem(_cart.Language, quantity).ToServiceModel();
            await _cartApi.CartModule.AddItemToCartAsync(_cart.Id, lineItem);
            await ReloadAsync();
            return this;
        }

        public virtual async Task<ICartBuilder> ChangeItemQuantityAsync(string id, int quantity)
        {
            var lineItem = _cart.Items.FirstOrDefault(i => i.Id == id);
            if (lineItem != null)
            {
                await InnerChangeItemQuantityAsync(lineItem, quantity);
            }
            await SaveAsync();
            return this;
        }

        public virtual async Task<ICartBuilder> ChangeItemQuantityAsync(int lineItemIndex, int quantity)
        {
            var lineItem = _cart.Items.ElementAt(lineItemIndex);
            if (lineItem != null)
            {
                await InnerChangeItemQuantityAsync(lineItem, quantity);
            }
            await SaveAsync();
            return this;
        }

        public virtual async Task<ICartBuilder> ChangeItemsQuantitiesAsync(int[] quantities)
        {
            for (var i = 0; i < quantities.Length; i++)
            {
                var lineItem = _cart.Items.ElementAt(i);
                if (lineItem != null && quantities[i] > 0)
                {
                    await InnerChangeItemQuantityAsync(lineItem, quantities[i]);
                }
            }
            await SaveAsync();
            return this;
        }

        public virtual async Task<ICartBuilder> RemoveItemAsync(string id)
        {
            await _cartApi.CartModule.RemoveCartItemAsync(_cart.Id, id);
            await ReloadAsync();
            return this;
        }

        public virtual async Task<ICartBuilder> ClearAsync()
        {
            await _cartApi.CartModule.ClearCartAsync(_cart.Id);
            await ReloadAsync();
            return this;
        }

        public virtual async Task<ICartBuilder> AddCouponAsync(string couponCode)
        {
            await _cartApi.CartModule.AddCartCouponAsync(_cart.Id, couponCode);
            await ReloadAsync();
            return this;
        }

        public virtual async Task<ICartBuilder> RemoveCouponAsync()
        {
            if (_cart.Coupon != null)
            {
                await _cartApi.CartModule.RemoveCartCouponAsync(_cart.Id, _cart.Coupon.Code);
            }
            await ReloadAsync();
            return this;
        }

        public virtual async Task<ICartBuilder> AddOrUpdateShipmentAsync(Shipment shipment)
        {
            await _cartApi.CartModule.AddOrUpdateCartShipmentAsync(_cart.Id, shipment.ToServiceModel());
            await ReloadAsync();
            return this;
        }

        public virtual async Task<ICartBuilder> RemoveShipmentAsync(string shipmentId)
        {
            var shipment = _cart.Shipments.FirstOrDefault(s => s.Id == shipmentId);
            if (shipment != null)
            {
                _cart.Shipments.Remove(shipment);
            }
            await SaveAsync();
            return this;
        }

        public virtual async Task<ICartBuilder> AddOrUpdatePaymentAsync(Payment payment)
        {
            await _cartApi.CartModule.AddOrUpdateCartPaymentAsync(_cart.Id, payment.ToServiceModel());
            await ReloadAsync();
            return this;
        }

        public virtual async Task<ICartBuilder> MergeWithCartAsync(ShoppingCart cart)
        {
            await _cartApi.CartModule.MergeWithCartAsync(_cart.Id, cart.ToServiceModel());
            await ReloadAsync();
            return this;
        }

        public virtual async Task<ICartBuilder> RemoveCartAsync()
        {
            await _cartApi.CartModule.DeleteCartsAsync(new List<string> { _cart.Id });
            await ReloadAsync();
            return this;
        }

        public virtual async Task<ICartBuilder> FillFromQuoteRequest(QuoteRequest quoteRequest)
        {
            var productIds = quoteRequest.Items.Select(i => i.ProductId);
            var products = await _catalogSearchService.GetProductsAsync(productIds.ToArray(), ItemResponseGroup.ItemLarge);

            _cart.Items.Clear();
            foreach (var product in products)
            {
                var quoteItem = quoteRequest.Items.FirstOrDefault(i => i.ProductId == product.Id);
                if (quoteItem != null)
                {
                    var lineItem = product.ToLineItem(_cart.Language, (int)quoteItem.SelectedTierPrice.Quantity);
                    lineItem.ListPrice = quoteItem.ListPrice;
                    lineItem.SalePrice = quoteItem.SelectedTierPrice.Price;
                    lineItem.ValidationType = ValidationType.None;

                    AddLineItem(lineItem);
                }
            }

            if (quoteRequest.RequestShippingQuote)
            {
                _cart.Shipments.Clear();
                var shipment = new Shipment(_cart.Currency);

                foreach (var item in _cart.Items)
                {
                    shipment.Items.Add(item.ToShipmentItem());
                }

                if (quoteRequest.ShippingAddress != null)
                {
                    shipment.DeliveryAddress = quoteRequest.ShippingAddress;
                }

                if (quoteRequest.ShipmentMethod != null)
                {
                    var availableShippingMethods = await GetAvailableShippingMethodsAsync();
                    if (availableShippingMethods != null)
                    {
                        var availableShippingMethod = availableShippingMethods.FirstOrDefault(sm => sm.ShipmentMethodCode == quoteRequest.ShipmentMethod.ShipmentMethodCode);
                        if (availableShippingMethod != null)
                        {
                            shipment = quoteRequest.ShipmentMethod.ToShipmentModel(_cart.Currency);
                        }
                    }
                }

                _cart.Shipments.Add(shipment);
            }

            _cart.Payments.Clear();
            var payment = new Payment(_cart.Currency);

            if (quoteRequest.BillingAddress != null)
            {
                payment.BillingAddress = quoteRequest.BillingAddress;
            }

            payment.Amount = quoteRequest.Totals.GrandTotalInclTax;

            _cart.Payments.Add(payment);

            await SaveAsync();

            return this;
        }

        public virtual async Task<ICollection<ShippingMethod>> GetAvailableShippingMethodsAsync()
        {
            var workContext = _workContextFactory();
            var shippingRates = await _cartApi.CartModule.GetAvailableShippingRatesAsync(_cart.Id);
            return shippingRates.Select(x => x.ToWebModel(_cart.Currency, workContext.AllCurrencies)).ToList();
        }

        public virtual async Task<ICollection<PaymentMethod>> GetAvailablePaymentMethodsAsync()
        {
            var payments = await _cartApi.CartModule.GetAvailablePaymentMethodsAsync(_cart.Id);
            return payments.Select(x => x.ToWebModel()).ToList();
        }

       
   
        public ShoppingCart Cart
        {
            get
            {
                return _cart;
            }
        }

        public virtual async Task<ICartBuilder> ReloadAsync()
        {
            //Invalidate cart in cache
            InvalidateCache();
            var workContext = _workContextFactory();
            var cart = await _cartApi.CartModule.GetCartByIdAsync(_cart.Id);
            if (cart != null)
            {
                var customer = await _customerService.GetCustomerByIdAsync(_cart.CustomerId) ?? workContext.CurrentCustomer;
                _cart = cart.ToWebModel(_cart.Currency, _cart.Language, customer);
            }
            else
            {
                await LoadDefaultCart(workContext.CurrentStore, workContext.CurrentCustomer, workContext.CurrentLanguage, workContext.CurrentCurrency);
            }
            return this;
        }

        #endregion

        #region IObserver<UserLoginEvent> Members
        /// <summary>
        /// Merger anonymous cart by loging event
        /// </summary>
        /// <param name="userLoginEvent"></param>
        public async Task OnNextAsync(UserLoginEvent userLoginEvent)
        {
            if (userLoginEvent == null)
                return;

            var workContext = userLoginEvent.WorkContext;
            var prevUser = userLoginEvent.PrevUser;
            var prevUserCart = userLoginEvent.WorkContext.CurrentCart;
            var newUser = userLoginEvent.NewUser;

            //If previous user was anonymous and it has not empty cart need merge anonymous cart to personal
            if (!prevUser.IsRegisteredUser && prevUserCart != null && prevUserCart.Items.Any())
            {
                //we load or create cart for new user
                await LoadDefaultCart(workContext.CurrentStore, newUser, workContext.CurrentLanguage, workContext.CurrentCurrency);
                await MergeWithCartAsync(prevUserCart);
            }
        }

        #endregion
        protected virtual async Task SaveAsync()
        {
            var cart = _cart.ToServiceModel();

            //Invalidate cart in cache
            InvalidateCache();
            await _cartApi.CartModule.UpdateAsync(cart);
            await ReloadAsync();
        }

     
        protected virtual void InvalidateCache()
        {
            //Invalidate cart in cache
            _cacheManager.Remove(CartCacheKey, _cartCacheRegion);
        }

        private async Task InnerChangeItemQuantityAsync(LineItem lineItem, int quantity)
        {
            if (lineItem != null)
            {
                var product = (await _catalogSearchService.GetProductsAsync(new[] { lineItem.ProductId }, ItemResponseGroup.ItemWithPrices)).FirstOrDefault();
                if (product != null)
                {
                    lineItem.SalePrice = product.Price.GetTierPrice(quantity).Price;
                }
                if (quantity > 0)
                {
                    lineItem.Quantity = quantity;
                }
                else
                {
                    _cart.Items.Remove(lineItem);
                }
            }
        }

        private void AddLineItem(LineItem lineItem)
        {
            var existingLineItem = _cart.Items.FirstOrDefault(li => li.ProductId == lineItem.ProductId);
            if (existingLineItem != null)
            {
                existingLineItem.Quantity += lineItem.Quantity;
            }
            else
            {
                lineItem.Id = null;
                _cart.Items.Add(lineItem);
            }
        }

   
        private string GetCartCacheKey(string storeId,string name, string customerId, string currency)
        {
            return "CartBuilder:" + string.Join(":", storeId, name, customerId, currency).ToLowerInvariant();
        }
    }
}
