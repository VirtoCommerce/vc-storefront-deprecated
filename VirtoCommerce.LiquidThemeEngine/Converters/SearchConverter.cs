using System.Linq;
using DotLiquid;
using PagedList;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class SearchConverter
    {
        public static Search ToShopifyModel(this IMutablePagedList<Storefront.Model.Catalog.Product> products, string terms)
        {
            var retVal = new Search
            {
                Results = new MutablePagedList<Drop>((pageNumber, pageSize, sortInfos) =>
                {
                    products.Slice(pageNumber, pageSize, sortInfos);
                    return new StaticPagedList<Drop>(products.Select(x => x.ToShopifyModel()).OfType<Drop>(), products);
                }),
                Terms = terms,
                Performed = true
            };
            return retVal;
        }
    }
}
