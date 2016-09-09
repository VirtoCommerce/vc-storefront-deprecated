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
using pricingModel = VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi.Models;

namespace VirtoCommerce.Storefront.Owin
{
    /// <summary>
    /// Populate main work context with such commerce data as store, user profile, cart, etc.
    /// </summary>
    public sealed class WorkContextOwinMiddleware : OwinMiddleware
    {
        private static readonly RequireHttps _requireHttps = GetRequireHttps();
        private static readonly PathString[] _owinIgnorePathsStrings = GetOwinIgnorePathStrings();
        private static readonly Country[] _allCountries = GetAllCounries();

        private readonly UnityContainer _container;
        private readonly ILocalCacheManager _cacheManager;

        public WorkContextOwinMiddleware(OwinMiddleware next, UnityContainer container)
            : base(next)
        {
            // WARNING! WorkContextOwinMiddleware is created once when application starts.
            // Don't store any instances which depend on WorkContext because it has a per request lifetime.
            _container = container;
            _cacheManager = container.Resolve<ILocalCacheManager>();
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (_requireHttps.Enabled && !context.Request.IsSecure)
            {
                RedirectToHttps(context);
            }
            else
            {
                if (IsStorefrontRequest(context.Request))
                {
                    await HandleStorefrontRequest(context);
                }

                await Next.Invoke(context);
            }
        }


        private static void RedirectToHttps(IOwinContext context)
        {
            var uriBuilder = new UriBuilder(context.Request.Uri)
            {
                Scheme = Uri.UriSchemeHttps,
                Port = _requireHttps.Port
            };
            context.Response.StatusCode = _requireHttps.StatusCode;
            context.Response.ReasonPhrase = _requireHttps.ReasonPhrase;
            context.Response.Headers["Location"] = uriBuilder.Uri.AbsoluteUri;
        }

        private static bool IsStorefrontRequest(IOwinRequest request)
        {
            return !_owinIgnorePathsStrings.Any(p => request.Path.StartsWithSegments(p));
        }

        private static bool IsAssetRequest(IOwinRequest request)
        {
            var retVal = string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase);
            if (retVal)
            {
                retVal = request.Uri.IsFile || request.Uri.AbsolutePath.Contains("/assets/");
            }
            return retVal;
        }

        private async Task HandleStorefrontRequest(IOwinContext context)
        {
            var workContext = _container.Resolve<WorkContext>();

            // Initialize common properties
            workContext.RequestUrl = context.Request.Uri;
            workContext.AllCountries = _allCountries;
            workContext.AllStores = await _cacheManager.GetAsync("GetAllStores", "ApiRegion", async () => await GetAllStoresAsync(), cacheNullValue: false);

            if (workContext.AllStores != null && workContext.AllStores.Any())
            {
                // Initialize request specific properties
                workContext.CurrentStore = GetStore(context, workContext.AllStores);
                workContext.CurrentLanguage = GetLanguage(context, workContext.AllStores, workContext.CurrentStore);

                var commerceApi = _container.Resolve<ICoreModuleApiClient>();
                workContext.AllCurrencies = await _cacheManager.GetAsync("GetAllCurrencies-" + workContext.CurrentLanguage.CultureName, "ApiRegion", async () => { return (await commerceApi.Commerce.GetAllCurrenciesAsync()).Select(x => x.ToWebModel(workContext.CurrentLanguage)).ToArray(); });

                //Sync store currencies with avail in system
                foreach (var store in workContext.AllStores)
                {
                    store.SyncCurrencies(workContext.AllCurrencies, workContext.CurrentLanguage);
                    store.CurrentSeoInfo = store.SeoInfos.FirstOrDefault(x => x.Language == workContext.CurrentLanguage);
                }

                //Set current currency
                workContext.CurrentCurrency = GetCurrency(context, workContext.CurrentStore);

                var qs = HttpUtility.ParseQueryString(workContext.RequestUrl.Query);
                //Initialize catalog search criteria
                workContext.CurrentCatalogSearchCriteria = new CatalogSearchCriteria(workContext.CurrentLanguage, workContext.CurrentCurrency, qs)
                {
                    CatalogId = workContext.CurrentStore.Catalog
                };
                //Initialize product response group
                workContext.CurrentProductResponseGroup = EnumUtility.SafeParse(qs.Get("resp_group"), ItemResponseGroup.ItemLarge);

                workContext.PageNumber = qs.Get("page").ToNullableInt();
                workContext.PageSize = qs.Get("count").ToNullableInt() ?? qs.Get("page_size").ToNullableInt();

                var catalogSearchService = _container.Resolve<ICatalogSearchService>();

                //This line make delay categories loading initialization (categories can be evaluated on view rendering time)
                workContext.Categories = new MutablePagedList<Category>((pageNumber, pageSize, sortInfos) =>
                {
                    var criteria = new CatalogSearchCriteria(workContext.CurrentLanguage, workContext.CurrentCurrency)
                    {
                        CatalogId = workContext.CurrentStore.Catalog,
                        SearchInChildren = true,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        ResponseGroup = CatalogSearchResponseGroup.WithCategories | CatalogSearchResponseGroup.WithOutlines
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
                            criteria.CategoryId = category.Id;
                            criteria.PageNumber = pageNumber2;
                            criteria.PageSize = pageSize2;
                            if (string.IsNullOrEmpty(criteria.SortBy) && !sortInfos2.IsNullOrEmpty())
                            {
                                criteria.SortBy = SortInfo.ToString(sortInfos2);
                            }
                            var searchResult = catalogSearchService.SearchProducts(criteria);
                            //Because catalog search products returns also aggregations we can use it to populate workContext using C# closure
                            //now workContext.Aggregation will be contains preloaded aggregations for current category
                            workContext.Aggregations = new MutablePagedList<Aggregation>(searchResult.Aggregations);
                            return searchResult.Products;
                        });
                    }
                    return result;
                });
                //This line make delay products loading initialization (products can be evaluated on view rendering time)
                workContext.Products = new MutablePagedList<Product>((pageNumber, pageSize, sortInfos) =>
                {
                    var criteria = workContext.CurrentCatalogSearchCriteria.Clone();
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
                });
                //This line make delay aggregation loading initialization (aggregation can be evaluated on view rendering time)
                workContext.Aggregations = new MutablePagedList<Aggregation>((pageNumber, pageSize, sortInfos) =>
                {
                    var criteria = workContext.CurrentCatalogSearchCriteria.Clone();
                    criteria.PageNumber = pageNumber;
                    criteria.PageSize = pageSize;
                    if (string.IsNullOrEmpty(criteria.SortBy) && !sortInfos.IsNullOrEmpty())
                    {
                        criteria.SortBy = SortInfo.ToString(sortInfos);
                    }
                    //Force to load products and its also populate workContext.Aggregations by preloaded values
                    workContext.Products.Slice(pageNumber, pageSize, sortInfos);
                    return workContext.Aggregations;
                });

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
                    //Shopping cart
                    var cartBuilder = _container.Resolve<ICartBuilder>();
                    await cartBuilder.LoadDefaultCart(workContext.CurrentStore, workContext.CurrentCustomer, workContext.CurrentLanguage, workContext.CurrentCurrency);
                    workContext.CurrentCart = cartBuilder.Cart;

