using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.Storefront.Model.Quote.Factories
{
    public class QuoteFactory
    {
        public virtual QuoteItem CreateQuoteItem()
        {
            return new QuoteItem();
        }
    }
}
