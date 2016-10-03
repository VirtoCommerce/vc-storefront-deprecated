using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.CartModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Cart.Services;
using VirtoCommerce.Storefront.Model.Cart.ValidationErrors;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Common.Events;
using VirtoCommerce.Storefront.Model.Common.Exceptions;
using VirtoCommerce.Storefront.Model.Customer;
using VirtoCommerce.Storefront.Model.Marketing;
using VirtoCommerce.Storefront.Model.Marketing.Services;
using VirtoCommerce.Storefront.Model.Order.Events;
using VirtoCommerce.Storefront.Model.Quote;
using VirtoCommerce.Storefront.Model.Services;
using VirtoCommerce.Storefront.Model.Stores;
using VirtoCommerce.Storefront.Model.Tax.Services;

namespace VirtoCommerce.Storefront.Builders
{
    public class CartBuilder : ICartBuilder, IAsyncObserver<UserLoginEvent>
    {
        private readonly Func<WorkContext> _workContextFactory;
        private readonly ICartModuleApiClient _cartApi;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly ILocalCacheManager _cacheManager;
        private readonly IPromotionEvaluator _promotionEvaluator;
        private readonly ITaxEvaluator _taxEvaluator;
        private readonly PromotionEvaluationContextConverter _promotionEvaluationContextConverter;
        private ShoppingCart _cart;
        private const string _cartCacheRegion = "CartRegion";

        public CartBuilder(
            Func<WorkContext> workContextFactory,
            ICartModuleApiClient cartApi,
            ICatalogSearchService catalogSearchService,
            ILocalCacheManager cacheManager,
            IPromotionEvaluator promotionEvaluator,
            ITaxEvaluator taxEvaluator,
            PromotionEvaluationContextConverter promotionEvaluationContextConverter)
        {
            _cartApi = cartApi;
            _catalogSearchService = catalogSearchService;
            _cacheManager = cacheManager;
            _workContextFactory = workContextFactory;
            _promotionEvaluator = promotionEvaluator;
            _taxEvaluator = taxEvaluator;
            _promotionEvaluationContextConverter = promotionEvaluationContextConverter;
        }

        #region ICartBuilder Members

        public virtual ICartBuilder TakeCart(ShoppingCart cart)
        {
            _cart = cart;
            return this;
        }

        public virtual async Task LoadOrCreateNewTransientCartAsync(string cartName, Store store, CustomerInfo customer, Language language, Currency currency)
        {
            var cacheKey = GetCartCacheKey(store.Id, cartName, customer.Id, currency.Code);
            bool needReevaluate = false;
            _cart = await _cacheManager.GetAsync(cacheKey, _cartCacheRegion, async () =>
            {
                needReevaluate = true;
                var cartSearchCriteria = new AutoRestClients.CartModuleApi.Models.ShoppingCartSearchCriteria
                {
                    StoreId = store.Id,
                    CustomerId = customer.Id,
                    Name = cartName,
                    Currency = currency.Code
                };

                var result = await _cartApi.CartModule.SearchAsync(cartSearchCriteria);
                var cart = result.Results.Select(x => x.ToWebModel(currency, language, customer)).FirstOrDefault();

                if (cart == null)
                {
                    cart = new ShoppingCart(currency, language)
                    {
                        CustomerId = customer.Id,
                        Name = "Default",
                        StoreId = store.Id,
                        IsAnonymous = !customer.IsRegisteredUser,
                        CustomerName = customer.IsRegisteredUser ? customer.UserName : StorefrontConstants.AnonymousUsername
                    };
                }

                cart.Customer = customer;
                return cart;
            });

            if (needReevaluate)
            {
                await EvaluatePromotionsAsync();
                await EvaluateTaxesAsync();
            }
        }

        public virtual async Task AddItemAsync(Product product, int quantity)
        {
            EnsureThatCartExist();

            var lineItem = product.ToLineItem(_cart.Language, quantity);
            await AddLineItemAsync(lineItem);
        }

        public virtual async Task ChangeItemQuantityAsync(string id, int quantity)
        {
            EnsureThatCartExist();

            var lineItem = _cart.Items.FirstOrDefault(i => i.Id == id);
            if (lineItem != null)
            {
                await InnerChangeItemQuantityAsync(lineItem, quantity);
            }
        }

