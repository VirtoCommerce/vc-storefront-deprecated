using System;
using System.Collections.Generic;

namespace VirtoCommerce.Storefront.Routing
{
    public class SeoRouteResponse
    {
        public SeoRouteResponse()
        {
            RouteData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public bool Redirect { get; set; }
        public string RedirectLocation { get; set; }
        public IDictionary<string, object> RouteData { get; private set; }
    }
}
