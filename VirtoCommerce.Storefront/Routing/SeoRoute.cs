using System;
using System.Web;
using System.Web.Routing;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Routing
{
    public class SeoRoute : Route
    {
        private readonly Func<WorkContext> _workContextFactory;
        private readonly ISeoRouteService _seoRouteService;
        private readonly Func<IStorefrontUrlBuilder> _storefrontUrlBuilderFactory;

        public SeoRoute(string url, IRouteHandler routeHandler, ISeoRouteService seoRouteService, Func<WorkContext> workContextFactory, Func<IStorefrontUrlBuilder> storefrontUrlBuilderFactory)
            : base(url, routeHandler)
        {
            _workContextFactory = workContextFactory;
            _seoRouteService = seoRouteService;
            _storefrontUrlBuilderFactory = storefrontUrlBuilderFactory;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var data = base.GetRouteData(httpContext);

            if (data != null)
            {
                var path = data.Values["path"] as string;

                if (path != null)
                {
                    var seoRouteResponse = _seoRouteService.HandleSeoRequest(path, _workContextFactory());
                    if (seoRouteResponse != null)
                    {
                        if (seoRouteResponse.Redirect)
                        {
                            var response = httpContext.Response;
                            response.Status = "301 Moved Permanently";
                            response.RedirectLocation = _storefrontUrlBuilderFactory().ToAppAbsolute(seoRouteResponse.RedirectLocation);
                            response.End();
                            data = null;
                        }
                        else if (seoRouteResponse.RouteData != null)
                        {
                            foreach (var kvp in seoRouteResponse.RouteData)
                            {
                                data.Values[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                }
            }

            return data;
        }
    }
}
