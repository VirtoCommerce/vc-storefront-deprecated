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

        public PromotionEvaluator(IMarketingModuleApiClient marketingApi)
        {
            _marketingApi = marketingApi;
        }

        #region IPromotionEvaluator Members
        public async Task EvaluateDiscountsAsync(PromotionEvaluationContext context, IEnumerable<IDiscountable> owners)
        {
            var rewards = await _marketingApi.MarketingModulePromotion.EvaluatePromotionsAsync(context.ToServiceModel());
            InnerEvaluateDiscounts(rewards, owners);
        }

        public void EvaluateDiscounts(PromotionEvaluationContext context, IEnumerable<IDiscountable> owners)
        {
            var rewards = _marketingApi.MarketingModulePromotion.EvaluatePromotions(context.ToServiceModel());
            InnerEvaluateDiscounts(rewards, owners);
        }

        #endregion

        private static void InnerEvaluateDiscounts(IList<marketingModel.PromotionReward> rewards, IEnumerable<IDiscountable> owners)
        {
            if (rewards == null)
            {
                return;
            }

            foreach (var owner in owners)
            {
                owner.ApplyRewards(rewards.Select(r => r.ToWebModel(owner.Currency)));
            }
        }
    }
}
