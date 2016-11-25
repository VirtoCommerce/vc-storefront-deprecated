using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Customer.Services;
using VirtoCommerce.Storefront.Model.Services;

namespace VirtoCommerce.Storefront.Controllers
{
    [OutputCache(CacheProfile = "VendorCachingProfile")]
    public class VendorController : StorefrontControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ICatalogSearchService _catalogSearchService;

        public VendorController(WorkContext context, IStorefrontUrlBuilder urlBuilder, ICustomerService customerService, ICatalogSearchService catalogSearchService)
            : base(context, urlBuilder)
        {
            _customerService = customerService;
            _catalogSearchService = catalogSearchService;
        }

        /// <summary>
        /// GET: /vendor/{vendorId}
        /// This action is used by storefront to get vendor details by vendor ID
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> VendorDetails(string vendorId)
        {
            var vendor = (await _customerService.GetVendorsByIdsAsync(base.WorkContext.CurrentStore, base.WorkContext.CurrentLanguage, vendorId)).FirstOrDefault();

            if (vendor != null)
            {
                vendor.Products = new MutablePagedList<Product>((pageNumber, pageSize, sortInfos) =>
                {
                    var criteria = new ProductSearchCriteria
                    {
                        VendorId = vendorId,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        SortBy = SortInfo.ToString(sortInfos),
                        ResponseGroup = base.WorkContext.CurrentProductSearchCriteria.ResponseGroup
                    };

                    var searchResult = _catalogSearchService.SearchProducts(criteria);
                    return searchResult.Products;
                }, 1, ProductSearchCriteria.DefaultPageSize);

                WorkContext.CurrentPageSeo = vendor.SeoInfo;
                WorkContext.CurrentVendor = vendor;

                return View("vendor", WorkContext);
            }

            throw new HttpException(404, "Vendor not found. Vendor ID: '" + vendorId + "'");
        }
    }
}
