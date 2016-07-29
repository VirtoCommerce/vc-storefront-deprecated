using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Customer.Services;

namespace VirtoCommerce.Storefront.Controllers
{
    [OutputCache(CacheProfile = "VendorCachingProfile")]
    public class VendorController : StorefrontControllerBase
    {
        private readonly ICustomerService _customerService;

        public VendorController(WorkContext context, IStorefrontUrlBuilder urlBuilder, ICustomerService customerService)
            : base(context, urlBuilder)
        {
            _customerService = customerService;
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
            var vendor = await _customerService.GetVendorByIdAsync(vendorId);

            if (vendor != null)
            {
                WorkContext.CurrentVendor = vendor;
                WorkContext.CurrentPageSeo = new SeoInfo
                {
                    Title = vendor.Name,
                };
                return View("vendor", WorkContext);
            }

            throw new HttpException(404, "Vendor not found. Vendor ID: '" + vendorId + "'");
        }
    }
}
