using System.Collections.Specialized;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model.Quote
{
    public class QuoteSearchCriteria : PagedSearchCriteria
    {
        public static int DefaultPageSize { get; set; }

        public QuoteSearchCriteria(NameValueCollection queryString)
            : base(queryString, DefaultPageSize)
        {
        }
    }
}
