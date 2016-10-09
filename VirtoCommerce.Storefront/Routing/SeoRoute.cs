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
                    var entity = _seoRouteService.FindEntityBySeoPath(path, _workContextFactory());
                    if (entity != null)
                    {
                        if (entity.SeoPath.EqualsInvariant(path))
                        {
                            switch (entity.ObjectType)
                            {
                                case "CatalogProduct":
                                    data.Values["controller"] = "Product";
                                    data.Values["action"] = "ProductDetails";
                                    data.Values["productId"] = entity.ObjectId;
                                    break;
                                case "Category":
                                    data.Values["controller"] = "CatalogSearch";
                                    data.Values["action"] = "CategoryBrowsing";
                                    data.Values["categoryId"] = entity.ObjectId;
                                    break;
                                case "Vendor":
                                    data.Values["controller"] = "Vendor";
                                    data.Values["action"] = "VendorDetails";
                                    data.Values["vendorId"] = entity.ObjectId;
                                    break;
                                case "Page":
                                    data.Values["controller"] = "Page";
                                    data.Values["action"] = "GetContentPage";
                                    data.Values["page"] = entity.ObjectInstance;
                                    break;
                            }
                        }
                        else
                        {
                            var response = httpContext.Response;
                            response.Status = "301 Moved Permanently";
                            response.RedirectLocation = _storefrontUrlBuilderFactory().ToAppAbsolute(entity.SeoPath);
                            response.End();
                            data = null;
                        }
                    }
                    else
                    {
                        data.Values["controller"] = "Asset";
                        data.Values["action"] = "HandleStaticFiles";
                    }
                }
            }

            return data;
        }
    }
}
