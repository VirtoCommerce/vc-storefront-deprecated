using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Optimization;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.StoreModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart.Services;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Customer;
using VirtoCommerce.Storefront.Model.Customer.Services;
using VirtoCommerce.Storefront.Model.LinkList.Services;
using VirtoCommerce.Storefront.Model.Quote.Services;
using VirtoCommerce.Storefront.Model.Services;
using VirtoCommerce.Storefront.Model.StaticContent;
using VirtoCommerce.Storefront.Model.StaticContent.Services;
using VirtoCommerce.Storefront.Model.Stores;
using VirtoCommerce.Storefront.Services;

namespace VirtoCommerce.Storefront.Owin
{
    /// <summary>
    /// Populate main work context with such commerce data as store, user profile, cart, etc.
    /// </summary>
    public class WorkContextOwinMiddleware : OwinMiddleware
    {
        protected static readonly RequireHttps RequireHttps = GetRequireHttps();
        protected static readonly PathString[] OwinIgnorePathsStrings = GetOwinIgnorePathStrings();
        protected static readonly Country[] AllCountries = GetAllCounries();

        protected UnityContainer Container { get; }
        protected ILocalCacheManager CacheManager { get; }

        public WorkContextOwinMiddleware(OwinMiddleware next, UnityContainer container)
            : base(next)
        {
            // WARNING! WorkContextOwinMiddleware is created once when application starts.
            // Don't store any instances which depend on WorkContext because it has a per request lifetime.
            Container = container;
            CacheManager = container.Resolve<ILocalCacheManager>();
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (RequireHttps.Enabled && !context.Request.IsSecure)
            {
                RedirectToHttps(context);
            }
            else
            {
                if (IsStorefrontRequest(context.Request) && !IsBundleRequest(context.Request))
                {
                    await ClearCacheIfHasChanges();
                    await HandleStorefrontRequest(context);
                }

                await Next.Invoke(context);
            }
        }


        protected virtual void RedirectToHttps(IOwinContext context)
        {
            var uriBuilder = new UriBuilder(context.Request.Uri)
            {
                Scheme = Uri.UriSchemeHttps,
                Port = RequireHttps.Port
            };
            context.Response.StatusCode = RequireHttps.StatusCode;
            context.Response.ReasonPhrase = RequireHttps.ReasonPhrase;
            context.Response.Headers["Location"] = uriBuilder.Uri.AbsoluteUri;
        }

        protected virtual bool IsStorefrontRequest(IOwinRequest request)
        {
            return !OwinIgnorePathsStrings.Any(p => request.Path.StartsWithSegments(p));
        }

        protected virtual bool IsBundleRequest(IOwinRequest request)
        {
            var path = "~" + request.Path;
            return BundleTable.Bundles.Any(b => b.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
        }

        protected virtual bool IsStaticAssetRequest(IOwinRequest request)
        {
            var result = string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase);
            if (result)
            {
                result = request.Uri.AbsolutePath.Contains("/assets/static/");
            }
            return result;
        }

        protected virtual bool IsAssetRequest(IOwinRequest request)
        {
            var retVal = string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase);
            if (retVal)
            {
                retVal = request.Uri.IsFile || request.Uri.AbsolutePath.Contains("/assets/");
            }
            return retVal;
        }

        protected virtual async Task ClearCacheIfHasChanges()
        {
            var changesTrackingService = Container.Resolve<IChangesTrackingService>();

            if (await changesTrackingService.HasChanges())
            {
                CacheManager.Clear();
            }
        }

