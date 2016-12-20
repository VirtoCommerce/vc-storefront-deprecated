using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model
{
    public partial class TaxRate
    {
        public TaxRate(Currency currency)
        {
            Rate = new Money(currency);
        }
        public Money Rate { get; set; }
        public TaxLine Line { get; set; }

        public static decimal TaxPercentRound(decimal percent)
        {
            return Math.Round(percent, 3, MidpointRounding.AwayFromZero);
        }
    }
}
