using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System;
using System.Configuration;
using System.Linq;
using VirtoCommerce.Storefront.AutoRestClients.CartModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CustomerModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.InventoryModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.MarketingModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.QuoteModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.SearchApiModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.StoreModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.SubscriptionModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Services;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Customer;
using VirtoCommerce.Storefront.Model.Catalog.Services;
using VirtoCommerce.Storefront.Model.Tax.Services;

namespace VirtoCommerce.Storefront.Test
{
    public class StorefrontTestBase
    {
        public StorefrontTestBase()
        {
            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(new UnityContainer()));
        }

        protected WorkContext GetTestWorkContext()
        {
            var coreApi = GetCoreApiClient();
            var storeApi = GetStoreApiClient();

            var allStores = storeApi.StoreModule.GetStores().Select(x => x.ToStore());
            var defaultStore = allStores.FirstOrDefault(x => string.Equals(x.Id, "Electronics", StringComparison.InvariantCultureIgnoreCase));
            var currencies = coreApi.Commerce.GetAllCurrencies().Select(x => x.ToCurrency(defaultStore.DefaultLanguage));
            defaultStore.SyncCurrencies(currencies, defaultStore.DefaultLanguage);

            var retVal = new WorkContext
            {
                AllCurrencies = defaultStore.Currencies,
                CurrentLanguage = defaultStore.DefaultLanguage,
                CurrentCurrency = defaultStore.DefaultCurrency,
                CurrentStore = defaultStore,
                CurrentCart = new Model.Cart.ShoppingCart(defaultStore.DefaultCurrency, defaultStore.DefaultLanguage),
                CurrentCustomer = new CustomerInfo
                {
                    Id = Guid.NewGuid().ToString()
                }
            };

            return retVal;
        }

        protected ICartModuleApiClient GetCartApiClient()
        {
            return new CartModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected ICatalogModuleApiClient GetCatalogApiClient()
        {
            return new CatalogModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected ICoreModuleApiClient GetCoreApiClient()
        {
            return new CoreModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected IInventoryModuleApiClient GetInventoryApiClient()
        {
            return new InventoryModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected IMarketingModuleApiClient GetMarketingApiClient()
        {
            return new MarketingModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected IPricingModuleApiClient GetPricingApiClient()
        {
            return new PricingModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected IQuoteModuleApiClient GetQuoteApiClient()
        {
            return new QuoteModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected ISearchApiModuleApiClient GetSearchApiClient()
        {
            return new SearchApiModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected IStoreModuleApiClient GetStoreApiClient()
        {
            return new StoreModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected ICustomerModuleApiClient GetCustomerApiClient()
        {
            return new CustomerModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected IOrdersModuleApiClient GetOrderApiClient()
        {
            return new OrdersModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected ISubscriptionModuleApiClient GetSubscriptionModuleApiClient()
        {
            return new SubscriptionModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected IProductAvailabilityService GetProductAvailabilityService()
        {
            return new ProductAvailabilityService();
        }

        protected ITaxEvaluator GetTaxEvaluator()
        {
            return new TaxEvaluator(GetCoreApiClient());
        }


        protected Uri GetApiBaseUri()
        {
            return new Uri(ConfigurationManager.ConnectionStrings["VirtoCommerceBaseUrl"].ConnectionString);
        }

        protected VirtoCommerceApiRequestHandler GetClientCredentials()
        {
            var apiAppId = ConfigurationManager.AppSettings["vc-public-ApiAppId"];
            var apiSecretKey = ConfigurationManager.AppSettings["vc-public-ApiSecretKey"];
            return new VirtoCommerceApiRequestHandler(new HmacCredentials(apiAppId, apiSecretKey), null);
        }
    }
}
