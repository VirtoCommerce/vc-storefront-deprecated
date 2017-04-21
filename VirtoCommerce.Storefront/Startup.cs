using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CacheManager.Core;
using CacheManager.Web;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Mvc;
using Newtonsoft.Json;
using NLog;
using Owin;
using VirtoCommerce.LiquidThemeEngine;
using VirtoCommerce.LiquidThemeEngine.Binders;
using VirtoCommerce.Storefront;
using VirtoCommerce.Storefront.App_Start;
using VirtoCommerce.Storefront.AutoRestClients.CacheModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CartModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.ContentModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CustomerModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.InventoryModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.MarketingModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.PlatformModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.QuoteModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.SearchApiModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.SitemapsModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.StoreModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.SubscriptionModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.ProductRecommendationsModuleApi;
using VirtoCommerce.Storefront.BackgroundJobs;
using VirtoCommerce.Storefront.Binders;
using VirtoCommerce.Storefront.Builders;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Controllers;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart.Services;
using VirtoCommerce.Storefront.Model.Catalog.Services;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Common.Events;
using VirtoCommerce.Storefront.Model.Customer.Services;
using VirtoCommerce.Storefront.Model.LinkList.Services;
using VirtoCommerce.Storefront.Model.Marketing.Services;
using VirtoCommerce.Storefront.Model.Order.Events;
using VirtoCommerce.Storefront.Model.Pricing.Services;
using VirtoCommerce.Storefront.Model.Quote.Events;
using VirtoCommerce.Storefront.Model.Quote.Services;
using VirtoCommerce.Storefront.Model.Services;
using VirtoCommerce.Storefront.Model.StaticContent.Services;
using VirtoCommerce.Storefront.Model.Tax.Services;
using VirtoCommerce.Storefront.Owin;
using VirtoCommerce.Storefront.Routing;
using VirtoCommerce.Storefront.Services;
using VirtoCommerce.Tools;
using VirtoCommerce.Storefront.Model.Recommendations;
using VirtoCommerce.Storefront.Services.Recommendations;

[assembly: OwinStartup(typeof(Startup))]
[assembly: PreApplicationStartMethod(typeof(Startup), "PreApplicationStart")]
namespace VirtoCommerce.Storefront
{
    public class Startup
    {
        private static readonly List<string> _directories = new List<string>(new[] { Path.Combine(HostingEnvironment.MapPath("~/App_Data"), Environment.Is64BitProcess ? "x64" : "x86") });
        private static Assembly _managerAssembly;

        public static void PreApplicationStart()
        {
            // TODO: Uncomment if you want to use PerRequestLifetimeManager
            Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(typeof(UnityPerRequestHttpModule));

            AppDomain.CurrentDomain.AssemblyResolve += Resolve;

            var managerAssemblyPath = HostingEnvironment.MapPath("~/Areas/Admin/bin/VirtoCommerce.Platform.Web.dll");
            if (managerAssemblyPath != null && File.Exists(managerAssemblyPath))
            {
                _directories.Add(HostingEnvironment.MapPath("~/Areas/Admin/bin"));

                _managerAssembly = Assembly.LoadFrom(managerAssemblyPath);
                BuildManager.AddReferencedAssembly(_managerAssembly);
            }
        }

        public void Configuration(IAppBuilder app)
        {
            var applicationInsightsKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

            if (!string.IsNullOrEmpty(applicationInsightsKey))
            {
                TelemetryConfiguration.Active.InstrumentationKey = applicationInsightsKey;
            }

            if (_managerAssembly != null)
            {
                AreaRegistration.RegisterAllAreas();
                CallChildConfigure(app, _managerAssembly, "VirtoCommerce.Platform.Web.Startup", "Configuration", "~/areas/admin", "admin/");
            }

            var appSettings = ConfigurationManager.AppSettings;

            UnityWebActivator.Start();
            var container = UnityConfig.GetConfiguredContainer();

            var locator = new UnityServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => locator);

            //Cure for System.Runtime.Caching.MemoryCache freezing 
            //https://www.zpqrtbnk.net/posts/appdomains-threads-cultureinfos-and-paracetamol
            app.SanitizeThreadCulture();
            // Caching configuration
            // Be cautious with SystemWebCacheHandle because it does not work in native threads (Hangfire jobs).
            var localCache = CacheFactory.FromConfiguration<object>("storefrontCache");
            var localCacheManager = new LocalCacheManager(localCache);
            container.RegisterInstance<ILocalCacheManager>(localCacheManager);

