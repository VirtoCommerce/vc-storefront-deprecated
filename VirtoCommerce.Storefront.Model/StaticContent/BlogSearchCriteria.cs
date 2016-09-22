using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model.StaticContent
{
    public class BlogSearchCriteria : PagedSearchCriteria
    {
        public BlogSearchCriteria()
            : base(new NameValueCollection())
        {
        }

        public BlogSearchCriteria(NameValueCollection queryString)
            : base(queryString)
        {
        }

        public string Category { get; set; }
        public string Tag { get; set; }
        public string Author { get; set; }

        protected virtual void Parse(NameValueCollection queryString)
        {
            Category = queryString.Get("category");
            Tag = queryString.Get("tag");
            Author = queryString.Get("author");        
        }


    }
}
