using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Quote;

namespace VirtoCommerce.Storefront.Converters
{
    public static class QuoteRequestTotalsConverter
    {
        public static QuoteRequestTotals ToWebModel(this QuoteModule.Client.Model.QuoteRequestTotals serviceModel, Currency currency)
        {
            var webModel = new QuoteRequestTotals(currency)
            {
                AdjustmentQuoteExlTax = new Money(serviceModel.AdjustmentQuoteExlTax ?? 0, currency),
                DiscountTotal = new Money(serviceModel.DiscountTotal ?? 0, currency),
                GrandTotalExlTax = new Money(serviceModel.GrandTotalExlTax ?? 0, currency),
                GrandTotalInclTax = new Money(serviceModel.GrandTotalInclTax ?? 0, currency),
                OriginalSubTotalExlTax = new Money(serviceModel.OriginalSubTotalExlTax ?? 0, currency),
                ShippingTotal = new Money(serviceModel.ShippingTotal ?? 0, currency),
                SubTotalExlTax = new Money(serviceModel.SubTotalExlTax ?? 0, currency),
                TaxTotal = new Money(serviceModel.TaxTotal ?? 0, currency)
            };

            return webModel;
        }

        public static QuoteModule.Client.Model.QuoteRequestTotals ToServiceModel(this QuoteRequestTotals webModel)
        {
            var serviceModel = new QuoteModule.Client.Model.QuoteRequestTotals
            {
                AdjustmentQuoteExlTax = (double)webModel.AdjustmentQuoteExlTax.Amount,
                DiscountTotal = (double)webModel.DiscountTotal.Amount,
                GrandTotalExlTax = (double)webModel.GrandTotalExlTax.Amount,
                GrandTotalInclTax = (double)webModel.GrandTotalInclTax.Amount,
                OriginalSubTotalExlTax = (double)webModel.OriginalSubTotalExlTax.Amount,
                ShippingTotal = (double)webModel.ShippingTotal.Amount,
                SubTotalExlTax = (double)webModel.SubTotalExlTax.Amount,
                TaxTotal = (double)webModel.TaxTotal.Amount
            };

            return serviceModel;
        }
    }
}