        public virtual async Task ChangeItemQuantityAsync(int lineItemIndex, int quantity)
        {
            EnsureThatCartExist();

            var lineItem = _cart.Items.ElementAt(lineItemIndex);
            if (lineItem != null)
            {
                await InnerChangeItemQuantityAsync(lineItem, quantity);
            }
        }

        public virtual async Task ChangeItemsQuantitiesAsync(int[] quantities)
        {
            EnsureThatCartExist();

            for (var i = 0; i < quantities.Length; i++)
            {
                var lineItem = _cart.Items.ElementAt(i);
                if (lineItem != null && quantities[i] > 0)
                {
                    await InnerChangeItemQuantityAsync(lineItem, quantities[i]);
                }
            }
        }

        public virtual ICartBuilder RemoveItem(string id)
        {
            EnsureThatCartExist();

            var lineItem = _cart.Items.FirstOrDefault(x => x.Id == id);
            if (lineItem != null)
            {
                _cart.Items.Remove(lineItem);
            }
            return this;
        }

        public virtual ICartBuilder AddCoupon(string couponCode)
        {
            EnsureThatCartExist();
            _cart.Coupon = new Coupon { Code = couponCode };
            return this;
        }


        public virtual ICartBuilder RemoveCoupon()
        {
            EnsureThatCartExist();
            _cart.Coupon = null;
            return this;
        }


        public virtual ICartBuilder Clear()
        {
            EnsureThatCartExist();
            _cart.Items.Clear();
            return this;
        }

        public virtual async Task AddOrUpdateShipmentAsync(Shipment shipment)
        {
            EnsureThatCartExist();

            Shipment existShipment = null;
            if (!shipment.IsTransient())
            {
                existShipment = _cart.Shipments.FirstOrDefault(s => s.Id == shipment.Id);
            }
            if (existShipment != null)
            {
                _cart.Shipments.Remove(existShipment);
            }
            shipment.Currency = _cart.Currency;
            _cart.Shipments.Add(shipment);

            if (!string.IsNullOrEmpty(shipment.ShipmentMethodCode))
            {
                var availableShippingMethods = await GetAvailableShippingMethodsAsync();
                var shippingMethod = availableShippingMethods.FirstOrDefault(sm => shipment.ShipmentMethodCode.EqualsInvariant(sm.ShipmentMethodCode) && shipment.ShipmentMethodOption.EqualsInvariant(sm.OptionName));
                if (shippingMethod == null)
                {
                    throw new Exception(string.Format(CultureInfo.InvariantCulture, "Unknown shipment method: {0} with option: {1}", shipment.ShipmentMethodCode, shipment.ShipmentMethodOption));
                }
                shipment.Price = shippingMethod.Price;
                shipment.DiscountAmount = shippingMethod.DiscountAmount;
                shipment.TaxType = shippingMethod.TaxType;
            }
        }

        public virtual ICartBuilder RemoveShipment(string shipmentId)
        {
            EnsureThatCartExist();

            var shipment = _cart.Shipments.FirstOrDefault(s => s.Id == shipmentId);
            if (shipment != null)
            {
                _cart.Shipments.Remove(shipment);
            }
            return this;
        }

        public virtual async Task AddOrUpdatePaymentAsync(Payment payment)
        {
            EnsureThatCartExist();

            Payment existPayment = null;
            if (!payment.IsTransient())
            {
                existPayment = _cart.Payments.FirstOrDefault(s => s.Id == payment.Id);
            }

            if (existPayment != null)
            {
                _cart.Payments.Remove(existPayment);
            }
            _cart.Payments.Add(payment);

            if (!string.IsNullOrEmpty(payment.PaymentGatewayCode))
            {
                var availablePaymentMethods = await GetAvailablePaymentMethodsAsync();
                var paymentMethod = availablePaymentMethods.FirstOrDefault(pm => string.Equals(pm.Code, payment.PaymentGatewayCode, StringComparison.InvariantCultureIgnoreCase));
                if (paymentMethod == null)
                {
                    throw new Exception("Unknown payment method " + payment.PaymentGatewayCode);
                }
            }
        }

        public virtual async Task MergeWithCartAsync(ShoppingCart cart)
        {
            EnsureThatCartExist();

            foreach (var lineItem in cart.Items)
            {
                await AddLineItemAsync(lineItem);
            }
            _cart.Coupon = cart.Coupon;

            _cart.Shipments.Clear();
            _cart.Shipments = cart.Shipments;

            _cart.Payments.Clear();
            _cart.Payments = cart.Payments;
        }

