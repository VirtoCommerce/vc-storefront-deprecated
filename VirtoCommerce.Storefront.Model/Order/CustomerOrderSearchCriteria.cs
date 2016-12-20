using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model.Order
{
    public class CustomerOrderSearchCriteria : PagedSearchCriteria
    {
        private static int _defaultPageSize = 20;

        public static int DefaultPageSize
        {
            get { return _defaultPageSize; }
            set { _defaultPageSize = value; }
        }
        public CustomerOrderSearchCriteria()
            :base(new NameValueCollection(), _defaultPageSize)
        {
        }

        public CustomerOrderSearchCriteria(NameValueCollection queryString)
            : base(queryString, DefaultPageSize)
        {
        }

        public SortInfo[] SortInfos { get; set; }
    }
}
