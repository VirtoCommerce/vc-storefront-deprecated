using System.Linq;
using Microsoft.Practices.ServiceLocation;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Recommendations;
using VirtoCommerce.Storefront.Model.Stores;
using dto = VirtoCommerce.Storefront.AutoRestClients.ProductRecommendationsModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class RecommendationConverterExtension
    {
        public static RecommendationConverter RecommendationConverterInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<RecommendationConverter>();
            }
        }

        public static dto.RecommendationEvalContext ToContextDto(this RecommendationEvalContext context)
        {
            return RecommendationConverterInstance.ToContextDto(context);
        }    
    }

    public class RecommendationConverter
    {
        public virtual dto.RecommendationEvalContext ToContextDto(RecommendationEvalContext context)
        {
            var retVal = new dto.RecommendationEvalContext
            {
                ModelId = context.ModelId,
                BuildId = context.BuildId,
                ProductIds = context.ProductIds.Where(x => !string.IsNullOrEmpty(x)).ToList(),
                StoreId = context.StoreId,
                Take = context.Take,
                Type = context.Type,
                UserId = context.UserId
            };
            return retVal;
        }
    }
}
