using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.MarketingModuleApi;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model.Marketing;
using VirtoCommerce.Storefront.Model.Marketing.Services;
using marketingModel = VirtoCommerce.Storefront.AutoRestClients.MarketingModuleApi.Models;

namespace VirtoCommerce.Storefront.Services
{
    public class PromotionEvaluator : IPromotionEvaluator
    {
        private readonly IMarketingModuleApiClient _marketingApi;
        private readonly PromotionEvaluationContextConverter _promotionEvaluationContextConverter;

        public PromotionEvaluator(IMarketingModuleApiClient marketingApi, PromotionEvaluationContextConverter promotionEvaluationContextConverter)
        {
            _marketingApi = marketingApi;
            _promotionEvaluationContextConverter = promotionEvaluationContextConverter;
        }

        #region IPromotionEvaluator Members

        public virtual async Task EvaluateDiscountsAsync(PromotionEvaluationContext context, IEnumerable<IDiscountable> owners)
        {
            var rewards = await _marketingApi.MarketingModulePromotion.EvaluatePromotionsAsync(_promotionEvaluationContextConverter.ToServiceModel(context));
            InnerEvaluateDiscounts(rewards, owners);
        }

        public virtual void EvaluateDiscounts(PromotionEvaluationContext context, IEnumerable<IDiscountable> owners)
        {
            var rewards = _marketingApi.MarketingModulePromotion.EvaluatePromotions(_promotionEvaluationContextConverter.ToServiceModel(context));
            InnerEvaluateDiscounts(rewards, owners);
        }

        #endregion

        protected virtual void InnerEvaluateDiscounts(IList<marketingModel.PromotionReward> rewards, IEnumerable<IDiscountable> owners)
        {
            if (rewards != null)
            {
                foreach (var owner in owners)
                {
                    owner.ApplyRewards(rewards.Select(r => r.ToWebModel(owner.Currency)));
                }
            }
        }
    }
}
