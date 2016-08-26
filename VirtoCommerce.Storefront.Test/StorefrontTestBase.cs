using System;
using System.Configuration;
using System.Linq;
using RestSharp;
using VirtoCommerce.InventoryModule.Client.Api;
using VirtoCommerce.MarketingModule.Client.Api;
using VirtoCommerce.OrderModule.Client.Api;
using VirtoCommerce.Platform.Client.Security;
using VirtoCommerce.PricingModule.Client.Api;
using VirtoCommerce.QuoteModule.Client.Api;
using VirtoCommerce.Storefront.AutoRestClients;
using VirtoCommerce.Storefront.AutoRestClients.CartModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CustomerModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.SearchModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.StoreModuleApi;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Customer;

namespace VirtoCommerce.Storefront.Test
{
    public class StorefrontTestBase
    {
        protected WorkContext GetTestWorkContext()
        {
            var coreApi = GetCoreApiClient();
            var storeApi = GetStoreApiClient();

            var allStores = storeApi.StoreModule.GetStores().Select(x => x.ToWebModel());
            var defaultStore = allStores.FirstOrDefault(x => string.Equals(x.Id, "Electronics", StringComparison.InvariantCultureIgnoreCase));
            var currencies = coreApi.Commerce.GetAllCurrencies().Select(x => x.ToWebModel(defaultStore.DefaultLanguage));
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

        protected IVirtoCommerceInventoryApi GetInventoryApiClient()
        {
            return new VirtoCommerceInventoryApi(new InventoryModule.Client.Client.ApiClient(GetApiBaseUrl(), new InventoryModule.Client.Client.Configuration(), GetHmacRestRequestHandler()));
        }

        protected IVirtoCommerceMarketingApi GetMarketingApiClient()
        {
            return new VirtoCommerceMarketingApi(new MarketingModule.Client.Client.ApiClient(GetApiBaseUrl(), new MarketingModule.Client.Client.Configuration(), GetHmacRestRequestHandler()));
        }

        protected IVirtoCommercePricingApi GetPricingApiClient()
        {
            return new VirtoCommercePricingApi(new PricingModule.Client.Client.ApiClient(GetApiBaseUrl(), new PricingModule.Client.Client.Configuration(), GetHmacRestRequestHandler()));
        }

        protected IVirtoCommerceQuoteApi GetQuoteApiClient()
        {
            return new VirtoCommerceQuoteApi(new QuoteModule.Client.Client.ApiClient(GetApiBaseUrl(), new QuoteModule.Client.Client.Configuration(), GetHmacRestRequestHandler()));
        }

        protected ISearchModuleApiClient GetSearchApiClient()
        {
            return new SearchModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected IStoreModuleApiClient GetStoreApiClient()
        {
            return new StoreModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected ICustomerModuleApiClient GetCustomerApiClient()
        {
            return new CustomerModuleApiClient(GetApiBaseUri(), GetClientCredentials());
        }

        protected IVirtoCommerceOrdersApi GetOrderApiClient()
        {
            return new VirtoCommerceOrdersApi(new OrderModule.Client.Client.ApiClient(GetApiBaseUrl(), new OrderModule.Client.Client.Configuration(), GetHmacRestRequestHandler()));
        }


        protected Uri GetApiBaseUri()
        {
            return new Uri(ConfigurationManager.ConnectionStrings["VirtoCommerceBaseUrl"].ConnectionString);
        }

        protected string GetApiBaseUrl()
        {
            return ConfigurationManager.ConnectionStrings["VirtoCommerceBaseUrl"].ConnectionString;
        }

        protected VirtoCommerceApiRequestHandler GetClientCredentials()
        {
            var apiAppId = ConfigurationManager.AppSettings["vc-public-ApiAppId"];
            var apiSecretKey = ConfigurationManager.AppSettings["vc-public-ApiSecretKey"];
            return new VirtoCommerceApiRequestHandler(new HmacCredentials(apiAppId, apiSecretKey), null);
        }

        protected Action<IRestRequest> GetHmacRestRequestHandler()
        {
            var apiAppId = ConfigurationManager.AppSettings["vc-public-ApiAppId"];
            var apiSecretKey = ConfigurationManager.AppSettings["vc-public-ApiSecretKey"];
            return new HmacRestRequestHandler(apiAppId, apiSecretKey).PrepareRequest;
        }
    }
}
