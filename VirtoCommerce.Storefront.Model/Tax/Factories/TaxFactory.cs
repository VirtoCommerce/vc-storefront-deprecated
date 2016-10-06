using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.Storefront.Model.Tax.Factories
{
    public class TaxFactory
    {
        public TaxEvaluationContext CreateTaxEvaluationContext(string storeId)
        {
            return new TaxEvaluationContext(storeId);
        }
    }
}
