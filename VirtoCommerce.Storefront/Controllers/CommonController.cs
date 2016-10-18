using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.Storefront.AutoRestClients.PlatformModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.StoreModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Controllers
{
    [OutputCache(CacheProfile = "CommonCachingProfile")]
    public class CommonController : StorefrontControllerBase
    {
        private readonly Country[] _countriesWithoutRegions;
        private readonly IStoreModuleApiClient _storeModuleApi;
        private readonly IPlatformModuleApiClient _platformApi;
        private readonly ILocalCacheManager _cacheManager;

        public CommonController(WorkContext workContext, IStorefrontUrlBuilder urlBuilder, IStoreModuleApiClient storeModuleApi,
                               IPlatformModuleApiClient platformApi, ILocalCacheManager cacheManager)
            : base(workContext, urlBuilder)
        {
            _cacheManager = cacheManager;
            _storeModuleApi = storeModuleApi;
            _platformApi = platformApi;
            _countriesWithoutRegions = workContext.AllCountries
                .Select(c => new Country { Name = c.Name, Code2 = c.Code2, Code3 = c.Code3, RegionType = c.RegionType })
                .ToArray();

        }

        /// <summary>
        /// POST : /resetcache
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ResetCache()
        {
            //check permissions
            if (_platformApi.Security.UserHasAnyPermission(WorkContext.CurrentCustomer.UserName, new[] { "cache:reset" }.ToList(), new List<string>()).Result ?? false)
            {
                _cacheManager.Clear();
                return StoreFrontRedirect("~/");
            }
            return new HttpUnauthorizedResult();
        }

        /// <summary>
        /// GET : /contact
        /// </summary>
        /// <param name="viewName"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult СontactUs(string viewName = "page.contact")
        {
            return View(viewName, WorkContext);
        }

        /// <summary>
        /// POST : /contact
        /// </summary>
        /// <param name="model"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public async Task<ActionResult> СontactUs(ContactUsForm model, string viewName = "page.contact")
        {
            await _storeModuleApi.StoreModule.SendDynamicNotificationAnStoreEmailAsync(model.ToServiceModel(WorkContext));
            WorkContext.ContactUsForm = model;
            if (model.Contact.ContainsKey("RedirectUrl") && model.Contact["RedirectUrl"].Any())
            {
                return StoreFrontRedirect(model.Contact["RedirectUrl"].First());
            }
            return View(viewName, WorkContext);
        }

        /// <summary>
        /// GET: common/setcurrency/{currency}
        /// Set current currency for current user session
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult SetCurrency(string currency, string returnUrl = "")
        {
            var cookie = new HttpCookie(StorefrontConstants.CurrencyCookie)
            {
                HttpOnly = true,
                Value = currency
            };
            var cookieExpires = 24 * 365; // TODO make configurable
            cookie.Expires = DateTime.Now.AddHours(cookieExpires);
            HttpContext.Response.Cookies.Remove(StorefrontConstants.CurrencyCookie);
            HttpContext.Response.Cookies.Add(cookie);

            //home page  and prevent open redirection attack
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = "~/";
            }

            return StoreFrontRedirect(returnUrl);
        }

        // GET: common/getcountries/json
        [HttpGet]
        public ActionResult GetCountries()
        {
            return Json(_countriesWithoutRegions, JsonRequestBehavior.AllowGet);
        }

        // GET: common/getregions/{countryCode}/json
        [HttpGet]
        public ActionResult GetRegions(string countryCode)
        {
            Country country = null;

            if (countryCode != null)
            {
                if (countryCode.Length == 3)
                {
                    country = WorkContext.AllCountries.FirstOrDefault(c => c.Code3.Equals(countryCode, StringComparison.OrdinalIgnoreCase));
                }
                else if (countryCode.Length == 2)
                {
                    country = WorkContext.AllCountries.FirstOrDefault(c => c.Code2.Equals(countryCode, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (country != null)
            {
                return Json(country.Regions, JsonRequestBehavior.AllowGet);
            }

            return HttpNotFound();
        }

        // GET: common/nostore
        [HttpGet]
        public ActionResult NoStore()
        {
            return View("NoStore");
        }

        // GET: /maintenance
        [HttpGet]
        public ActionResult Maintenance()
        {
            return View("maintenance", "empty", WorkContext);
        }
    }
}