        protected virtual async Task HandleStorefrontRequest(IOwinContext context)
        {
            var workContext = Container.Resolve<WorkContext>();

            // Initialize common properties
            var qs = HttpUtility.ParseQueryString(context.Request.Uri.Query);
            workContext.RequestUrl = context.Request.Uri;
            workContext.QueryString = qs;
            workContext.AllCountries = AllCountries;
            workContext.AllStores = await CacheManager.GetAsync("GetAllStores", "ApiRegion", async () => await GetAllStoresAsync(), cacheNullValue: false);

            if (workContext.AllStores != null && workContext.AllStores.Any())
            {
                // Initialize request specific properties
                workContext.CurrentStore = GetStore(context, workContext.AllStores);
                workContext.CurrentLanguage = GetLanguage(context, workContext.AllStores, workContext.CurrentStore);

                if (!IsStaticAssetRequest(context.Request))
                {
                    var commerceApi = Container.Resolve<ICoreModuleApiClient>();
                    workContext.AllCurrencies = await CacheManager.GetAsync("GetAllCurrencies-" + workContext.CurrentLanguage.CultureName, "ApiRegion", async () => { return (await commerceApi.Commerce.GetAllCurrenciesAsync()).Select(x => x.ToCurrency(workContext.CurrentLanguage)).ToArray(); });

                    //Sync store currencies with avail in system
                    foreach (var store in workContext.AllStores)
                    {
                        store.SyncCurrencies(workContext.AllCurrencies, workContext.CurrentLanguage);
                        store.CurrentSeoInfo = store.SeoInfos.FirstOrDefault(x => x.Language == workContext.CurrentLanguage);
                    }

                    //Set current currency
                    workContext.CurrentCurrency = GetCurrency(context, workContext.CurrentStore);

                    //Initialize catalog search criteria
                    workContext.CurrentProductSearchCriteria = new ProductSearchCriteria(workContext.CurrentLanguage, workContext.CurrentCurrency, qs);

                    //Initialize product response group. Exclude properties meta-information for performance reason (property values will be returned)
                    workContext.CurrentProductResponseGroup = EnumUtility.SafeParse(qs.Get("resp_group"), ItemResponseGroup.ItemLarge & ~ItemResponseGroup.ItemProperties);

                    workContext.PageNumber = qs.Get("page").ToNullableInt();
                    workContext.PageSize = qs.Get("count").ToNullableInt() ?? qs.Get("page_size").ToNullableInt();

                    var catalogSearchService = Container.Resolve<ICatalogSearchService>();

                    //This line make delay categories loading initialization (categories can be evaluated on view rendering time)
                    workContext.Categories = new MutablePagedList<Category>((pageNumber, pageSize, sortInfos) =>
                    {
                        var criteria = new CategorySearchCriteria(workContext.CurrentLanguage)
                        {
                            PageNumber = pageNumber,
                            PageSize = pageSize,
                            ResponseGroup = CategoryResponseGroup.Small
                        };

                        if (string.IsNullOrEmpty(criteria.SortBy) && !sortInfos.IsNullOrEmpty())
                        {
                            criteria.SortBy = SortInfo.ToString(sortInfos);
                        }
                        var result = catalogSearchService.SearchCategories(criteria);
                        foreach (var category in result)
                        {
                            category.Products = new MutablePagedList<Product>((pageNumber2, pageSize2, sortInfos2) =>
                            {
                                var productSearchCriteria = new ProductSearchCriteria(workContext.CurrentLanguage, workContext.CurrentCurrency)
                                {
                                    PageNumber = pageNumber2,
                                    PageSize = pageSize2,
                                    Outline = category.Outline + "*",
                                    ResponseGroup = workContext.CurrentProductSearchCriteria.ResponseGroup
                                };

                                //criteria.CategoryId = category.Id;
                                if (string.IsNullOrEmpty(criteria.SortBy) && !sortInfos2.IsNullOrEmpty())
                                {
                                    productSearchCriteria.SortBy = SortInfo.ToString(sortInfos2);
                                }

                                var searchResult = catalogSearchService.SearchProducts(productSearchCriteria);

                                //Because catalog search products returns also aggregations we can use it to populate workContext using C# closure
                                //now workContext.Aggregation will be contains preloaded aggregations for current category
                                workContext.Aggregations = new MutablePagedList<Aggregation>(searchResult.Aggregations);
                                return searchResult.Products;
                            }, 1, ProductSearchCriteria.DefaultPageSize);
                        }
                        return result;
                    }, 1, CategorySearchCriteria.DefaultPageSize);

                    //This line make delay products loading initialization (products can be evaluated on view rendering time)
                    workContext.Products = new MutablePagedList<Product>((pageNumber, pageSize, sortInfos) =>
                    {
                        var criteria = workContext.CurrentProductSearchCriteria.Clone();
                        criteria.PageNumber = pageNumber;
                        criteria.PageSize = pageSize;
                        if (string.IsNullOrEmpty(criteria.SortBy) && !sortInfos.IsNullOrEmpty())
                        {
                            criteria.SortBy = SortInfo.ToString(sortInfos);
                        }
                        var result = catalogSearchService.SearchProducts(criteria);
                        //Prevent double api request for get aggregations
                        //Because catalog search products returns also aggregations we can use it to populate workContext using C# closure
                        //now workContext.Aggregation will be contains preloaded aggregations for current search criteria
                        workContext.Aggregations = new MutablePagedList<Aggregation>(result.Aggregations);
                        return result.Products;
                    }, 1, ProductSearchCriteria.DefaultPageSize);

                    //This line make delay aggregation loading initialization (aggregation can be evaluated on view rendering time)
                    workContext.Aggregations = new MutablePagedList<Aggregation>((pageNumber, pageSize, sortInfos) =>
                    {
                        var criteria = workContext.CurrentProductSearchCriteria.Clone();
                        criteria.PageNumber = pageNumber;
                        criteria.PageSize = pageSize;
                        if (string.IsNullOrEmpty(criteria.SortBy) && !sortInfos.IsNullOrEmpty())
                        {
                            criteria.SortBy = SortInfo.ToString(sortInfos);
                        }
                        //Force to load products and its also populate workContext.Aggregations by preloaded values
                        workContext.Products.Slice(pageNumber, pageSize, sortInfos);
                        return workContext.Aggregations;
                    }, 1, ProductSearchCriteria.DefaultPageSize);

                    workContext.CurrentOrderSearchCriteria = new Model.Order.OrderSearchCriteria(qs);
                    workContext.CurrentQuoteSearchCriteria = new Model.Quote.QuoteSearchCriteria(qs);

                    //Get current customer
                    workContext.CurrentCustomer = await GetCustomerAsync(context);
                    //Validate that current customer has to store access
                    ValidateUserStoreLogin(context, workContext.CurrentCustomer, workContext.CurrentStore);
                    MaintainAnonymousCustomerCookie(context, workContext);

                    // Gets the collection of external login providers
                    var externalAuthTypes = context.Authentication.GetExternalAuthenticationTypes();

                    workContext.ExternalLoginProviders = externalAuthTypes.Select(at => new LoginProvider
                    {
                        AuthenticationType = at.AuthenticationType,
                        Caption = at.Caption,
                        Properties = at.Properties
                    }).ToList();

                    workContext.ApplicationSettings = GetApplicationSettings();

                    //Do not load shopping cart and other for resource requests
                    if (!IsAssetRequest(context.Request))
                    {
                        await HandleNonAssetRequest(context, workContext);
                    }
                }
            }
        }

