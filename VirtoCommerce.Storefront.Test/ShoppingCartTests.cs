using System;
using Moq;
using VirtoCommerce.Storefront.Builders;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart.Services;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Customer;
using VirtoCommerce.Storefront.Services;
using Xunit;

namespace VirtoCommerce.Storefront.Test
{
    public class ShoppingCartTests : StorefrontTestBase
    {
        [Fact]
        public void CreateNewCart_AnonymousUser_CheckThatNewCartCreated()
        {
            var cartBuilder = GetCartBuilder();
            var workContext = GetTestWorkContext();
            var anonymousCustomer = new CustomerInfo
            {
                Id = Guid.NewGuid().ToString(),
                IsRegisteredUser = false
            };
            var task = cartBuilder.LoadOrCreateCartAsync("default", workContext.CurrentStore, anonymousCustomer, workContext.CurrentLanguage, workContext.CurrentCurrency);
            task.Wait();
            Assert.True(cartBuilder.Cart.IsTransient());

            //cartBuilder.SaveAsync().Wait();
            var cart = cartBuilder.Cart;
            Assert.False(cart.IsTransient());

            task = cartBuilder.LoadOrCreateCartAsync("default", workContext.CurrentStore, anonymousCustomer, workContext.CurrentLanguage, workContext.CurrentCurrency);
            task.Wait();
            Assert.Equal(cart.Id, cartBuilder.Cart.Id);
        }

        [Fact]
        public void ManageItemsInCart_AnonymousUser_CheckThatItemsAndTotalsChanged()
        {
        }

        [Fact]
        public void MergeAnonymousCart_RegisteredUser_CheckThatAllMerged()
        {
        }

        [Fact]
        public void SingleShipmentAndPaymentWithCouponCheckout_RegisteredUser_CheckOrderCreated()
        {
        }

        [Fact]
        public void MultipleShipmentAndPartialPaymentWithCouponCheckout_RegisteredUser_CheckOrderCreated()
        {
        }

        private ICartBuilder GetCartBuilder()
        {
            var catalogModuleApi = GetCatalogApiClient();
            var cartApi = GetCartApiClient();
            var commerceApi = GetCoreApiClient();
            var marketingApi = GetMarketingApiClient();
            var inventoryApi = GetInventoryApiClient();
            var pricingApi = GetPricingApiClient();
            var searchApi = GetSearchApiClient();
            var customerApi = GetCustomerApiClient();
            var orderApi = GetOrderApiClient();
            var quoteApi = GetQuoteApiClient();
            var storeApi = GetStoreApiClient();

            var cacheManager = new Mock<ILocalCacheManager>().Object;
            var workContextFactory = new Func<WorkContext>(GetTestWorkContext);
            var promotionEvaluator = new PromotionEvaluator(marketingApi);

            var pricingService = new PricingServiceImpl(workContextFactory, pricingApi, null);
            var customerService = new CustomerServiceImpl(workContextFactory, customerApi, orderApi, quoteApi, storeApi, cacheManager);
            var catalogSearchService = new CatalogSearchServiceImpl(workContextFactory, catalogModuleApi, pricingService, inventoryApi, searchApi, promotionEvaluator, customerService);

            var retVal = new CartBuilder(cartApi, catalogSearchService,  cacheManager, workContextFactory, null, null, null);
            return retVal;
        }
    }
}
