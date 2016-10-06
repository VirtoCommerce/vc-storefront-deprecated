using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Quote;
using VirtoCommerce.Storefront.Model.Quote.Factories;
using quoteDTO = VirtoCommerce.Storefront.AutoRestClients.QuoteModuleApi.Models;
using VirtoCommerce.Storefront.Model.Cart.Factories;
using coreDTO = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;
using VirtoCommerce.Storefront.Common;

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

        public static QuoteRequest ToQuoteRequest(this quoteDTO.QuoteRequest quoteRequestDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            return QuoteConverterInstance.ToQuoteRequest(quoteRequestDTO, availCurrencies, language);
        }

        public static quoteDTO.QuoteRequest ToQuoteRequestDTO(this QuoteRequest quoteRequest)
        {
            return QuoteConverterInstance.ToQuoteRequestDTO(quoteRequest);
        }

        public static TaxDetail ToTaxDetail(this quoteDTO.TaxDetail taxDetailDTO, Currency currency)
        {
            return QuoteConverterInstance.ToTaxDetail(taxDetailDTO, currency);
        }

        public static quoteDTO.TaxDetail ToQuoteTaxDetailDTO(this TaxDetail taxDetail)
        {
            return QuoteConverterInstance.ToQuoteTaxDetailDTO(taxDetail);
        }

        public static quoteDTO.QuoteAttachment ToQuoteAttachmentDTO(this Attachment attachment)
        {
            return QuoteConverterInstance.ToQuoteAttachmentDTO(attachment);
        }

        public static TierPrice ToTierPrice(this quoteDTO.TierPrice tierPriceDTO, Currency currency)
        {
            return QuoteConverterInstance.ToTierPrice(tierPriceDTO, currency);
        }

        public static quoteDTO.TierPrice ToQuoteTierPriceDTO(this TierPrice tierPrice)
        {
            return QuoteConverterInstance.ToQuoteTierPriceDTO(tierPrice);
        }

        public static QuoteItem ToQuoteItem(this Product product, long quantity)
        {
            return QuoteConverterInstance.ToQuoteItem(product, quantity);
        }

        public static QuoteItem ToQuoteItem(this quoteDTO.QuoteItem quoteItemDTO, Currency currency)
        {
            return QuoteConverterInstance.ToQuoteItem(quoteItemDTO, currency);
        }

        public static quoteDTO.QuoteItem ToQuoteItemDTO(this QuoteItem quoteItem)
        {
            return QuoteConverterInstance.ToQuoteItemDTO(quoteItem);
        }

        public static QuoteRequestTotals ToQuoteTotals(this quoteDTO.QuoteRequestTotals totalsDTO, Currency currency)
        {
            return QuoteConverterInstance.ToQuoteTotals(totalsDTO, currency);
        }

        public static quoteDTO.QuoteRequestTotals ToQuoteTotalsDTO(this QuoteRequestTotals totals)
        {
            return QuoteConverterInstance.ToQuoteTotalsDTO(totals);
        }

        public static ShippingMethod ToShippingMethod(this quoteDTO.ShipmentMethod shippingMethodDTO, Currency currency)
        {
            return QuoteConverterInstance.ToShippingMethod(shippingMethodDTO, currency);
        }

        public static Address ToAddress(this quoteDTO.Address addressDTO)
        {
            return QuoteConverterInstance.ToAddress(addressDTO);
        }

        public static quoteDTO.Address ToQuoteAddressDTO(this Address address)
        {
            return QuoteConverterInstance.ToQuoteAddressDTO(address);
        }

        public static Attachment ToAttachment(this quoteDTO.QuoteAttachment attachmentDTO)
        {
            return QuoteConverterInstance.ToAttachment(attachmentDTO);
        }

        public static DynamicProperty ToDynamicProperty(this quoteDTO.DynamicObjectProperty propertyDTO)
        {
            return QuoteConverterInstance.ToDynamicProperty(propertyDTO);
        }

        public static quoteDTO.DynamicObjectProperty ToQuoteDynamicPropertyDTO(this DynamicProperty property)
        {
            return QuoteConverterInstance.ToQuoteDynamicPropertyDTO(property);
        }
    }

    public class QuoteConverter
    {
        public virtual DynamicProperty ToDynamicProperty(quoteDTO.DynamicObjectProperty propertyDTO)
        {
            return propertyDTO.JsonConvert<coreDTO.DynamicObjectProperty>().ToDynamicProperty();
        }

        public virtual quoteDTO.DynamicObjectProperty ToQuoteDynamicPropertyDTO(DynamicProperty property)
        {
            return property.ToDynamicPropertyDTO().JsonConvert<quoteDTO.DynamicObjectProperty>();
        }

        public virtual Attachment ToAttachment(quoteDTO.QuoteAttachment attachmentDTO)
        {
            var result = new Attachment();
            result.InjectFrom<NullableAndEnumValueInjecter>(attachmentDTO);
            return result;
        }

        public virtual Address ToAddress(quoteDTO.Address addressDTO)
        {
            return addressDTO.JsonConvert<coreDTO.Address>().ToAddress();
        }

        public virtual quoteDTO.Address ToQuoteAddressDTO(Address address)
        {
            return address.ToCoreAddressDTO().JsonConvert<quoteDTO.Address>();
        }

        public virtual QuoteRequest ToQuoteRequest(quoteDTO.QuoteRequest quoteRequestDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(quoteRequestDTO.Currency)) ?? new Currency(language, quoteRequestDTO.Currency);
            var result = ServiceLocator.Current.GetInstance<QuoteFactory>().CreateQuoteRequest(currency, language);

            result.InjectFrom<NullableAndEnumValueInjecter>(quoteRequestDTO);

            result.Currency = currency;
            result.Language = language;
            result.ManualRelDiscountAmount = new Money(quoteRequestDTO.ManualRelDiscountAmount ?? 0, currency);
            result.ManualShippingTotal = new Money(quoteRequestDTO.ManualShippingTotal ?? 0, currency);
            result.ManualSubTotal = new Money(quoteRequestDTO.ManualSubTotal ?? 0, currency);

            if (quoteRequestDTO.Addresses != null)
            {
                result.Addresses = quoteRequestDTO.Addresses.Select(a => ToAddress(a)).ToList();
            }

            if (quoteRequestDTO.Attachments != null)
            {
                result.Attachments = quoteRequestDTO.Attachments.Select(a => ToAttachment(a)).ToList();
            }

            if (!string.IsNullOrEmpty(quoteRequestDTO.Coupon))
            {
                result.Coupon = new Coupon { AppliedSuccessfully = true, Code = quoteRequestDTO.Coupon };
            }

            if (quoteRequestDTO.DynamicProperties != null)
            {
                result.DynamicProperties = quoteRequestDTO.DynamicProperties.Select(ToDynamicProperty).ToList();
            }

            if (quoteRequestDTO.Items != null)
            {
                result.Items = quoteRequestDTO.Items.Select(i => ToQuoteItem(i, currency)).ToList();
            }

            // TODO
            if (quoteRequestDTO.ShipmentMethod != null)
            {
            }

            if (quoteRequestDTO.TaxDetails != null)
            {
                result.TaxDetails = quoteRequestDTO.TaxDetails.Select(td => ToTaxDetail(td, currency)).ToList();
            }

            if (quoteRequestDTO.Totals != null)
            {
                result.Totals = ToQuoteTotals(quoteRequestDTO.Totals, currency);
            }

            return result;
        }

        public virtual quoteDTO.QuoteRequest ToQuoteRequestDTO(QuoteRequest quoteRequest)
        {
            var serviceModel = new quoteDTO.QuoteRequest();

            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(quoteRequest);

            serviceModel.Currency = quoteRequest.Currency.Code;
            serviceModel.Addresses = quoteRequest.Addresses.Select(ToQuoteAddressDTO).ToList();
            serviceModel.Attachments = quoteRequest.Attachments.Select(ToQuoteAttachmentDTO).ToList();
            serviceModel.DynamicProperties = quoteRequest.DynamicProperties.Select(ToQuoteDynamicPropertyDTO).ToList();
            serviceModel.Items = quoteRequest.Items.Select(ToQuoteItemDTO).ToList();
            serviceModel.LanguageCode = quoteRequest.Language.CultureName;
            serviceModel.ManualRelDiscountAmount = quoteRequest.ManualRelDiscountAmount != null ? (double?)quoteRequest.ManualRelDiscountAmount.Amount : null;
            serviceModel.ManualShippingTotal = quoteRequest.ManualShippingTotal != null ? (double?)quoteRequest.ManualShippingTotal.Amount : null;
            serviceModel.ManualSubTotal = quoteRequest.ManualSubTotal != null ? (double?)quoteRequest.ManualSubTotal.Amount : null;
            serviceModel.TaxDetails = quoteRequest.TaxDetails.Select(ToQuoteTaxDetailDTO).ToList();

            if (quoteRequest.Coupon != null && quoteRequest.Coupon.AppliedSuccessfully)
            {
                serviceModel.Coupon = quoteRequest.Coupon.Code;
            }

            if (quoteRequest.Totals != null)
            {
                serviceModel.Totals = ToQuoteTotalsDTO(quoteRequest.Totals);
            }

            return serviceModel;
        }

        public virtual QuoteItem ToQuoteItem(quoteDTO.QuoteItem quoteItemDTO, Currency currency)
        {
            var result = ServiceLocator.Current.GetInstance<QuoteFactory>().CreateQuoteItem();

            result.InjectFrom<NullableAndEnumValueInjecter>(quoteItemDTO);

            result.Currency = currency;
            result.ListPrice = new Money(quoteItemDTO.ListPrice ?? 0, currency);
            result.SalePrice = new Money(quoteItemDTO.SalePrice ?? 0, currency);

            if (quoteItemDTO.ProposalPrices != null)
            {
                result.ProposalPrices = quoteItemDTO.ProposalPrices.Select(pp => ToTierPrice(pp, currency)).ToList();
            }

            if (quoteItemDTO.SelectedTierPrice != null)
            {
                result.SelectedTierPrice = ToTierPrice( quoteItemDTO.SelectedTierPrice, currency);
            }

            return result;
        }

        public virtual quoteDTO.QuoteItem ToQuoteItemDTO(QuoteItem quoteItem)
        {
            var serviceModel = new quoteDTO.QuoteItem();

            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(quoteItem);

            serviceModel.Currency = quoteItem.Currency.Code;
            serviceModel.ListPrice = (double)quoteItem.ListPrice.Amount;
            serviceModel.ProposalPrices = quoteItem.ProposalPrices.Select(ToQuoteTierPriceDTO).ToList();
            serviceModel.SalePrice = (double)quoteItem.SalePrice.Amount;

            if (quoteItem.SelectedTierPrice != null)
            {
                serviceModel.SelectedTierPrice = ToQuoteTierPriceDTO(quoteItem.SelectedTierPrice);
            }

            return serviceModel;
        }

        public virtual QuoteItem ToQuoteItem(Product product, long quantity)
        {
            var retVal = ServiceLocator.Current.GetInstance<QuoteFactory>().CreateQuoteItem();

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

        public virtual QuoteRequestTotals ToQuoteTotals(quoteDTO.QuoteRequestTotals totalsDTO, Currency currency)
        {
            var result = ServiceLocator.Current.GetInstance<QuoteFactory>().CreateTotals(currency);

            result.AdjustmentQuoteExlTax = new Money(totalsDTO.AdjustmentQuoteExlTax ?? 0, currency);
            result.DiscountTotal = new Money(totalsDTO.DiscountTotal ?? 0, currency);
            result.GrandTotalExlTax = new Money(totalsDTO.GrandTotalExlTax ?? 0, currency);
            result.GrandTotalInclTax = new Money(totalsDTO.GrandTotalInclTax ?? 0, currency);
            result.OriginalSubTotalExlTax = new Money(totalsDTO.OriginalSubTotalExlTax ?? 0, currency);
            result.ShippingTotal = new Money(totalsDTO.ShippingTotal ?? 0, currency);
            result.SubTotalExlTax = new Money(totalsDTO.SubTotalExlTax ?? 0, currency);
            result.TaxTotal = new Money(totalsDTO.TaxTotal ?? 0, currency);
            return result;
        }

        public virtual quoteDTO.QuoteRequestTotals ToQuoteTotalsDTO(QuoteRequestTotals totals)
        {
            var result = new quoteDTO.QuoteRequestTotals
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

        public virtual ShippingMethod ToShippingMethod(quoteDTO.ShipmentMethod shippingMethodDTO, Currency currency)
        {
            var result = ServiceLocator.Current.GetInstance<CartFactory>().CreateShippingMethod(currency);
            result.InjectFrom<NullableAndEnumValueInjecter>(shippingMethodDTO);
            result.Price = new Money(shippingMethodDTO.Price ?? 0, currency);
            return result;
        }

        public virtual TaxDetail ToTaxDetail(quoteDTO.TaxDetail taxDetail, Currency currency)
        {
            var result = new TaxDetail(currency);
            result.InjectFrom(taxDetail);
            return result;
        }

        public virtual quoteDTO.TaxDetail ToQuoteTaxDetailDTO(TaxDetail taxDetail)
        {
            var result = new quoteDTO.TaxDetail();
            result.InjectFrom(taxDetail);
            return result;
        }

        public virtual quoteDTO.QuoteAttachment ToQuoteAttachmentDTO(Attachment attachment)
        {
            var result = new quoteDTO.QuoteAttachment();
            result.InjectFrom<NullableAndEnumValueInjecter>(attachment);
            return result;
        }

        public virtual TierPrice ToTierPrice(quoteDTO.TierPrice tierPriceDTO, Currency currency)
        {
            var result = new TierPrice(currency);
            result.InjectFrom<NullableAndEnumValueInjecter>(tierPriceDTO);
            result.Price = new Money(tierPriceDTO.Price ?? 0, currency);
            return result;
        }

        public virtual quoteDTO.TierPrice ToQuoteTierPriceDTO(TierPrice webModel)
        {
            var serviceModel = new quoteDTO.TierPrice();
            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(webModel);
            serviceModel.Price = (double)webModel.Price.Amount;
            return serviceModel;
        }

    }
}