        protected virtual async Task HandleNonAssetRequest(IOwinContext context, WorkContext workContext)
        {
            await InitializeShoppingCart(context, workContext);

            if (workContext.CurrentStore.QuotesEnabled)
            {
                var quoteRequestBuilder = Container.Resolve<IQuoteRequestBuilder>();
                await quoteRequestBuilder.GetOrCreateNewTransientQuoteRequestAsync(workContext.CurrentStore, workContext.CurrentCustomer, workContext.CurrentLanguage, workContext.CurrentCurrency);
                workContext.CurrentQuoteRequest = quoteRequestBuilder.QuoteRequest;
            }

            var linkListService = Container.Resolve<IMenuLinkListService>();
            var linkLists = await CacheManager.GetAsync("GetAllStoreLinkLists-" + workContext.CurrentStore.Id, "ApiRegion", async () => await linkListService.LoadAllStoreLinkListsAsync(workContext.CurrentStore.Id));
            workContext.CurrentLinkLists = linkLists.GroupBy(x => x.Name).Select(x => x.FindWithLanguage(workContext.CurrentLanguage)).Where(x => x != null).ToList();

            // load all static content
            var staticContents = CacheManager.Get(string.Join(":", "AllStoreStaticContent", workContext.CurrentStore.Id), "ContentRegion", () =>
            {
                var staticContentService = Container.Resolve<IStaticContentService>();
                var allContentItems = staticContentService.LoadStoreStaticContent(workContext.CurrentStore).ToList();
                var blogs = allContentItems.OfType<Blog>().ToArray();
                var blogArticlesGroup = allContentItems.OfType<BlogArticle>().GroupBy(x => x.BlogName, x => x).ToList();

                foreach (var blog in blogs)
                {
                    var blogArticles = blogArticlesGroup.FirstOrDefault(x => string.Equals(x.Key, blog.Name, StringComparison.OrdinalIgnoreCase));
                    if (blogArticles != null)
                    {
                        blog.Articles = new MutablePagedList<BlogArticle>(blogArticles);
                    }
                }

                return new { Pages = allContentItems, Blogs = blogs };
            });
            workContext.Pages = new MutablePagedList<ContentItem>(staticContents.Pages.Where(x => x.Language.IsInvariant || x.Language == workContext.CurrentLanguage));
            workContext.Blogs = new MutablePagedList<Blog>(staticContents.Blogs.Where(x => x.Language.IsInvariant || x.Language == workContext.CurrentLanguage));

            // Initialize blogs search criteria 
            workContext.CurrentBlogSearchCritera = new BlogSearchCriteria(workContext.QueryString);

            //Pricelists
            var pricelistCacheKey = string.Join("-", "EvaluatePriceLists", workContext.CurrentStore.Id, workContext.CurrentCustomer.Id);
            workContext.CurrentPricelists = await CacheManager.GetAsync(pricelistCacheKey, "ApiRegion", async () =>
            {
                var evalContext = workContext.ToPriceEvaluationContextDto();
                var pricingModuleApi = Container.Resolve<IPricingModuleApiClient>();
                var pricingResult = await pricingModuleApi.PricingModule.EvaluatePriceListsAsync(evalContext);
                return pricingResult.Select(p => p.ToPricelist(workContext.AllCurrencies, workContext.CurrentLanguage)).ToList();
            });

            // Vendors with their products
            workContext.Vendors = new MutablePagedList<Vendor>((pageNumber, pageSize, sortInfos) =>
            {
                var catalogSearchService = Container.Resolve<ICatalogSearchService>();
                var customerService = Container.Resolve<ICustomerService>();
                var vendors = customerService.SearchVendors(null, pageNumber, pageSize, sortInfos);

                foreach (var vendor in vendors)
                {
                    vendor.Products = new MutablePagedList<Product>((pageNumber2, pageSize2, sortInfos2) =>
                    {
                        var criteria = new ProductSearchCriteria
                        {
                            VendorId = vendor.Id,
                            PageNumber = pageNumber2,
                            PageSize = pageSize2,
                            ResponseGroup = workContext.CurrentProductSearchCriteria.ResponseGroup & ~ItemResponseGroup.ItemWithVendor,
                            SortBy = SortInfo.ToString(sortInfos2),
                        };
                        var searchResult = catalogSearchService.SearchProducts(criteria);
                        return searchResult.Products;
                    }, 1, ProductSearchCriteria.DefaultPageSize);
                }

                return vendors;
            }, 1, VendorSearchCriteria.DefaultPageSize);
        }

