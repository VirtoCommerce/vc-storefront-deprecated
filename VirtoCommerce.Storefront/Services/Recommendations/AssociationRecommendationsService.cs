using System;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Recommendations;
using VirtoCommerce.Storefront.Model.Services;

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

        public Task<Product[]> GetRecommendationsAsync(RecommendationsContext context, string type, int skip, int take)
        {
            throw new NotImplementedException();
        }
    }
}