using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.Storefront.Model.Marketing.Factories
{
    public class MarketingFactory
    {
        public virtual PromotionProductEntry CreatePromotionProductEntry()
        {
            return new PromotionProductEntry();
        }
    }
}