        protected virtual async Task InitializeShoppingCart(IOwinContext context, WorkContext workContext)
        {
            var cartBuilder = Container.Resolve<ICartBuilder>();
            await cartBuilder.LoadOrCreateNewTransientCartAsync("default", workContext.CurrentStore, workContext.CurrentCustomer, workContext.CurrentLanguage, workContext.CurrentCurrency);
            workContext.CurrentCart = cartBuilder.Cart;
        }

        protected virtual async Task<Store[]> GetAllStoresAsync()
        {
            var storeApi = Container.Resolve<IStoreModuleApiClient>();
            var stores = await storeApi.StoreModule.GetStoresAsync();
            var result = stores.Select(s => s.ToStore()).ToArray();
            return result.Any() ? result : null;
        }

        protected virtual void ValidateUserStoreLogin(IOwinContext context, CustomerInfo customer, Store currentStore)
        {

            if (customer.IsRegisteredUser && !customer.AllowedStores.IsNullOrEmpty()
                && !customer.AllowedStores.Any(x => string.Equals(x, currentStore.Id, StringComparison.InvariantCultureIgnoreCase)))
            {
                context.Authentication.SignOut();
                context.Authentication.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
            }
        }

        protected virtual async Task<CustomerInfo> GetCustomerAsync(IOwinContext context)
        {
            var retVal = new CustomerInfo();

            var principal = context.Authentication.User;
            var identity = principal.Identity;

            if (identity.IsAuthenticated)
            {
                var userId = identity.GetUserId();
                if (userId == null)
                {
                    //If somehow claim not found in user cookies need load user by name from API
                    var commerceApi = Container.Resolve<ICoreModuleApiClient>();
                    var user = await commerceApi.StorefrontSecurity.GetUserByNameAsync(identity.Name);
                    if (user != null)
                    {
                        userId = user.Id;
                    }
                }

                if (userId != null)
                {
                    var customerService = Container.Resolve<ICustomerService>();
                    var customer = await customerService.GetCustomerByIdAsync(userId);
                    retVal = customer ?? retVal;
                    retVal.Id = userId;
                    retVal.UserName = identity.Name;
                    retVal.IsRegisteredUser = true;
                }

                retVal.OperatorUserId = principal.FindFirstValue(StorefrontConstants.OperatorUserIdClaimType);
                retVal.OperatorUserName = principal.FindFirstValue(StorefrontConstants.OperatorUserNameClaimType);

                var allowedStores = principal.FindFirstValue(StorefrontConstants.AllowedStoresClaimType);
                if (!string.IsNullOrEmpty(allowedStores))
                {
                    retVal.AllowedStores = allowedStores.Split(',');
                }
            }

            if (!retVal.IsRegisteredUser)
            {
                retVal.Id = context.Request.Cookies[StorefrontConstants.AnonymousCustomerIdCookie];
                retVal.UserName = StorefrontConstants.AnonymousUsername;
                retVal.FullName = StorefrontConstants.AnonymousUsername;
            }

            return retVal;
        }

