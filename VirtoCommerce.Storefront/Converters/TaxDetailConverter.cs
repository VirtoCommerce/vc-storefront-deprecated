using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Converters
{
    public static class TaxDetailConverter
    {
        public static TaxDetail ToWebModel(this CartModule.Client.Model.TaxDetail taxDetail, Currency currency)
        {
            var result = new TaxDetail(currency);
            result.InjectFrom(taxDetail);
            return result;
        }

        public static TaxDetail ToWebModel(this OrderModule.Client.Model.TaxDetail taxDetail, Currency currency)
        {
            var result = new TaxDetail(currency);
            result.InjectFrom(taxDetail);
            return result;
        }

        public static TaxDetail ToWebModel(this QuoteModule.Client.Model.TaxDetail taxDetail, Currency currency)
        {
            var result = new TaxDetail(currency);
            result.InjectFrom(taxDetail);
            return result;
        }

        public static CartModule.Client.Model.TaxDetail ToCartApiModel(this TaxDetail taxDetail)
        {
            var result = new CartModule.Client.Model.TaxDetail();
            result.InjectFrom(taxDetail);
            return result;
        }

        public static QuoteModule.Client.Model.TaxDetail ToQuoteApiModel(this TaxDetail taxDetail)
        {
            var result = new QuoteModule.Client.Model.TaxDetail();
            result.InjectFrom(taxDetail);
            return result;
        }
    }
}
