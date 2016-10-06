using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model.Marketing.Factories
{
    public class MarketingFactory
    {
        public virtual DynamicContentItem CreateDynamicContentItem()
        {
            return new DynamicContentItem();
        }

        public virtual Discount CreateDiscount(Currency currency)
        {
            return new Discount(currency);
        }

        public virtual Promotion CreatePromotion()
        {
            return new Promotion();
        }

        public virtual PromotionReward CreatePromotionReward()
        {
            return new PromotionReward();
        }

        public virtual PromotionEvaluationContext CreatePromotionEvaluationContext()
        {
            return new PromotionEvaluationContext();
        }

        public virtual PromotionProductEntry CreatePromotionProductEntry()
        {
            return new PromotionProductEntry();
        }
    }
}