        protected virtual void MaintainAnonymousCustomerCookie(IOwinContext context, WorkContext workContext)
        {
            var anonymousCustomerId = context.Request.Cookies[StorefrontConstants.AnonymousCustomerIdCookie];

            if (workContext.CurrentCustomer.IsRegisteredUser)
            {
                if (!string.IsNullOrEmpty(anonymousCustomerId))
                {
                    // Remove anonymous customer cookie for registered customer
                    context.Response.Cookies.Append(StorefrontConstants.AnonymousCustomerIdCookie, string.Empty, new CookieOptions { Expires = DateTime.UtcNow.AddDays(-30) });
                }
            }
            else
            {
                if (string.IsNullOrEmpty(anonymousCustomerId))
                {
                    // Add anonymous customer cookie for nonregistered customer
                    anonymousCustomerId = Guid.NewGuid().ToString();
                    workContext.CurrentCustomer.Id = anonymousCustomerId;

                    // Workaround for the next problem:
                    // You set a cookie in your OWIN middleware, but the cookie is not returned in the response received by a browser.
                    // http://appetere.com/post/owinresponse-cookies-not-set-when-remove-an-httpresponse-cookie
                    // Need to maintain cookies through owinContext.Response.OnSendingHeaders
                    context.Response.OnSendingHeaders(state =>
                    {
                        context.Response.Cookies.Append(StorefrontConstants.AnonymousCustomerIdCookie, anonymousCustomerId, new CookieOptions { Expires = DateTime.UtcNow.AddDays(30) });
                    }, null);
                }
            }
        }

        protected virtual Store GetStore(IOwinContext context, ICollection<Store> stores)
        {
            //Remove store name from url need to prevent writing store in routing
            var storeId = GetStoreIdFromUrl(context, stores);

            if (string.IsNullOrEmpty(storeId))
            {
                storeId = context.Request.Cookies[StorefrontConstants.StoreCookie];
            }

            if (string.IsNullOrEmpty(storeId))
            {
                storeId = ConfigurationManager.AppSettings["DefaultStore"];
            }

            var store = stores.FirstOrDefault(s => string.Equals(s.Id, storeId, StringComparison.OrdinalIgnoreCase));

            if (store == null)
            {
                store = stores.FirstOrDefault();
            }

            return store;
        }

        protected virtual string GetStoreIdFromUrl(IOwinContext context, ICollection<Store> stores)
        {
            //Try first find by store url (if it defined)
            string retVal = null;

            foreach (var store in stores)
            {
                var pathString = new PathString("/" + store.Id);
                PathString remainingPath;
                if (context.Request.Path.StartsWithSegments(pathString, out remainingPath))
                {
                    retVal = store.Id;
                    break;
                }
            }

            if (retVal == null)
            {
                retVal = stores.Where(x => x.IsStoreUrl(context.Request.Uri)).Select(x => x.Id).FirstOrDefault();
            }

            return retVal;
        }

        protected virtual Language GetLanguage(IOwinContext context, ICollection<Store> stores, Store store)
        {
            var languages = stores.SelectMany(s => s.Languages)
                .Union(stores.Select(s => s.DefaultLanguage))
                .Select(x => x.CultureName)
                .Distinct()
                .ToArray();

            //Get language from request url and remove it from from url need to prevent writing language in routing
            var languageCode = GetLanguageFromUrl(context, languages);

            //Get language from Cookies
            if (string.IsNullOrEmpty(languageCode))
            {
                languageCode = context.Request.Cookies[StorefrontConstants.LanguageCookie];
            }
            var retVal = store.DefaultLanguage;
            //Get store default language if language not in the supported by stores list
            if (!string.IsNullOrEmpty(languageCode))
            {
                var language = new Language(languageCode);
                retVal = store.Languages.Contains(language) ? language : retVal;
            }
            return retVal;
        }

