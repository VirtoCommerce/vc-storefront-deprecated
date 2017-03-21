using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model.Catalog;

namespace VirtoCommerce.Storefront.Model.Recommendations
{
    public interface IRecommendationsService
    {
        Task<Product[]> GetRecommendationsAsync(RecommendationsContext context, string type, int skip, int take);
    }
}
