using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Services;

namespace VirtoCommerce.Storefront.Controllers
{
    [OutputCache(CacheProfile = "CatalogSearchCachingProfile")]
    public class CatalogSearchController : StorefrontControllerBase
    {
        private readonly ICatalogSearchService _searchService;

        public CatalogSearchController(WorkContext workContext, IStorefrontUrlBuilder urlBuilder, ICatalogSearchService searchService)
            : base(workContext, urlBuilder)
        {
            _searchService = searchService;
        }

        /// GET search
        /// This method used for search products by given criteria 
        /// <returns></returns>
        public ActionResult SearchProducts()
        {
            //All resulting categories, products and aggregations will be lazy evaluated when view will be rendered. (workContext.Products, workContext.Categories etc) 
            //All data will loaded using by current search criteria taken from query string
            return View("search", WorkContext);
        }

        /// <summary>
        /// GET search/{categoryId}?view=...
        /// This method called from SeoRoute when url contains slug for category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public async Task<ActionResult> CategoryBrowsing(string categoryId, string view)
        {
            var category = (await _searchService.GetCategoriesAsync(new[] { categoryId }, CategoryResponseGroup.Full)).FirstOrDefault();
            if(category == null)
            {
                throw new HttpException(404, String.Format("Category {0} not found.", categoryId));
            }

            WorkContext.CurrentCategory = category;
            WorkContext.CurrentPageSeo = category.SeoInfo.JsonClone();
            WorkContext.CurrentPageSeo.Slug = category.Url;

            var criteria = WorkContext.CurrentProductSearchCriteria.Clone();
            criteria.Outline = string.Format("{0}*", category.Outline); // should we simply take it from current category?
       
            if (category != null)
            {
                category.Products = new MutablePagedList<Product>((pageNumber, pageSize, sortInfos) =>
                {                 
                    criteria.PageNumber = pageNumber;
                    criteria.PageSize = pageSize;
                    if (string.IsNullOrEmpty(criteria.SortBy) && !sortInfos.IsNullOrEmpty())
                    {
                        criteria.SortBy = SortInfo.ToString(sortInfos);
                    }
                    var result = _searchService.SearchProducts(criteria);
                    //Prevent double api request for get aggregations
                    //Because catalog search products returns also aggregations we can use it to populate workContext using C# closure
                    //now workContext.Aggregation will be contains preloaded aggregations for current search criteria
                    WorkContext.Aggregations = new MutablePagedList<Aggregation>(result.Aggregations);
                    return result.Products;
                }, 1, ProductSearchCriteria.DefaultPageSize);
                               

                // make sure title is set
                if(string.IsNullOrEmpty(WorkContext.CurrentPageSeo.Title))
                {
                    WorkContext.CurrentPageSeo.Title = category.Name;
                }
            }

            if (string.IsNullOrEmpty(view))
            {
                view = "grid";
            }

            if (view.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                return View("collection.list", WorkContext);
            }

            return View("collection", WorkContext);
        }
    }
}
