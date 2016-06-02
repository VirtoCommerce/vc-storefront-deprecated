using System;
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
            cartBuilder = cartBuilder.GetOrCreateNewTransientCartAsync(workContext.CurrentStore, anonymousCustomer, workContext.CurrentLanguage, workContext.CurrentCurrency).Result;
            Assert.True(cartBuilder.Cart.IsTransient());

            cartBuilder.SaveAsync().Wait();
            var cart = cartBuilder.Cart;
            Assert.False(cart.IsTransient());

            cartBuilder = cartBuilder.GetOrCreateNewTransientCartAsync(workContext.CurrentStore, anonymousCustomer, workContext.CurrentLanguage, workContext.CurrentCurrency).Result;
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

            var cacheManager = new Moq.Mock<ILocalCacheManager>();
            var workContextFactory = new Func<WorkContext>(GetTestWorkContext);
            var promotionEvaluator = new PromotionEvaluator(marketingApi);

            var pricingService = new PricingServiceImpl(workContextFactory, pricingApi, commerceApi);
            var catalogSearchService = new CatalogSearchServiceImpl(workContextFactory, catalogModuleApi, pricingService, inventoryApi, searchApi, promotionEvaluator);

            var retVal = new CartBuilder(cartApi, promotionEvaluator, catalogSearchService, commerceApi, cacheManager.Object);
            return retVal;
        }
    }
}
