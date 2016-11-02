using System.Collections.Specialized;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model.Customer
{
    public class VendorSearchCriteria : PagedSearchCriteria
    {
        public static int DefaultPageSize { get; set; }

        public VendorSearchCriteria(NameValueCollection queryString)
            : base(queryString, DefaultPageSize)
        {
        }
    }
}