        public virtual async Task RemoveCartAsync()
        {
            EnsureThatCartExist();
            InvalidateCache();
            await _cartApi.CartModule.DeleteCartsAsync(new List<string> { _cart.Id });
        }

        public virtual async Task FillFromQuoteRequestAsync(QuoteRequest quoteRequest)
        {
            EnsureThatCartExist();

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
                    if (lineItem.ListPrice < lineItem.SalePrice)
                    {
                        lineItem.ListPrice = lineItem.SalePrice;
                    }
                    lineItem.IsReadOnly = true;
                    lineItem.Id = null;
                    _cart.Items.Add(lineItem);
                }
            }

            if (quoteRequest.RequestShippingQuote)
            {
                _cart.Shipments.Clear();
                var shipment = new Shipment(_cart.Currency);

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
        }

        public virtual async Task<ICollection<ShippingMethod>> GetAvailableShippingMethodsAsync()
        {
            var workContext = _workContextFactory();

            //Request available shipping rates 
            var shippingRates = await _cartApi.CartModule.GetAvailableShippingRatesAsync(_cart.Id);
            var retVal = shippingRates.Select(x => x.ToWebModel(_cart.Currency, workContext.AllCurrencies)).ToList();

            //Evaluate promotions cart and apply rewards for available shipping methods
            var promoEvalContext = _promotionEvaluationContextConverter.ToPromotionEvaluationContext(_cart);
            await _promotionEvaluator.EvaluateDiscountsAsync(promoEvalContext, retVal);

            //Evaluate taxes for available shipping rates
            var taxEvalContext = _cart.ToTaxEvalContext();
            taxEvalContext.Lines.Clear();
            taxEvalContext.Lines.AddRange(retVal.SelectMany(x => x.ToTaxLines()));
            await _taxEvaluator.EvaluateTaxesAsync(taxEvalContext, retVal);

            return retVal;
        }

        public virtual async Task<ICollection<PaymentMethod>> GetAvailablePaymentMethodsAsync()
        {
            EnsureThatCartExist();
            var payments = await _cartApi.CartModule.GetAvailablePaymentMethodsAsync(_cart.Id);
            return payments.Select(x => x.ToWebModel()).ToList();
        }

        public async Task ValidateAsync()
        {
            EnsureThatCartExist();
            await Task.WhenAll(ValidateCartItemsAsync(), ValidateCartShipmentsAsync());
            _cart.IsValid = _cart.Items.All(x => x.IsValid) && _cart.Shipments.All(x => x.IsValid);
        }

        public virtual async Task EvaluatePromotionsAsync()
        {
            EnsureThatCartExist();
            var evalContext = _promotionEvaluationContextConverter.ToPromotionEvaluationContext(_cart);

            await _promotionEvaluator.EvaluateDiscountsAsync(evalContext, new IDiscountable[] { _cart });
        }

        public async Task EvaluateTaxesAsync()
        {
            await _taxEvaluator.EvaluateTaxesAsync(_cart.ToTaxEvalContext(), new[] { _cart });
        }

        public ShoppingCart Cart
        {
            get
            {
                return _cart;
            }
        }

