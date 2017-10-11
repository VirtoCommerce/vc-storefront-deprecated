﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.CartModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.SubscriptionModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Converters.Subscriptions;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Cart.Services;
using VirtoCommerce.Storefront.Model.Cart.ValidationErrors;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Catalog.Services;
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
using cartModel = VirtoCommerce.Storefront.AutoRestClients.CartModuleApi.Models;

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
        private readonly ISubscriptionModuleApiClient _subscriptionApi;
        private readonly IProductAvailabilityService _productAvailabilityService;
        private const string _cartCacheRegion = "CartRegion";

        public CartBuilder(
            Func<WorkContext> workContextFactory,
            ICartModuleApiClient cartApi,
            ICatalogSearchService catalogSearchService,
            ILocalCacheManager cacheManager,
            IPromotionEvaluator promotionEvaluator,
            ITaxEvaluator taxEvaluator,
            ISubscriptionModuleApiClient subscriptionApi,
            IProductAvailabilityService productAvailabilityService)
        {
            _cartApi = cartApi;
            _catalogSearchService = catalogSearchService;
            _cacheManager = cacheManager;
            _workContextFactory = workContextFactory;
            _promotionEvaluator = promotionEvaluator;
            _taxEvaluator = taxEvaluator;
            _subscriptionApi = subscriptionApi;
            _productAvailabilityService = productAvailabilityService;
        }

        #region ICartBuilder Members

        public virtual ShoppingCart Cart { get; private set; }

        public virtual Task TakeCartAsync(ShoppingCart cart)
        {
            Cart = cart;
            return Task.FromResult((object)null);
        }

        public virtual async Task LoadOrCreateNewTransientCartAsync(string cartName, Store store, CustomerInfo customer, Language language, Currency currency)
        {
            var cacheKey = GetCartCacheKey(store.Id, cartName, customer.Id, currency.Code);
            var needReevaluate = false;

            Cart = await _cacheManager.GetAsync(cacheKey, _cartCacheRegion, async () =>
            {
                needReevaluate = true;

                var cartSearchCriteria = CreateCartSearchCriteria(cartName, store, customer, language, currency);
                var cartSearchResult = await _cartApi.CartModule.SearchAsync(cartSearchCriteria);

                var cartDto = cartSearchResult.Results.FirstOrDefault();
                var cart = cartDto?.ToShoppingCart(currency, language, customer) ?? CreateCart(cartName, store, customer, language, currency);

                //Load cart payment plan with have same id
                if (store.SubscriptionEnabled)
                {
                    var paymentPlanIds = new[] { cart.Id }.Concat(cart.Items.Select(x => x.ProductId).Distinct()).ToArray();

                    var paymentPlans = (await _subscriptionApi.SubscriptionModule.GetPaymentPlanByIdsAsync(paymentPlanIds)).Select(x => x.ToPaymentPlan()).ToList();
                    cart.PaymentPlan = paymentPlans.FirstOrDefault(x => x.Id == cart.Id);
                    foreach (var lineItem in cart.Items)
                    {
                        lineItem.PaymentPlan = paymentPlans.FirstOrDefault(x => x.Id == lineItem.ProductId);
                    }
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
            EnsureCartExists();

            var isProductAvailable = await _productAvailabilityService.IsAvailable(product, quantity);
            if (isProductAvailable)
            {
                var lineItem = product.ToLineItem(Cart.Language, quantity);
                await AddLineItemAsync(lineItem);
            }
        }

        public virtual async Task ChangeItemQuantityAsync(string id, int quantity)
        {
            EnsureCartExists();

            var lineItem = Cart.Items.FirstOrDefault(i => i.Id == id);
            if (lineItem != null)
            {
                await ChangeItemQuantityAsync(lineItem, quantity);
            }
        }

        public virtual async Task ChangeItemQuantityAsync(int lineItemIndex, int quantity)
        {
            EnsureCartExists();

            var lineItem = Cart.Items.ElementAt(lineItemIndex);
            if (lineItem != null)
            {
                await ChangeItemQuantityAsync(lineItem, quantity);
            }
        }

        public virtual async Task ChangeItemsQuantitiesAsync(int[] quantities)
        {
            EnsureCartExists();

            for (var i = 0; i < quantities.Length; i++)
            {
                var lineItem = Cart.Items.ElementAt(i);
                if (lineItem != null && quantities[i] > 0)
                {
                    await ChangeItemQuantityAsync(lineItem, quantities[i]);
                }
            }
        }

        public virtual Task RemoveItemAsync(string id)
        {
            EnsureCartExists();

            var lineItem = Cart.Items.FirstOrDefault(x => x.Id == id);
            if (lineItem != null)
            {
                Cart.Items.Remove(lineItem);
            }

            return Task.FromResult((object)null);
        }

        public virtual Task AddCouponAsync(string couponCode)
        {
            EnsureCartExists();
            Cart.Coupon = new Coupon { Code = couponCode };
            return Task.FromResult((object)null);
        }

        public virtual Task RemoveCouponAsync()
        {
            EnsureCartExists();
            Cart.Coupon = null;
            return Task.FromResult((object)null);
        }

        public virtual Task ClearAsync()
        {
            EnsureCartExists();
            Cart.Items.Clear();
            return Task.FromResult((object)null);
        }

        public virtual async Task AddOrUpdateShipmentAsync(Shipment shipment)
        {
            EnsureCartExists();

            await RemoveExistingShipmentAsync(shipment);

            shipment.Currency = Cart.Currency;
            Cart.Shipments.Add(shipment);

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

        public virtual Task RemoveShipmentAsync(string shipmentId)
        {
            EnsureCartExists();

            var shipment = Cart.Shipments.FirstOrDefault(s => s.Id == shipmentId);
            if (shipment != null)
            {
                Cart.Shipments.Remove(shipment);
            }

            return Task.FromResult((object)null);
        }

        public virtual async Task AddOrUpdatePaymentAsync(Payment payment)
        {
            EnsureCartExists();

            await RemoveExistingPaymentAsync(payment);

            Cart.Payments.Add(payment);

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

        public virtual Task UpdateCartComment(string comment)
        {
            EnsureCartExists();

            Cart.Comment = comment;

            return Task.FromResult((object)null);
        }

        public virtual async Task MergeWithCartAsync(ShoppingCart cart)
        {
            EnsureCartExists();

            //Reset primary keys for all aggregated entities before merge
            //To prevent insertions same Ids for target cart
            var entities = cart.GetFlatObjectsListWithInterface<IEntity>();
            foreach (var entity in entities)
            {
                entity.Id = null;
            }

            foreach (var lineItem in cart.Items)
            {
                await AddLineItemAsync(lineItem);
            }
            Cart.Coupon = cart.Coupon;

            Cart.Shipments.Clear();
            Cart.Shipments = cart.Shipments;

            Cart.Payments.Clear();
            Cart.Payments = cart.Payments;
        }

        public virtual async Task RemoveCartAsync()
        {
            EnsureCartExists();
            InvalidateCache();
            await _cartApi.CartModule.DeleteCartsAsync(new List<string> { Cart.Id });
        }

        public virtual async Task FillFromQuoteRequestAsync(QuoteRequest quoteRequest)
        {
            EnsureCartExists();

            var productIds = quoteRequest.Items.Select(i => i.ProductId);
            var products = await _catalogSearchService.GetProductsAsync(productIds.ToArray(), ItemResponseGroup.ItemLarge);

            Cart.Items.Clear();
            foreach (var product in products)
            {
                var quoteItem = quoteRequest.Items.FirstOrDefault(i => i.ProductId == product.Id);
                if (quoteItem != null)
                {
                    var lineItem = product.ToLineItem(Cart.Language, (int)quoteItem.SelectedTierPrice.Quantity);
                    lineItem.ListPrice = quoteItem.ListPrice;
                    lineItem.SalePrice = quoteItem.SelectedTierPrice.Price;
                    if (lineItem.ListPrice < lineItem.SalePrice)
                    {
                        lineItem.ListPrice = lineItem.SalePrice;
                    }
                    lineItem.DiscountAmount = lineItem.ListPrice - lineItem.SalePrice;
                    lineItem.IsReadOnly = true;
                    lineItem.Id = null;
                    Cart.Items.Add(lineItem);
                }
            }

            if (quoteRequest.RequestShippingQuote)
            {
                Cart.Shipments.Clear();
                var shipment = new Shipment(Cart.Currency);

                if (quoteRequest.ShippingAddress != null)
                {
                    shipment.DeliveryAddress = quoteRequest.ShippingAddress;
                }

                if (quoteRequest.ShipmentMethod != null)
                {
                    var availableShippingMethods = await GetAvailableShippingMethodsAsync();
                    var availableShippingMethod = availableShippingMethods?.FirstOrDefault(sm => sm.ShipmentMethodCode == quoteRequest.ShipmentMethod.ShipmentMethodCode);

                    if (availableShippingMethod != null)
                    {
                        shipment = quoteRequest.ShipmentMethod.ToCartShipment(Cart.Currency);
                    }
                }
                Cart.Shipments.Add(shipment);
            }

            var payment = new Payment(Cart.Currency)
            {
                Amount = quoteRequest.Totals.GrandTotalInclTax
            };

            if (quoteRequest.BillingAddress != null)
            {
                payment.BillingAddress = quoteRequest.BillingAddress;
            }

            Cart.Payments.Clear();
            Cart.Payments.Add(payment);
        }

        public virtual async Task<ICollection<ShippingMethod>> GetAvailableShippingMethodsAsync()
        {
            var workContext = _workContextFactory();

            //Request available shipping rates 
            var shippingRates = await _cartApi.CartModule.GetAvailableShippingRatesAsync(Cart.Id);
            var retVal = shippingRates.Select(x => x.ToShippingMethod(Cart.Currency, workContext.AllCurrencies)).OrderBy(x => x.Priority).ToList();

            //Evaluate promotions cart and apply rewards for available shipping methods
            var promoEvalContext = Cart.ToPromotionEvaluationContext();
            await _promotionEvaluator.EvaluateDiscountsAsync(promoEvalContext, retVal);

            //Evaluate taxes for available shipping rates
            var taxEvalContext = Cart.ToTaxEvalContext(workContext.CurrentStore);
            taxEvalContext.Lines.Clear();
            taxEvalContext.Lines.AddRange(retVal.SelectMany(x => x.ToTaxLines()));
            await _taxEvaluator.EvaluateTaxesAsync(taxEvalContext, retVal);

            return retVal;
        }

        public virtual async Task<ICollection<PaymentMethod>> GetAvailablePaymentMethodsAsync()
        {
            EnsureCartExists();
            var payments = await _cartApi.CartModule.GetAvailablePaymentMethodsAsync(Cart.Id);
            var retVal = payments.Select(x => x.ToPaymentMethod(Cart)).OrderBy(x => x.Priority).ToList();

            //Evaluate promotions cart and apply rewards for available shipping methods
            var promoEvalContext = Cart.ToPromotionEvaluationContext();
            await _promotionEvaluator.EvaluateDiscountsAsync(promoEvalContext, retVal);

            //Evaluate taxes for available payments 
            var workContext = _workContextFactory();
            var taxEvalContext = Cart.ToTaxEvalContext(workContext.CurrentStore);
            taxEvalContext.Lines.Clear();
            taxEvalContext.Lines.AddRange(retVal.SelectMany(x => x.ToTaxLines()));
            await _taxEvaluator.EvaluateTaxesAsync(taxEvalContext, retVal);

            return retVal;
        }

        public async Task ValidateAsync()
        {
            EnsureCartExists();
            await Task.WhenAll(ValidateCartItemsAsync(), ValidateCartShipmentsAsync());
            Cart.IsValid = Cart.Items.All(x => x.IsValid) && Cart.Shipments.All(x => x.IsValid);
        }

        public virtual async Task EvaluatePromotionsAsync()
        {
            EnsureCartExists();

            bool isReadOnlyLineItems = Cart.Items.Any(i => i.IsReadOnly);
            if (!isReadOnlyLineItems)
            {
                //Get product inventory to fill InStockQuantity parameter of LineItem
                var products = await GetCartProductsAsync();

                foreach (var lineItem in Cart.Items.ToList())
                {
                    var product = products.FirstOrDefault(p => p.Id == lineItem.ProductId);
                    lineItem.InStockQuantity = (int)await _productAvailabilityService.GetAvailableQuantity(product);
                }

                var evalContext = Cart.ToPromotionEvaluationContext();
                await _promotionEvaluator.EvaluateDiscountsAsync(evalContext, new IDiscountable[] { Cart });
            }
        }

        public async Task EvaluateTaxesAsync()
        {
            var workContext = _workContextFactory();
            await _taxEvaluator.EvaluateTaxesAsync(Cart.ToTaxEvalContext(workContext.CurrentStore), new[] { Cart });
        }

        public virtual async Task SaveAsync()
        {
            EnsureCartExists();
            InvalidateCache();

            await EvaluatePromotionsAsync();
            await EvaluateTaxesAsync();

            var cart = Cart.ToShoppingCartDto();
            if (string.IsNullOrEmpty(cart.Id))
            {
                cart = await _cartApi.CartModule.CreateAsync(cart);
            }
            else
            {
                await _cartApi.CartModule.UpdateAsync(cart);
            }
            cart = await _cartApi.CartModule.GetCartByIdAsync(cart.Id);
            Cart = cart.ToShoppingCart(Cart.Currency, Cart.Language, Cart.Customer);
        }

        #endregion

        #region IObserver<UserLoginEvent> Members

        /// <summary>
        /// Merges anonymous cart and logged in customer cart
        /// </summary>
        /// <param name="userLoginEvent"></param>
        public virtual async Task OnNextAsync(UserLoginEvent userLoginEvent)
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
            _cacheManager.Remove(GetCartCacheKey(Cart.StoreId, Cart.Name, Cart.CustomerId, Cart.Currency.Code), _cartCacheRegion);
            _cacheManager.Remove(GetCartCacheKey(Cart.Id), _cartCacheRegion);
        }

        protected virtual string GetCartCacheKey(string cartId)
        {
            return "CartBuilder:" + cartId;
        }

        protected virtual string GetCartCacheKey(string storeId, string cartName, string customerId, string currency)
        {
            return "CartBuilder:" + string.Join(":", storeId, cartName, customerId, currency).ToLowerInvariant();
        }

        protected virtual cartModel.ShoppingCartSearchCriteria CreateCartSearchCriteria(string cartName, Store store, CustomerInfo customer, Language language, Currency currency)
        {
            return new cartModel.ShoppingCartSearchCriteria
            {
                StoreId = store.Id,
                CustomerId = customer.Id,
                Name = cartName,
                Currency = currency.Code,
            };
        }

        protected virtual ShoppingCart CreateCart(string cartName, Store store, CustomerInfo customer, Language language, Currency currency)
        {
            var cart = new ShoppingCart(currency, language)
            {
                CustomerId = customer.Id,
                Name = cartName,
                StoreId = store.Id,
                Language = language,
                IsAnonymous = !customer.IsRegisteredUser,
                CustomerName = customer.IsRegisteredUser ? customer.UserName : StorefrontConstants.AnonymousUsername
            };

            return cart;
        }

        protected virtual async Task ValidateCartItemsAsync()
        {
            var products = await GetCartProductsAsync();

            foreach (var lineItem in Cart.Items.ToList())
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
                    var isProductAvailable = await _productAvailabilityService.IsAvailable(product, lineItem.Quantity);
                    if (!isProductAvailable)
                    {
                        lineItem.IsValid = false;

                        var availableQuantity = await _productAvailabilityService.GetAvailableQuantity(product);
                        lineItem.ValidationErrors.Add(new QuantityError(availableQuantity));
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
            foreach (var shipment in Cart.Shipments.ToArray())
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

        protected virtual Task RemoveExistingPaymentAsync(Payment payment)
        {
            if (payment != null)
            {
                var existingPayment = !payment.IsTransient() ? Cart.Payments.FirstOrDefault(s => s.Id == payment.Id) : null;
                if (existingPayment != null)
                {
                    Cart.Payments.Remove(existingPayment);
                }
            }

            return Task.FromResult((object)null);
        }

        protected virtual Task RemoveExistingShipmentAsync(Shipment shipment)
        {
            if (shipment != null)
            {
                var existShipment = !shipment.IsTransient() ? Cart.Shipments.FirstOrDefault(s => s.Id == shipment.Id) : null;
                if (existShipment != null)
                {
                    Cart.Shipments.Remove(existShipment);
                }
            }

            return Task.FromResult((object)null);
        }

        protected virtual async Task ChangeItemQuantityAsync(LineItem lineItem, int quantity)
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
                    Cart.Items.Remove(lineItem);
                }
            }
        }

        protected virtual async Task AddLineItemAsync(LineItem lineItem)
        {
            var existingLineItem = Cart.Items.FirstOrDefault(li => li.ProductId == lineItem.ProductId);
            if (existingLineItem != null)
            {
                await ChangeItemQuantityAsync(existingLineItem, existingLineItem.Quantity + Math.Max(1, lineItem.Quantity));
            }
            else
            {
                lineItem.Id = null;
                Cart.Items.Add(lineItem);
            }
        }

        protected virtual void EnsureCartExists()
        {
            if (Cart == null)
            {
                throw new StorefrontException("Cart not loaded.");
            }
        }

        protected virtual async Task<Product[]> GetCartProductsAsync()
        {
            var productIds = Cart.Items.Select(i => i.ProductId).ToArray();
            var cacheKey = "CartBuilder.ValidateCartItemsAsync:" + Cart.Id + ":" + string.Join(":", productIds);
            var products = await _cacheManager.GetAsync(cacheKey, "ApiRegion", async () => await _catalogSearchService.GetProductsAsync(productIds, ItemResponseGroup.ItemWithPrices | ItemResponseGroup.ItemWithDiscounts | ItemResponseGroup.Inventory));
            return products;
        }
    }
}