        protected virtual string GetLanguageFromUrl(IOwinContext context, string[] languages)
        {
            var requestPath = context.Request.Path.ToString();
            var regexpPattern = string.Format(@"\/({0})\/?", string.Join("|", languages));
            var match = Regex.Match(requestPath, regexpPattern, RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        protected virtual Currency GetCurrency(IOwinContext context, Store store)
        {
            //Get currency from request url
            var currencyCode = context.Request.Query.Get("currency");
            //Next try get from Cookies
            if (string.IsNullOrEmpty(currencyCode))
            {
                currencyCode = context.Request.Cookies[StorefrontConstants.CurrencyCookie];
            }

            var retVal = store.DefaultCurrency;
            //Get store default currency if currency not in the supported by stores list
            if (!string.IsNullOrEmpty(currencyCode))
            {
                retVal = store.Currencies.FirstOrDefault(x => x.Equals(currencyCode)) ?? retVal;
            }
            return retVal;
        }

        protected virtual IDictionary<string, object> GetApplicationSettings()
        {
            var appSettings = new Dictionary<string, object>();

            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                appSettings.Add(key, ConfigurationManager.AppSettings[key]);
            }

            return appSettings;
        }


        protected static RequireHttps GetRequireHttps()
        {
            return new RequireHttps
            {
                Enabled = ConfigurationManager.AppSettings.GetValue("VirtoCommerce:Storefront:RequireHttps:Enabled", false),
                StatusCode = ConfigurationManager.AppSettings.GetValue("VirtoCommerce:Storefront:RequireHttps:StatusCode", 308),
                ReasonPhrase = ConfigurationManager.AppSettings.GetValue("VirtoCommerce:Storefront:RequireHttps:ReasonPhrase", "Permanent Redirect"),
                Port = ConfigurationManager.AppSettings.GetValue("VirtoCommerce:Storefront:RequireHttps:Port", 443),
            };
        }

        protected static PathString[] GetOwinIgnorePathStrings()
        {
            var result = new List<PathString>();

            var owinIgnore = ConfigurationManager.AppSettings["VirtoCommerce:Storefront:OwinIgnore"];

            if (!string.IsNullOrEmpty(owinIgnore))
            {
                result.AddRange(owinIgnore.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => new PathString(p)));
            }

            return result.ToArray();
        }

        protected static Country[] GetAllCounries()
        {
            Country[] result = null;

            var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(GetRegionInfo)
                .Where(r => r != null)
                .ToList();

            var countriesFilePath = HostingEnvironment.MapPath("~/App_Data/countries.json");
            if (countriesFilePath != null)
            {
                var countriesJson = File.ReadAllText(countriesFilePath);
                var countriesDict = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(countriesJson);

                result = countriesDict
                    .Select(kvp => ParseCountry(kvp, regions))
                    .Where(c => c.Code3 != null)
                    .ToArray();
            }

            return result;
        }

        protected static RegionInfo GetRegionInfo(CultureInfo culture)
        {
            RegionInfo result = null;

            try
            {
                result = new RegionInfo(culture.LCID);
            }
            catch
            {
                // ignored
            }

            return result;
        }

        protected static Country ParseCountry(KeyValuePair<string, JObject> pair, List<RegionInfo> regions)
        {
            var region = regions.FirstOrDefault(r => string.Equals(r.EnglishName, pair.Key, StringComparison.OrdinalIgnoreCase));

            var country = new Country
            {
                Name = pair.Key,
                Code2 = region != null ? region.TwoLetterISORegionName : string.Empty,
                Code3 = region != null ? region.ThreeLetterISORegionName : string.Empty,
                RegionType = pair.Value["label"] != null ? pair.Value["label"].ToString() : null
            };

            var provinceCodes = pair.Value["province_codes"].ToObject<Dictionary<string, string>>();
            if (provinceCodes != null && provinceCodes.Any())
            {
                country.Regions = provinceCodes
                    .Select(kvp => new CountryRegion { Name = kvp.Key, Code = kvp.Value })
                    .ToArray();
            }

            return country;
        }
    }
}
