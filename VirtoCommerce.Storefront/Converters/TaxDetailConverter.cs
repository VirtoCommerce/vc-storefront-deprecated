using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using cartModel = VirtoCommerce.Storefront.AutoRestClients.CartModuleApi.Models;
using orderModel = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class TaxDetailConverter
    {
        public static TaxDetail ToWebModel(this cartModel.TaxDetail taxDetail, Currency currency)
        {
            var result = new TaxDetail(currency);
            result.InjectFrom(taxDetail);
            return result;
        }

        public static TaxDetail ToWebModel(this orderModel.TaxDetail taxDetail, Currency currency)
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

        public static cartModel.TaxDetail ToCartApiModel(this TaxDetail taxDetail)
        {
            var result = new cartModel.TaxDetail();
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
