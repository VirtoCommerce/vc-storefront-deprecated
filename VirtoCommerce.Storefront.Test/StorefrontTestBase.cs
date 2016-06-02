using System;
using System.Configuration;
using System.Linq;
using RestSharp;
using VirtoCommerce.CartModule.Client.Api;
using VirtoCommerce.CatalogModule.Client.Api;
using VirtoCommerce.CoreModule.Client.Api;
using VirtoCommerce.InventoryModule.Client.Api;
using VirtoCommerce.MarketingModule.Client.Api;
using VirtoCommerce.Platform.Client.Security;
using VirtoCommerce.PricingModule.Client.Api;
using VirtoCommerce.QuoteModule.Client.Api;
using VirtoCommerce.SearchModule.Client.Api;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Customer;
using VirtoCommerce.StoreModule.Client.Api;

namespace VirtoCommerce.Storefront.Test
{
    public class StorefrontTestBase
    {
        protected WorkContext GetTestWorkContext()
        {
            var coreApi = GetCoreApiClient();
            var storeApi = GetStoreApiClient();

            var allStores = storeApi.StoreModuleGetStores().Select(x => x.ToWebModel());
            var defaultStore = allStores.FirstOrDefault(x => string.Equals(x.Id, "Electronics", StringComparison.InvariantCultureIgnoreCase));
            var currencies = coreApi.CommerceGetAllCurrencies().Select(x => x.ToWebModel(defaultStore.DefaultLanguage));
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

        protected IVirtoCommerceCartApi GetCartApiClient()
        {
            return new VirtoCommerceCartApi(new CartModule.Client.Client.ApiClient(GetApiBaseUrl(), new CartModule.Client.Client.Configuration(), GetHmacRestRequestHandler()));
        }

        protected IVirtoCommerceCatalogApi GetCatalogApiClient()
        {
            return new VirtoCommerceCatalogApi(new CatalogModule.Client.Client.ApiClient(GetApiBaseUrl(), new CatalogModule.Client.Client.Configuration(), GetHmacRestRequestHandler()));
        }

        protected IVirtoCommerceCoreApi GetCoreApiClient()
        {
            return new VirtoCommerceCoreApi(new CoreModule.Client.Client.ApiClient(GetApiBaseUrl(), new CoreModule.Client.Client.Configuration(), GetHmacRestRequestHandler()));
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

        protected IVirtoCommerceSearchApi GetSearchApiClient()
        {
            return new VirtoCommerceSearchApi(new SearchModule.Client.Client.ApiClient(GetApiBaseUrl(), new SearchModule.Client.Client.Configuration(), GetHmacRestRequestHandler()));
        }

        protected IVirtoCommerceStoreApi GetStoreApiClient()
        {
            return new VirtoCommerceStoreApi(new StoreModule.Client.Client.ApiClient(GetApiBaseUrl(), new StoreModule.Client.Client.Configuration(), GetHmacRestRequestHandler()));
        }


        protected string GetApiBaseUrl()
        {
            return ConfigurationManager.ConnectionStrings["VirtoCommerceBaseUrl"].ConnectionString;
        }

        protected Action<IRestRequest> GetHmacRestRequestHandler()
        {
            var apiAppId = ConfigurationManager.AppSettings["vc-public-ApiAppId"];
            var apiSecretKey = ConfigurationManager.AppSettings["vc-public-ApiSecretKey"];
            return new HmacRestRequestHandler(apiAppId, apiSecretKey).PrepareRequest;
        }
    }
}
