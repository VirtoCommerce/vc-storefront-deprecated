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

        #region IRecommendationsService members
        public string ProviderName
        {
            get
            {
                return "Association";
            }
        }
        public async Task<Product[]> GetRecommendationsAsync(RecommendationEvalContext context)
        {
            Product[] products = await _catalogSearchService.GetProductsAsync(context.ProductIds.ToArray(), ItemResponseGroup.ItemAssociations);

            //Need to load related products from associated product and categories
            var retVal = products.SelectMany(p => p.Associations.OfType<ProductAssociation>().OrderBy(x => x.Priority))
                                 .Select(a => a.Product).ToList();
            retVal.AddRange(products.SelectMany(p => p.Associations.OfType<CategoryAssociation>().OrderBy(x => x.Priority))
                                .SelectMany(a => a.Category.Products.ToArray()));

            return retVal.Take(context.Take).ToArray();
        } 
        #endregion

    }
}