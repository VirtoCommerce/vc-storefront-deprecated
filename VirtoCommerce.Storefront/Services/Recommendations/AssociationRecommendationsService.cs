using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Recommendations;
using VirtoCommerce.Storefront.Model.Services;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Services.Recommendations
{
    public class AssociationRecommendationsService : IRecommendationsService
    {
        private readonly Func<WorkContext> _workContextFactory;
        private readonly ICatalogSearchService _catalogSearchService;

        public AssociationRecommendationsService(Func<WorkContext> workContextFactory, ICatalogSearchService catalogSearchService)
        {
            _workContextFactory = workContextFactory;
            _catalogSearchService = catalogSearchService;
        }

        public async Task<Product[]> GetRecommendationsAsync(RecommendationsContext context, string type, int skip, int take)
        {
            string[] productIds = context.ProductIds;
            Product[] products = await _catalogSearchService.GetProductsAsync(productIds.ToArray(), ItemResponseGroup.ItemLarge);

            //Need to load related products from associated product and categories
            IEnumerable<Product> productAssociations = products.SelectMany(p => p.Associations.OfType<ProductAssociation>().OrderBy(x => x.Priority)).Select(a => a.Product);
            IList<Product> retVal = productAssociations.Skip(skip).Take(take).ToList();
            var totalCount = productAssociations.Count();

            skip = Math.Max(0, skip - totalCount);
            take = Math.Max(0, take - retVal.Count);

            //Load product from associated categories with correct pagination
            if (take > 0)
            {
                IEnumerable<CategoryAssociation> productCategories = products.SelectMany(p => p.Associations.OfType<CategoryAssociation>().OrderBy(x => x.Priority));
                foreach (var categoryAssociation in productCategories)
                {
                    HandleCategory(categoryAssociation.Category, ref retVal, ref skip, ref take, ref totalCount);

                    if (take == 0)
                        break;
                }
            }

            //Load product from same category
            if (take > 0)
            {
                WorkContext workContext = _workContextFactory();
                string[] categoryIds = products.Select(p => p.CategoryId).Distinct().ToArray();

                foreach (string categoryId in categoryIds)
                {
                    Category category = workContext.Categories.FirstOrDefault(c => c.Id == categoryId);

                    HandleCategory(category, ref retVal, ref skip, ref take, ref totalCount);

                    if (take == 0)
                        break;
                }

                //Load products from other categories
                if (take > 0)
                {
                    foreach (Category category in workContext.Categories)
                    {
                        HandleCategory(category, ref retVal, ref skip, ref take, ref totalCount);

                        if (take == 0)
                            break;
                    }
                }
            }

            return retVal.ToArray();
        }

        private void HandleCategory(Category category, ref IList<Product> products, ref int skip, ref int take, ref int totalCount)
        {
            if (category != null && category.Products != null)
            {
                category.Products.Slice(skip / take + 1, take, null);
                products.AddRange(category.Products);
                totalCount += category.Products.GetTotalCount();
                skip = Math.Max(0, skip - totalCount);
                take = Math.Max(0, take - category.Products.Count());
            }
        }
    }
}