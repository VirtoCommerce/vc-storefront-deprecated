using System.Linq;
using Microsoft.Practices.ServiceLocation;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Recommendations;
using VirtoCommerce.Storefront.Model.Stores;
using dto = VirtoCommerce.Storefront.AutoRestClients.ProductRecommendationsModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CognitiveRecommendationConverterExtension
    {
        public static CognitiveRecommendationConverter RecommendationConverterInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CognitiveRecommendationConverter>();
            }
        }

        public static dto.RecommendationEvalContext ToContextDto(this CognitiveRecommendationEvalContext context)
        {
            return RecommendationConverterInstance.ToContextDto(context);
        }    
    }

    public class CognitiveRecommendationConverter
    {
        public virtual dto.RecommendationEvalContext ToContextDto(CognitiveRecommendationEvalContext context)
        {
            var retVal = new dto.RecommendationEvalContext
            {
                BuildId = context.BuildId,
                ModelId = context.ModelId,
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
