using VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.Storefront.Routing
{
    public interface ISeoRouteService
    {
        SeoEntity FindEntityBySeoPath(string seoPath, WorkContext workContext);
    }
}
