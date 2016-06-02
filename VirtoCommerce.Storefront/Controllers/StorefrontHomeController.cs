using System.Web.Mvc;
using VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.Storefront.Controllers
{
    [OutputCache(CacheProfile = "HomeCachingProfile")]
    public class StorefrontHomeController : Controller
    {
        private readonly WorkContext _workContext;

        public StorefrontHomeController(WorkContext context)
        {
            _workContext = context;
        }

        [HttpGet]
        public ActionResult Index()
        {
            //Load categories for main page (may be removed if it not necessary)
            //var catalogSearchCriteria = new CatalogSearchCriteria()
            //{
            //    CatalogId = _workContext.CurrentStore.Catalog,
            //    ResponseGroup = CatalogSearchResponseGroup.WithCategories,
            //    SortBy = "Priority",
            //    SearchInChildren = false
            //};

            // _workContext.CurrentCatalogSearchResult = await _catalogSearchService.SearchAsync(catalogSearchCriteria);
            return View("index", _workContext);
        }
    }
}
