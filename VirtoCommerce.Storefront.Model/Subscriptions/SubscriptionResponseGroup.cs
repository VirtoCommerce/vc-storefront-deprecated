using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.Storefront.Model.Subscriptions
{
    [Flags]
    public enum SubscriptionResponseGroup
    {
        Default = 0,
        WithOrderPrototype = 1,
        WithRlatedOrders = 1 << 1,
        Full = Default | WithOrderPrototype | WithRlatedOrders
    }
}
