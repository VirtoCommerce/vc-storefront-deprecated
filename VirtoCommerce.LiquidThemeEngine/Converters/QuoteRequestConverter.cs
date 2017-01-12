using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.LiquidThemeEngine.Objects;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class QuoteRequestConverter
    {
        public static QuoteRequest ToShopifyModel(this Storefront.Model.Quote.QuoteRequest quoteRequest)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidQuoteRequest(quoteRequest);
        }

        public static QuoteItem ToShopifyModel(this Storefront.Model.Quote.QuoteItem quoteItem)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidQuoteItem(quoteItem);
        }

        public static QuoteRequestTotals ToShopifyModel(this Storefront.Model.Quote.QuoteRequestTotals totals)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidRequestTotal(totals);
        }
    }

    public partial class ShopifyModelConverter
    {
        public virtual QuoteRequest ToLiquidQuoteRequest(Storefront.Model.Quote.QuoteRequest quoteRequest)
        {
            var result = new QuoteRequest();

            result.InjectFrom<NullableAndEnumValueInjecter>(quoteRequest);

            result.Addresses = new List<Address>();
            foreach (var address in quoteRequest.Addresses)
            {
                result.Addresses.Add(ToLiquidAddress(address));
            }

            result.Attachments = new List<Attachment>();
            foreach (var attachment in quoteRequest.Attachments)
            {
                result.Attachments.Add(ToLiquidAttachment(attachment));
            }

            if (quoteRequest.Coupon != null)
            {
                result.Coupon = quoteRequest.Coupon.Code;
            }

            result.Currency = ToLiquidCurrency(quoteRequest.Currency);

            result.Items = new List<QuoteItem>();
            foreach (var quoteItem in quoteRequest.Items)
            {
                result.Items.Add(ToLiquidQuoteItem(quoteItem));
            }

            result.Language = ToLiquidLanguage(quoteRequest.Language);
            result.ManualRelDiscountAmount = quoteRequest.ManualRelDiscountAmount.Amount;
            result.ManualShippingTotal = quoteRequest.ManualShippingTotal.Amount;
            result.ManualSubTotal = quoteRequest.ManualSubTotal.Amount;

            if (quoteRequest.ShipmentMethod != null)
            {
                result.ShipmentMethod = ToLiquidShippingMethod(quoteRequest.ShipmentMethod);
            }

            result.TaxDetails = new List<TaxLine>();
            foreach (var taxDetail in quoteRequest.TaxDetails)
            {
                result.TaxDetails.Add(ToLiquidTaxLine(taxDetail));
            }

            if (quoteRequest.Totals != null)
            {
                result.Totals = ToLiquidRequestTotal(quoteRequest.Totals);
            }

            return result;
        }

        public virtual QuoteItem ToLiquidQuoteItem(Storefront.Model.Quote.QuoteItem quoteItem)
        {
            var result = new QuoteItem();

            result.InjectFrom<NullableAndEnumValueInjecter>(quoteItem);

            result.Currency = ToLiquidCurrency(quoteItem.Currency);
            result.ListPrice = quoteItem.ListPrice.Amount * 100;

            result.ProposalPrices = new List<TierPrice>();
            foreach (var proposalPrice in quoteItem.ProposalPrices)
            {
                result.ProposalPrices.Add(ToLiquidTierPrice(proposalPrice));
            }

            result.SalePrice = quoteItem.SalePrice.Amount * 100;

            if (quoteItem.SelectedTierPrice != null)
            {
                result.SelectedTierPrice = ToLiquidTierPrice(quoteItem.SelectedTierPrice);
            }

            return result;
        }

        public virtual QuoteRequestTotals ToLiquidRequestTotal(Storefront.Model.Quote.QuoteRequestTotals requestTotal)
        {
            var result = new QuoteRequestTotals();

            result.AdjustmentQuoteExlTax = requestTotal.AdjustmentQuoteExlTax.Amount * 100;
            result.DiscountTotal = requestTotal.DiscountTotal.Amount * 100;
            result.GrandTotalExlTax = requestTotal.GrandTotalExlTax.Amount * 100;
            result.GrandTotalInclTax = requestTotal.GrandTotalInclTax.Amount * 100;
            result.OriginalSubTotalExlTax = requestTotal.OriginalSubTotalExlTax.Amount * 100;
            result.ShippingTotal = requestTotal.ShippingTotal.Amount * 100;
            result.SubTotalExlTax = requestTotal.SubTotalExlTax.Amount * 100;
            result.TaxTotal = requestTotal.TaxTotal.Amount * 100;

            return result;
        }
    }
}