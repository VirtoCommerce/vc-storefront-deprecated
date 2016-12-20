using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;
using VirtoCommerce.Storefront.Model.Quote;
using coreDto = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;
using quoteDto = VirtoCommerce.Storefront.AutoRestClients.QuoteModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class QuoteConverterExtension
    {
        public static QuoteConverter QuoteConverterInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<QuoteConverter>();
            }
        }

        public static QuoteRequest ToQuoteRequest(this quoteDto.QuoteRequest quoteRequestDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            return QuoteConverterInstance.ToQuoteRequest(quoteRequestDto, availCurrencies, language);
        }

        public static quoteDto.QuoteRequest ToQuoteRequestDto(this QuoteRequest quoteRequest)
        {
            return QuoteConverterInstance.ToQuoteRequestDto(quoteRequest);
        }

        public static TaxDetail ToTaxDetail(this quoteDto.TaxDetail taxDetailDto, Currency currency)
        {
            return QuoteConverterInstance.ToTaxDetail(taxDetailDto, currency);
        }

        public static quoteDto.TaxDetail ToQuoteTaxDetailDto(this TaxDetail taxDetail)
        {
            return QuoteConverterInstance.ToQuoteTaxDetailDto(taxDetail);
        }

        public static quoteDto.QuoteAttachment ToQuoteAttachmentDto(this Attachment attachment)
        {
            return QuoteConverterInstance.ToQuoteAttachmentDto(attachment);
        }

        public static TierPrice ToTierPrice(this quoteDto.TierPrice tierPriceDto, Currency currency)
        {
            return QuoteConverterInstance.ToTierPrice(tierPriceDto, currency);
        }

        public static quoteDto.TierPrice ToQuoteTierPriceDto(this TierPrice tierPrice)
        {
            return QuoteConverterInstance.ToQuoteTierPriceDto(tierPrice);
        }

        public static QuoteItem ToQuoteItem(this Product product, long quantity)
        {
            return QuoteConverterInstance.ToQuoteItem(product, quantity);
        }

        public static QuoteItem ToQuoteItem(this quoteDto.QuoteItem quoteItemDto, Currency currency)
        {
            return QuoteConverterInstance.ToQuoteItem(quoteItemDto, currency);
        }

        public static quoteDto.QuoteItem ToQuoteItemDto(this QuoteItem quoteItem)
        {
            return QuoteConverterInstance.ToQuoteItemDto(quoteItem);
        }

        public static QuoteRequestTotals ToQuoteTotals(this quoteDto.QuoteRequestTotals totalsDto, Currency currency)
        {
            return QuoteConverterInstance.ToQuoteTotals(totalsDto, currency);
        }

        public static quoteDto.QuoteRequestTotals ToQuoteTotalsDto(this QuoteRequestTotals totals)
        {
            return QuoteConverterInstance.ToQuoteTotalsDto(totals);
        }

        public static ShippingMethod ToShippingMethod(this quoteDto.ShipmentMethod shippingMethodDto, Currency currency)
        {
            return QuoteConverterInstance.ToShippingMethod(shippingMethodDto, currency);
        }

        public static Address ToAddress(this quoteDto.Address addressDto)
        {
            return QuoteConverterInstance.ToAddress(addressDto);
        }

        public static quoteDto.Address ToQuoteAddressDto(this Address address)
        {
            return QuoteConverterInstance.ToQuoteAddressDto(address);
        }

        public static Attachment ToAttachment(this quoteDto.QuoteAttachment attachmentDto)
        {
            return QuoteConverterInstance.ToAttachment(attachmentDto);
        }

        public static DynamicProperty ToDynamicProperty(this quoteDto.DynamicObjectProperty propertyDto)
        {
            return QuoteConverterInstance.ToDynamicProperty(propertyDto);
        }

        public static quoteDto.DynamicObjectProperty ToQuoteDynamicPropertyDto(this DynamicProperty property)
        {
            return QuoteConverterInstance.ToQuoteDynamicPropertyDto(property);
        }
    }

    public class QuoteConverter
    {
        public virtual DynamicProperty ToDynamicProperty(quoteDto.DynamicObjectProperty propertyDto)
        {
            return propertyDto.JsonConvert<coreDto.DynamicObjectProperty>().ToDynamicProperty();
        }

        public virtual quoteDto.DynamicObjectProperty ToQuoteDynamicPropertyDto(DynamicProperty property)
        {
            return property.ToDynamicPropertyDto().JsonConvert<quoteDto.DynamicObjectProperty>();
        }

        public virtual Attachment ToAttachment(quoteDto.QuoteAttachment attachmentDto)
        {
            var result = new Attachment();
            result.InjectFrom<NullableAndEnumValueInjecter>(attachmentDto);
            return result;
        }

        public virtual Address ToAddress(quoteDto.Address addressDto)
        {
            return addressDto.JsonConvert<coreDto.Address>().ToAddress();
        }

        public virtual quoteDto.Address ToQuoteAddressDto(Address address)
        {
            return address.ToCoreAddressDto().JsonConvert<quoteDto.Address>();
        }

        public virtual QuoteRequest ToQuoteRequest(quoteDto.QuoteRequest quoteRequestDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(quoteRequestDto.Currency)) ?? new Currency(language, quoteRequestDto.Currency);
            var result = new QuoteRequest(currency, language);

            result.InjectFrom<NullableAndEnumValueInjecter>(quoteRequestDto);

            result.Currency = currency;
            result.Language = language;
            result.ManualRelDiscountAmount = new Money(quoteRequestDto.ManualRelDiscountAmount ?? 0, currency);
            result.ManualShippingTotal = new Money(quoteRequestDto.ManualShippingTotal ?? 0, currency);
            result.ManualSubTotal = new Money(quoteRequestDto.ManualSubTotal ?? 0, currency);

            if (quoteRequestDto.Addresses != null)
            {
                result.Addresses = quoteRequestDto.Addresses.Select(a => ToAddress(a)).ToList();
            }

            if (quoteRequestDto.Attachments != null)
            {
                result.Attachments = quoteRequestDto.Attachments.Select(a => ToAttachment(a)).ToList();
            }

            if (!string.IsNullOrEmpty(quoteRequestDto.Coupon))
            {
                result.Coupon = new Coupon { AppliedSuccessfully = true, Code = quoteRequestDto.Coupon };
            }

            if (quoteRequestDto.DynamicProperties != null)
            {
                result.DynamicProperties = quoteRequestDto.DynamicProperties.Select(ToDynamicProperty).ToList();
            }

            if (quoteRequestDto.Items != null)
            {
                result.Items = quoteRequestDto.Items.Select(i => ToQuoteItem(i, currency)).ToList();
            }

            // TODO
            if (quoteRequestDto.ShipmentMethod != null)
            {
            }

            if (quoteRequestDto.TaxDetails != null)
            {
                result.TaxDetails = quoteRequestDto.TaxDetails.Select(td => ToTaxDetail(td, currency)).ToList();
            }

            if (quoteRequestDto.Totals != null)
            {
                result.Totals = ToQuoteTotals(quoteRequestDto.Totals, currency);
            }

            return result;
        }

        public virtual quoteDto.QuoteRequest ToQuoteRequestDto(QuoteRequest quoteRequest)
        {
            var serviceModel = new quoteDto.QuoteRequest();

            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(quoteRequest);

            serviceModel.Currency = quoteRequest.Currency.Code;
            serviceModel.Addresses = quoteRequest.Addresses.Select(ToQuoteAddressDto).ToList();
            serviceModel.Attachments = quoteRequest.Attachments.Select(ToQuoteAttachmentDto).ToList();
            serviceModel.DynamicProperties = quoteRequest.DynamicProperties.Select(ToQuoteDynamicPropertyDto).ToList();
            serviceModel.Items = quoteRequest.Items.Select(ToQuoteItemDto).ToList();
            serviceModel.LanguageCode = quoteRequest.Language.CultureName;
            serviceModel.ManualRelDiscountAmount = quoteRequest.ManualRelDiscountAmount != null ? (double?)quoteRequest.ManualRelDiscountAmount.Amount : null;
            serviceModel.ManualShippingTotal = quoteRequest.ManualShippingTotal != null ? (double?)quoteRequest.ManualShippingTotal.Amount : null;
            serviceModel.ManualSubTotal = quoteRequest.ManualSubTotal != null ? (double?)quoteRequest.ManualSubTotal.Amount : null;
            serviceModel.TaxDetails = quoteRequest.TaxDetails.Select(ToQuoteTaxDetailDto).ToList();

            if (quoteRequest.Coupon != null && quoteRequest.Coupon.AppliedSuccessfully)
            {
                serviceModel.Coupon = quoteRequest.Coupon.Code;
            }

            if (quoteRequest.Totals != null)
            {
                serviceModel.Totals = ToQuoteTotalsDto(quoteRequest.Totals);
            }

            return serviceModel;
        }

        public virtual QuoteItem ToQuoteItem(quoteDto.QuoteItem quoteItemDto, Currency currency)
        {
            var result = new QuoteItem();

            result.InjectFrom<NullableAndEnumValueInjecter>(quoteItemDto);

            result.Currency = currency;
            result.ListPrice = new Money(quoteItemDto.ListPrice ?? 0, currency);
            result.SalePrice = new Money(quoteItemDto.SalePrice ?? 0, currency);

            if (quoteItemDto.ProposalPrices != null)
            {
                result.ProposalPrices = quoteItemDto.ProposalPrices.Select(pp => ToTierPrice(pp, currency)).ToList();
            }

            if (quoteItemDto.SelectedTierPrice != null)
            {
                result.SelectedTierPrice = ToTierPrice(quoteItemDto.SelectedTierPrice, currency);
            }

            return result;
        }

        public virtual quoteDto.QuoteItem ToQuoteItemDto(QuoteItem quoteItem)
        {
            var serviceModel = new quoteDto.QuoteItem();

            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(quoteItem);

            serviceModel.Currency = quoteItem.Currency.Code;
            serviceModel.ListPrice = (double)quoteItem.ListPrice.Amount;
            serviceModel.ProposalPrices = quoteItem.ProposalPrices.Select(ToQuoteTierPriceDto).ToList();
            serviceModel.SalePrice = (double)quoteItem.SalePrice.Amount;

            if (quoteItem.SelectedTierPrice != null)
            {
                serviceModel.SelectedTierPrice = ToQuoteTierPriceDto(quoteItem.SelectedTierPrice);
            }

            return serviceModel;
        }

        public virtual QuoteItem ToQuoteItem(Product product, long quantity)
        {
            var retVal = new QuoteItem();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(product);

            retVal.Id = null;
            retVal.ImageUrl = product.PrimaryImage != null ? product.PrimaryImage.Url : null;
            retVal.ListPrice = product.Price.ListPrice;
            retVal.ProductId = product.Id;
            retVal.SalePrice = product.Price.SalePrice;
            retVal.ProposalPrices.Add(new TierPrice(product.Price.SalePrice, quantity));
            retVal.SelectedTierPrice = retVal.ProposalPrices.First();

            return retVal;
        }

        public virtual QuoteRequestTotals ToQuoteTotals(quoteDto.QuoteRequestTotals totalsDto, Currency currency)
        {
            var result = new QuoteRequestTotals(currency);

            result.AdjustmentQuoteExlTax = new Money(totalsDto.AdjustmentQuoteExlTax ?? 0, currency);
            result.DiscountTotal = new Money(totalsDto.DiscountTotal ?? 0, currency);
            result.GrandTotalExlTax = new Money(totalsDto.GrandTotalExlTax ?? 0, currency);
            result.GrandTotalInclTax = new Money(totalsDto.GrandTotalInclTax ?? 0, currency);
            result.OriginalSubTotalExlTax = new Money(totalsDto.OriginalSubTotalExlTax ?? 0, currency);
            result.ShippingTotal = new Money(totalsDto.ShippingTotal ?? 0, currency);
            result.SubTotalExlTax = new Money(totalsDto.SubTotalExlTax ?? 0, currency);
            result.TaxTotal = new Money(totalsDto.TaxTotal ?? 0, currency);
            return result;
        }

        public virtual quoteDto.QuoteRequestTotals ToQuoteTotalsDto(QuoteRequestTotals totals)
        {
            var result = new quoteDto.QuoteRequestTotals
            {
                AdjustmentQuoteExlTax = (double)totals.AdjustmentQuoteExlTax.Amount,
                DiscountTotal = (double)totals.DiscountTotal.Amount,
                GrandTotalExlTax = (double)totals.GrandTotalExlTax.Amount,
                GrandTotalInclTax = (double)totals.GrandTotalInclTax.Amount,
                OriginalSubTotalExlTax = (double)totals.OriginalSubTotalExlTax.Amount,
                ShippingTotal = (double)totals.ShippingTotal.Amount,
                SubTotalExlTax = (double)totals.SubTotalExlTax.Amount,
                TaxTotal = (double)totals.TaxTotal.Amount
            };

            return result;
        }

        public virtual ShippingMethod ToShippingMethod(quoteDto.ShipmentMethod shippingMethodDto, Currency currency)
        {
            var result = new ShippingMethod(currency);
            result.InjectFrom<NullableAndEnumValueInjecter>(shippingMethodDto);
            result.Price = new Money(shippingMethodDto.Price ?? 0, currency);
            return result;
        }

        public virtual TaxDetail ToTaxDetail(quoteDto.TaxDetail taxDetail, Currency currency)
        {
            var result = new TaxDetail(currency);
            result.InjectFrom(taxDetail);
            return result;
        }

        public virtual quoteDto.TaxDetail ToQuoteTaxDetailDto(TaxDetail taxDetail)
        {
            var result = new quoteDto.TaxDetail();
            result.InjectFrom(taxDetail);
            return result;
        }

        public virtual quoteDto.QuoteAttachment ToQuoteAttachmentDto(Attachment attachment)
        {
            var result = new quoteDto.QuoteAttachment();
            result.InjectFrom<NullableAndEnumValueInjecter>(attachment);
            return result;
        }

        public virtual TierPrice ToTierPrice(quoteDto.TierPrice tierPriceDto, Currency currency)
        {
            var result = new TierPrice(currency);
            result.InjectFrom<NullableAndEnumValueInjecter>(tierPriceDto);
            result.Price = new Money(tierPriceDto.Price ?? 0, currency);
            return result;
        }

        public virtual quoteDto.TierPrice ToQuoteTierPriceDto(TierPrice webModel)
        {
            var serviceModel = new quoteDto.TierPrice();
            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(webModel);
            serviceModel.Price = (double)webModel.Price.Amount;
            return serviceModel;
        }

    }
}
