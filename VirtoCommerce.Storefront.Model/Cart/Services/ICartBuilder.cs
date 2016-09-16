using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Customer;
using VirtoCommerce.Storefront.Model.Quote;
using VirtoCommerce.Storefront.Model.Stores;

namespace VirtoCommerce.Storefront.Model.Cart.Services
{
    /// <summary>
    /// Represent abstraction for working with customer shopping cart
    /// </summary>
    public interface ICartBuilder
    {
        /// <summary>
        ///  Capture cart and all next changes will be implemented on it
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        ICartBuilder TakeCart(ShoppingCart cart);

        /// <summary>
        /// Load or created new cart for specified parameters and capture it.  All next changes will be implemented on it
        /// </summary>
        /// <param name="store"></param>
        /// <param name="customer"></param>
        /// <param name="language"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        Task LoadOrCreateCartAsync(string cartName, Store store, CustomerInfo customer, Language language, Currency currency);

        /// <summary>
        /// Add new product to cart
        /// </summary>
        /// <param name="product"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        ICartBuilder AddItem(Product product, int quantity);

        /// <summary>
        /// Change cart item qty by product index
        /// </summary>
        /// <param name="lineItemId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        Task ChangeItemQuantityAsync(string lineItemId, int quantity);

        /// <summary>
        /// Change cart item qty by item id
        /// </summary>
        /// <param name="lineItemIndex"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        Task ChangeItemQuantityAsync(int lineItemIndex, int quantity);

        Task ChangeItemsQuantitiesAsync(int[] quantities);

        /// <summary>
        /// Remove item from cart by id
        /// </summary>
        /// <param name="lineItemId"></param>
        /// <returns></returns>
        ICartBuilder RemoveItem(string lineItemId);

        /// <summary>
        /// Apply marketing coupon to captured cart
        /// </summary>
        /// <param name="couponCode"></param>
        /// <returns></returns>
        ICartBuilder AddCoupon(string couponCode);

        /// <summary>
        /// remove exist coupon from cart
        /// </summary>
        /// <returns></returns>
        ICartBuilder RemoveCoupon();

        /// <summary>
        /// Clear cart remove all items and shipments and payments
        /// </summary>
        /// <returns></returns>
        ICartBuilder Clear();

        /// <summary>
        /// Add or update shipment to cart
        /// </summary>
        /// <param name="updateModel"></param>
        /// <returns></returns>
        Task AddOrUpdateShipmentAsync(Shipment shipment);

        /// <summary>
        /// Remove exist shipment from cart
        /// </summary>
        /// <param name="shipmentId"></param>
        /// <returns></returns>
        ICartBuilder RemoveShipment(string shipmentId);

        /// <summary>
        /// Add or update payment in cart
        /// </summary>
        /// <param name="updateModel"></param>
        /// <returns></returns>
        Task AddOrUpdatePaymentAsync(Payment payment);

        /// <summary>
        /// Merge other cart with captured
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        ICartBuilder MergeWithCart(ShoppingCart cart);

        /// <summary>
        /// Remove cart from service
        /// </summary>
        /// <returns></returns>
        Task RemoveCartAsync();

        /// <summary>
        /// Fill current captured cart from RFQ
        /// </summary>
        /// <param name="quoteRequest"></param>
        /// <returns></returns>
        Task FillFromQuoteRequestAsync(QuoteRequest quoteRequest);

        /// <summary>
        /// Returns all available shipment methods for current cart
        /// </summary>
        /// <returns></returns>
        Task<ICollection<ShippingMethod>> GetAvailableShippingMethodsAsync();

        /// <summary>
        /// Returns all available payment methods for current cart
        /// </summary>
        /// <returns></returns>
        Task<ICollection<PaymentMethod>> GetAvailablePaymentMethodsAsync();

        /// <summary>
        /// Evaluate marketing discounts for captured cart
        /// </summary>
        /// <returns></returns>
        Task EvaluatePromotionsAsync();

        /// <summary>
        /// Evaluate taxes  for captured cart
        /// </summary>
        /// <returns></returns>
        Task EvaluateTaxesAsync();


        Task ValidateAsync();

        ShoppingCart Cart { get; }


        Task SaveAsync();
    }
}