            //Because CacheManagerOutputCacheProvider used diff cache manager instance need translate clear region by this way
            //https://github.com/MichaCo/CacheManager/issues/32
            localCacheManager.OnClearRegion += (sender, region) =>
            {
                try
                {
                    CacheManagerOutputCacheProvider.Cache.ClearRegion(region.Region);
                }
                catch { }
            };
            localCacheManager.OnClear += (sender, args) =>
            {
                try
                {
                    CacheManagerOutputCacheProvider.Cache.Clear();
                }
                catch { }
            };

            var distributedCache = CacheFactory.Build("distributedCache", settings =>
      {
          var jsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
          var redisCacheEnabled = appSettings.GetValue("VirtoCommerce:Storefront:RedisCache:Enabled", false);

          var memoryHandlePart = settings
              .WithJsonSerializer(jsonSerializerSettings, jsonSerializerSettings)
              .WithUpdateMode(CacheUpdateMode.Up)
              .WithSystemRuntimeCacheHandle("memory")
              .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromHours(1));

          if (redisCacheEnabled)
          {
              var redisCacheConnectionStringName = appSettings.GetValue("VirtoCommerce:Storefront:RedisCache:ConnectionStringName", "RedisCache");
              var redisConnectionString = ConfigurationManager.ConnectionStrings[redisCacheConnectionStringName].ConnectionString;

              memoryHandlePart
                  .And
                  .WithRedisConfiguration("redis", redisConnectionString)
                  .WithRetryTimeout(100)
                  .WithMaxRetries(1000)
                  .WithRedisBackplane("redis")
                  .WithRedisCacheHandle("redis", true)
                  .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromHours(1));
          }
      });
            var distributedCacheManager = new DistributedCacheManager(distributedCache);
            container.RegisterInstance<IDistributedCacheManager>(distributedCacheManager);

            var logger = LogManager.GetLogger("default");
            container.RegisterInstance<ILogger>(logger);

            // Create new work context for each request
            container.RegisterType<WorkContext, WorkContext>(new PerRequestLifetimeManager());
            Func<WorkContext> workContextFactory = () => container.Resolve<WorkContext>();
            container.RegisterInstance(workContextFactory);

            // Workaround for old storefront base URL: remove /api/ suffix since it is already included in every resource address in VirtoCommerce.Client library.
            var baseUrl = ConfigurationManager.ConnectionStrings["VirtoCommerceBaseUrl"].ConnectionString;
            if (baseUrl != null && baseUrl.EndsWith("/api/", StringComparison.OrdinalIgnoreCase))
            {
                var apiPosition = baseUrl.LastIndexOf("/api/", StringComparison.OrdinalIgnoreCase);
                if (apiPosition >= 0)
                {
                    baseUrl = baseUrl.Remove(apiPosition);
                }
            }

            var apiAppId = appSettings["vc-public-ApiAppId"];
            var apiSecretKey = appSettings["vc-public-ApiSecretKey"];
            container.RegisterInstance(new HmacCredentials(apiAppId, apiSecretKey));

            container.RegisterType<VirtoCommerceApiRequestHandler>(new PerRequestLifetimeManager());

            ServicePointManager.UseNagleAlgorithm = false;

            var compressionHandler = new System.Net.Http.HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            //Timeout for all API requests. Should be small on production to prevent platform API flood.
            var apiRequestTimeout = TimeSpan.Parse(appSettings.GetValue("VirtoCommerce:Storefront:ApiRequest:Timeout", "0:0:30"));

            var baseUri = new Uri(baseUrl);
            container.RegisterType<ICacheModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new CacheModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<ICartModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new CartModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<ICatalogModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new CatalogModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<IContentModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new ContentModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<ICoreModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new CoreModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<ICustomerModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new CustomerModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<IInventoryModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new InventoryModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<IMarketingModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new MarketingModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<IOrdersModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new OrdersModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<IPlatformModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new PlatformModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<IPricingModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new PricingModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<IQuoteModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new QuoteModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<ISearchApiModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new SearchApiModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<ISitemapsModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new SitemapsModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<IStoreModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new StoreModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<ISubscriptionModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new SubscriptionModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).DisableRetries().WithTimeout(apiRequestTimeout)));
            container.RegisterType<IProductRecommendationsModuleApiClient>(new PerRequestLifetimeManager(), new InjectionFactory(c => new ProductRecommendationsModuleApiClient(baseUri, c.Resolve<VirtoCommerceApiRequestHandler>(), compressionHandler).WithTimeout(apiRequestTimeout)));

            container.RegisterType<IMarketingService, MarketingServiceImpl>();
            container.RegisterType<IPromotionEvaluator, PromotionEvaluator>();
            container.RegisterType<ITaxEvaluator, TaxEvaluator>();
            container.RegisterType<IPricingService, PricingServiceImpl>();
            container.RegisterType<ICustomerService, CustomerServiceImpl>();
            container.RegisterType<IMenuLinkListService, MenuLinkListServiceImpl>();
            container.RegisterType<ISeoRouteService, SeoRouteService>();
            container.RegisterType<IProductAvailabilityService, ProductAvailabilityService>();

            container.RegisterType<ICartBuilder, CartBuilder>();
            container.RegisterType<IQuoteRequestBuilder, QuoteRequestBuilder>();
            container.RegisterType<ICatalogSearchService, CatalogSearchServiceImpl>();
            container.RegisterType<IAuthenticationManager>(new InjectionFactory(context => HttpContext.Current.GetOwinContext().Authentication));
            container.RegisterType<IUrlBuilder, UrlBuilder>();
            container.RegisterType<IStorefrontUrlBuilder, StorefrontUrlBuilder>(new PerRequestLifetimeManager());

            container.RegisterType<IRecommendationsService, CognitiveRecommendationsService>("Cognitive");
            container.RegisterType<IRecommendationsService, AssociationRecommendationsService>("Association");

            //Register domain events
            container.RegisterType<IEventPublisher<OrderPlacedEvent>, EventPublisher<OrderPlacedEvent>>();
            container.RegisterType<IEventPublisher<UserLoginEvent>, EventPublisher<UserLoginEvent>>();
            container.RegisterType<IEventPublisher<QuoteRequestUpdatedEvent>, EventPublisher<QuoteRequestUpdatedEvent>>();
            //Register event handlers (observers)
            container.RegisterType<IAsyncObserver<OrderPlacedEvent>, CustomerServiceImpl>("Invalidate customer cache when user placed new order");
            container.RegisterType<IAsyncObserver<QuoteRequestUpdatedEvent>, CustomerServiceImpl>("Invalidate customer cache when quote request was updated");
            container.RegisterType<IAsyncObserver<UserLoginEvent>, CartBuilder>("Merge anonymous cart with loggined user cart");
            container.RegisterType<IAsyncObserver<UserLoginEvent>, QuoteRequestBuilder>("Merge anonymous quote request with loggined user quote");

            var cmsContentConnectionString = BlobConnectionString.Parse(ConfigurationManager.ConnectionStrings["ContentConnectionString"].ConnectionString);
            var themesBasePath = cmsContentConnectionString.RootPath.TrimEnd('/') + "/" + "Themes";
            var staticContentBasePath = cmsContentConnectionString.RootPath.TrimEnd('/') + "/" + "Pages";
            //Use always file system provider for global theme
            var globalThemesBlobProvider = new FileSystemContentBlobProvider(ResolveLocalPath("~/App_Data/Themes/default"));
            IContentBlobProvider themesBlobProvider;
            IStaticContentBlobProvider staticContentBlobProvider;
            if ("AzureBlobStorage".Equals(cmsContentConnectionString.Provider, StringComparison.OrdinalIgnoreCase))
            {
                themesBlobProvider = new AzureBlobContentProvider(cmsContentConnectionString.ConnectionString, themesBasePath, localCacheManager);
                staticContentBlobProvider = new AzureBlobContentProvider(cmsContentConnectionString.ConnectionString, staticContentBasePath, localCacheManager);
            }
            else
            {
                themesBlobProvider = new FileSystemContentBlobProvider(ResolveLocalPath(themesBasePath));
                staticContentBlobProvider = new FileSystemContentBlobProvider(ResolveLocalPath(staticContentBasePath));
            }
            container.RegisterInstance(staticContentBlobProvider);

            var shopifyLiquidEngine = new ShopifyLiquidThemeEngine(localCacheManager, workContextFactory, () => container.Resolve<IStorefrontUrlBuilder>(), themesBlobProvider, globalThemesBlobProvider, "~/themes/assets", "~/themes/global/assets");
            container.RegisterInstance<ILiquidThemeEngine>(shopifyLiquidEngine);

            //Register liquid engine
            ViewEngines.Engines.Add(new DotLiquidThemedViewEngine(shopifyLiquidEngine));

            // Shopify model binders convert Shopify form fields with bad names to VirtoCommerce model properties.
            container.RegisterType<IModelBinderProvider, ShopifyModelBinderProvider>("shopify");

            //Static content service
            var staticContentService = new StaticContentServiceImpl(shopifyLiquidEngine, localCacheManager, workContextFactory, () => container.Resolve<IStorefrontUrlBuilder>(), StaticContentItemFactory.GetContentItemFromPath, staticContentBlobProvider);
            container.RegisterInstance<IStaticContentService>(staticContentService);
            //Register generate sitemap background job
            container.RegisterType<GenerateSitemapJob>(new InjectionFactory(c => new GenerateSitemapJob(themesBlobProvider, c.Resolve<ISitemapsModuleApiClient>(), c.Resolve<IStorefrontUrlBuilder>())));

            var changesTrackingEnabled = ConfigurationManager.AppSettings.GetValue("VirtoCommerce:Storefront:ChangesTracking:Enabled", true);
            var changesTrackingInterval = TimeSpan.Parse(ConfigurationManager.AppSettings.GetValue("VirtoCommerce:Storefront:ChangesTracking:Interval", "0:1:0"));
            var changesTrackingService = new ChangesTrackingService(changesTrackingEnabled, changesTrackingInterval, container.Resolve<ICacheModuleApiClient>());
            container.RegisterInstance<IChangesTrackingService>(changesTrackingService);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters, workContextFactory, () => container.Resolve<CommonController>());
            RouteConfig.RegisterRoutes(RouteTable.Routes, container.Resolve<ISeoRouteService>(), workContextFactory, () => container.Resolve<IStorefrontUrlBuilder>());
            AuthConfig.ConfigureAuth(app, () => container.Resolve<IStorefrontUrlBuilder>());
            var bundleConfig = container.Resolve<BundleConfig>();
            bundleConfig.Minify = ConfigurationManager.AppSettings.GetValue("VirtoCommerce:Storefront:OptimizeStaticContent", false);
            bundleConfig.RegisterBundles(BundleTable.Bundles);

            //This special binders need because all these types not contains default ctor and Money with Currency properties
            ModelBinders.Binders.Add(typeof(Model.Cart.Shipment), new CartModelBinder<Model.Cart.Shipment>(workContextFactory));
            ModelBinders.Binders.Add(typeof(Model.Cart.Payment), new CartModelBinder<Model.Cart.Payment>(workContextFactory));
            ModelBinders.Binders.Add(typeof(Model.Order.PaymentIn), new OrderModelBinder<Model.Order.PaymentIn>(workContextFactory));
            ModelBinders.Binders.Add(typeof(Model.Recommendations.RecommendationEvalContext), new ReccomendationsModelBinder<Model.Recommendations.RecommendationEvalContext>());
            app.Use<WorkContextOwinMiddleware>(container);
            app.UseStageMarker(PipelineStage.PostAuthorize);
            app.Use<StorefrontUrlRewriterOwinMiddleware>(container);
            app.UseStageMarker(PipelineStage.PostAuthorize);
        }


        private static void CallChildConfigure(IAppBuilder app, Assembly assembly, string typeName, string methodName, string virtualRoot, string routPrefix)
        {
            var type = assembly.GetType(typeName);
            if (type != null)
            {
                var methodInfo = type.GetMethod(methodName, new[] { typeof(IAppBuilder), typeof(string), typeof(string) });
                if (methodInfo != null)
                {
                    var classInstance = Activator.CreateInstance(type, null);
                    var parameters = new object[] { app, virtualRoot, routPrefix };
                    methodInfo.Invoke(classInstance, parameters);
                }
            }
        }

        private static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;

            var assemblyName = new AssemblyName(args.Name);
            var fileName = assemblyName.Name + ".dll";

            foreach (var directoryPath in _directories)
            {
                var filePath = Path.Combine(directoryPath, fileName);
                if (File.Exists(filePath))
                {
                    assembly = Assembly.LoadFrom(filePath);
                    break;
                }
            }

            return assembly;
        }

        private static string ResolveLocalPath(string path)
        {
            string result;

            if (path.StartsWith("~"))
            {
                result = HostingEnvironment.MapPath(path);
            }
            else if (Path.IsPathRooted(path))
            {
                result = path;
            }
            else
            {
                result = HostingEnvironment.MapPath("~/");
                result += path;
            }

            return result;
        }
    }
}
