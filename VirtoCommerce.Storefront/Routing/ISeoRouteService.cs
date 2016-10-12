using VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.Storefront.Routing
{
    public interface ISeoRouteService
    {
        SeoRouteResponse HandleSeoRequest(string seoPath, WorkContext workContext);
    }
}