        public virtual async Task SaveAsync()
        {
            EnsureThatCartExist();
            InvalidateCache();

            await EvaluatePromotionsAsync();
            await EvaluateTaxesAsync();

            var cart = _cart.ToServiceModel();
            if (string.IsNullOrEmpty(cart.Id))
            {
                cart = await _cartApi.CartModule.CreateAsync(cart);
            }
            else
            {
                await _cartApi.CartModule.UpdateAsync(cart);
            }
            cart = await _cartApi.CartModule.GetCartByIdAsync(cart.Id);
            _cart = cart.ToWebModel(_cart.Currency, _cart.Language, _cart.Customer);
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
                await LoadOrCreateNewTransientCartAsync(prevUserCart.Name, workContext.CurrentStore, newUser, workContext.CurrentLanguage, workContext.CurrentCurrency);
                await MergeWithCartAsync(prevUserCart);
                await SaveAsync();
                await _cartApi.CartModule.DeleteCartsAsync(new[] { prevUserCart.Id }.ToList());
            }
        }

        #endregion

        protected virtual void InvalidateCache()
        {
            //Invalidate cart in cache
            _cacheManager.Remove(GetCartCacheKey(_cart.StoreId, _cart.Name, _cart.CustomerId, _cart.Currency.Code), _cartCacheRegion);
            _cacheManager.Remove(GetCartCacheKey(_cart.Id), _cartCacheRegion);
        }

        protected virtual string GetCartCacheKey(string cartId)
        {
            return "CartBuilder:" + cartId;
        }
        protected virtual string GetCartCacheKey(string storeId, string cartName, string customerId, string currency)
        {
            return "CartBuilder:" + string.Join(":", storeId, cartName, customerId, currency).ToLowerInvariant();
        }

        protected virtual async Task ValidateCartItemsAsync()
        {
            var productIds = _cart.Items.Select(i => i.ProductId).ToArray();
            var cacheKey = "CartBuilder.ValidateCartItemsAsync:" + _cart.Id + ":" + string.Join(":", productIds);
            var products = await _cacheManager.GetAsync(cacheKey, "ApiRegion", async () => await _catalogSearchService.GetProductsAsync(productIds, ItemResponseGroup.ItemWithPrices | ItemResponseGroup.ItemWithDiscounts | ItemResponseGroup.Inventory));

            foreach (var lineItem in _cart.Items.ToList())
            {
                lineItem.ValidationErrors.Clear();

                var product = products.FirstOrDefault(p => p.Id == lineItem.ProductId);
                if (product == null || !product.IsActive || !product.IsBuyable)
                {
                    lineItem.ValidationErrors.Add(new UnavailableError());
                    lineItem.IsValid = false;
                }
                else
                {
                    if (product.TrackInventory && product.Inventory != null)
                    {
                        var availableQuantity = product.Inventory.InStockQuantity;
                        if (product.Inventory.ReservedQuantity.HasValue)
                        {
                            availableQuantity -= product.Inventory.ReservedQuantity.Value;
                        }
                        if (availableQuantity.HasValue && lineItem.Quantity > availableQuantity.Value)
                        {
                            lineItem.ValidationErrors.Add(new QuantityError(availableQuantity.Value));
                            lineItem.IsValid = false;
                        }
                    }

                    var tierPrice = product.Price.GetTierPrice(lineItem.Quantity);
                    if (tierPrice.Price > lineItem.SalePrice)
                    {
                        lineItem.ValidationErrors.Add(new PriceError(lineItem.SalePrice, lineItem.SalePriceWithTax, tierPrice.Price, tierPrice.PriceWithTax));
                    }
                }
            }
        }

        protected virtual async Task ValidateCartShipmentsAsync()
        {
            foreach (var shipment in _cart.Shipments.ToArray())
            {
                shipment.ValidationErrors.Clear();

                var availShippingmethods = await GetAvailableShippingMethodsAsync();
                var shipmentShippingMethod = availShippingmethods.FirstOrDefault(sm => shipment.HasSameMethod(sm));
                if (shipmentShippingMethod == null)
                {
                    shipment.ValidationErrors.Add(new UnavailableError());
                }
                else if (shipmentShippingMethod.Price != shipment.Price)
                {
                    shipment.ValidationErrors.Add(new PriceError(shipment.Price, shipment.PriceWithTax, shipmentShippingMethod.Price, shipmentShippingMethod.PriceWithTax));
                }
            }
        }

        private async Task InnerChangeItemQuantityAsync(LineItem lineItem, int quantity)
        {
            if (lineItem != null && !lineItem.IsReadOnly)
            {
                var product = (await _catalogSearchService.GetProductsAsync(new[] { lineItem.ProductId }, ItemResponseGroup.ItemWithPrices)).FirstOrDefault();
                if (product != null)
                {
                    lineItem.SalePrice = product.Price.GetTierPrice(quantity).Price;
                    //List price should be always greater ot equals sale price because it may cause incorrect totals calculation
                    if (lineItem.ListPrice < lineItem.SalePrice)
                    {
                        lineItem.ListPrice = lineItem.SalePrice;
                    }
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

        private async Task AddLineItemAsync(LineItem lineItem)
        {
            var existingLineItem = _cart.Items.FirstOrDefault(li => li.ProductId == lineItem.ProductId);
            if (existingLineItem != null)
            {
                await InnerChangeItemQuantityAsync(existingLineItem, existingLineItem.Quantity + 1);
            }
            else
            {
                lineItem.Id = null;
                _cart.Items.Add(lineItem);
            }
        }


        private void EnsureThatCartExist()
        {
            if (_cart == null)
            {
                throw new StorefrontException("Cart not loaded.");
            }
        }
    }
}