                    if (workContext.CurrentStore.QuotesEnabled)
                    {
                        var quoteRequestBuilder = _container.Resolve<IQuoteRequestBuilder>();
                        await quoteRequestBuilder.GetOrCreateNewTransientQuoteRequestAsync(workContext.CurrentStore, workContext.CurrentCustomer, workContext.CurrentLanguage, workContext.CurrentCurrency);
                        workContext.CurrentQuoteRequest = quoteRequestBuilder.QuoteRequest;
                    }

                    var linkListService = _container.Resolve<IMenuLinkListService>();
                    var linkLists = await _cacheManager.GetAsync("GetAllStoreLinkLists-" + workContext.CurrentStore.Id, "ApiRegion", async () => await linkListService.LoadAllStoreLinkListsAsync(workContext.CurrentStore.Id));
                    workContext.CurrentLinkLists = linkLists.GroupBy(x => x.Name).Select(x => x.FindWithLanguage(workContext.CurrentLanguage)).Where(x => x != null).ToList();
                    // load all static content
                    var staticContents = _cacheManager.Get(string.Join(":", "AllStoreStaticContent", workContext.CurrentStore.Id), "ContentRegion", () =>
                    {
                        var staticContentService = _container.Resolve<IStaticContentService>();
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
                    workContext.CurrentBlogSearchCritera = new BlogSearchCriteria(qs);

                    //Pricelists
                    var pricelistCacheKey = string.Join("-", "EvaluatePriceLists", workContext.CurrentStore.Id, workContext.CurrentCustomer.Id);
                    workContext.CurrentPricelists = await _cacheManager.GetAsync(pricelistCacheKey, "ApiRegion", async () =>
                    {
                        var evalContext = new pricingModel.PriceEvaluationContext
                        {
                            StoreId = workContext.CurrentStore.Id,
                            CatalogId = workContext.CurrentStore.Catalog,
                            CustomerId = workContext.CurrentCustomer.Id,
                            Quantity = 1
                        };

                        var pricingModuleApi = _container.Resolve<IPricingModuleApiClient>();
                        var pricingResult = await pricingModuleApi.PricingModule.EvaluatePriceListsAsync(evalContext);
                        return pricingResult.Select(p => p.ToWebModel()).ToList();
                    });

                    //Vendors with their products
                    workContext.Vendors = new MutablePagedList<Vendor>((pageNumber, pageSize, sortInfos) =>
                    {
                        var customerService = _container.Resolve<ICustomerService>();
                        var vendors = customerService.SearchVendors(null, pageNumber, pageSize, sortInfos);
                        foreach (var vendor in vendors)
                        {
                            vendor.Products = new MutablePagedList<Product>((pageNumber2, pageSize2, sortInfos2) =>
                            {
                                var criteria = new CatalogSearchCriteria
                                {
                                    CatalogId = workContext.CurrentStore.Catalog,
                                    VendorId = vendor.Id,
                                    SearchInChildren = true,
                                    PageNumber = pageNumber2,
                                    PageSize = pageSize2,
                                    SortBy = SortInfo.ToString(sortInfos2),
                                    ResponseGroup = CatalogSearchResponseGroup.WithProducts
                                };
                                var searchResult = catalogSearchService.SearchProducts(criteria);
                                return searchResult.Products;
                            });
                        }
                        return vendors;
                    });
                }
            }
        }

        private static RequireHttps GetRequireHttps()
        {
            return new RequireHttps
            {
                Enabled = ConfigurationManager.AppSettings.GetValue("VirtoCommerce:Storefront:RequireHttps:Enabled", false),
                StatusCode = ConfigurationManager.AppSettings.GetValue("VirtoCommerce:Storefront:RequireHttps:StatusCode", 308),
                ReasonPhrase = ConfigurationManager.AppSettings.GetValue("VirtoCommerce:Storefront:RequireHttps:ReasonPhrase", "Permanent Redirect"),
                Port = ConfigurationManager.AppSettings.GetValue("VirtoCommerce:Storefront:RequireHttps:Port", 443),
            };
        }

        private static PathString[] GetOwinIgnorePathStrings()
        {
            var result = new List<PathString>();

            var owinIgnore = ConfigurationManager.AppSettings["VirtoCommerce:Storefront:OwinIgnore"];

            if (!string.IsNullOrEmpty(owinIgnore))
            {
                result.AddRange(owinIgnore.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => new PathString(p)));
            }

            return result.ToArray();
        }

        private async Task<Store[]> GetAllStoresAsync()
        {
            var storeApi = _container.Resolve<IStoreModuleApiClient>();
            var stores = await storeApi.StoreModule.GetStoresAsync();
            var result = stores.Select(s => s.ToWebModel()).ToArray();
            return result.Any() ? result : null;
        }

        private void ValidateUserStoreLogin(IOwinContext context, CustomerInfo customer, Store currentStore)
        {

            if (customer.IsRegisteredUser && !customer.AllowedStores.IsNullOrEmpty()
                && !customer.AllowedStores.Any(x => string.Equals(x, currentStore.Id, StringComparison.InvariantCultureIgnoreCase)))
            {
                context.Authentication.SignOut();
                context.Authentication.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
            }
        }

        private async Task<CustomerInfo> GetCustomerAsync(IOwinContext context)
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
                    var commerceApi = _container.Resolve<ICoreModuleApiClient>();
                    var user = await commerceApi.StorefrontSecurity.GetUserByNameAsync(identity.Name);
                    if (user != null)
                    {
                        userId = user.Id;
                    }
                }

                if (userId != null)
                {
                    var customerService = _container.Resolve<ICustomerService>();
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

        private void MaintainAnonymousCustomerCookie(IOwinContext context, WorkContext workContext)
        {
            string anonymousCustomerId = context.Request.Cookies[StorefrontConstants.AnonymousCustomerIdCookie];

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

                    // Add anonymous customer cookie for non registered customer
                    anonymousCustomerId = Guid.NewGuid().ToString();
                    workContext.CurrentCustomer.Id = anonymousCustomerId;

                    ///Workaround of the next problem:
                    //You set a cookie in your OWIN middleware, but the cookie is not returned in the response received by a browser.
                    //http://appetere.com/post/owinresponse-cookies-not-set-when-remove-an-httpresponse-cookie
                    //Need to maintain cookies through  owinContext.Response.OnSendingHeaders
                    context.Response.OnSendingHeaders(state =>
                    {
                        context.Response.Cookies.Append(StorefrontConstants.AnonymousCustomerIdCookie, anonymousCustomerId, new CookieOptions { Expires = DateTime.UtcNow.AddDays(30) });
                    },  null);
                 
                }
            }
        }

        private Store GetStore(IOwinContext context, ICollection<Store> stores)
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

        private string GetStoreIdFromUrl(IOwinContext context, ICollection<Store> stores)
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

        private Language GetLanguage(IOwinContext context, ICollection<Store> stores, Store store)
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

        private string GetLanguageFromUrl(IOwinContext context, string[] languages)
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

        private static Currency GetCurrency(IOwinContext context, Store store)
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

        private static Country[] GetAllCounries()
        {
            var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(GetRegionInfo)
                .Where(r => r != null)
                .ToList();

            var countriesJson = File.ReadAllText(HostingEnvironment.MapPath("~/App_Data/countries.json"));
            var countriesDict = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(countriesJson);

            var countries = countriesDict
                .Select(kvp => ParseCountry(kvp, regions))
                .Where(c => c.Code3 != null)
                .ToArray();

            return countries;
        }

        private static RegionInfo GetRegionInfo(CultureInfo culture)
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

        private static Country ParseCountry(KeyValuePair<string, JObject> pair, List<RegionInfo> regions)
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

        private IDictionary<string, object> GetApplicationSettings()
        {
            var appSettings = new Dictionary<string, object>();

            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                appSettings.Add(key, ConfigurationManager.AppSettings[key]);
            }

            return appSettings;
        }
    }
}
