using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model.Quote.Factories
{
    public class QuoteFactory
    {
        public virtual QuoteRequestTotals CreateTotals(Currency currency)
        {
            return new QuoteRequestTotals(currency);
        }
        public virtual QuoteRequest CreateQuoteRequest(Currency currency, Language language)
        {
            return new QuoteRequest(currency, language);
        }

        public virtual QuoteItem CreateQuoteItem()
        {
            return new QuoteItem();
        }
    }
}
