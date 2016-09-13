using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;
using cartModel = VirtoCommerce.Storefront.AutoRestClients.CartModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CartLineItemConverter
    {
        public static LineItem ToLineItem(this Product product, Language language, int quantity)
        {
            var retVal = new LineItem(product.Price.Currency, language);

            retVal.InjectFrom<NullableAndEnumValueInjecter>(product);

            retVal.ImageUrl = product.PrimaryImage != null ? product.PrimaryImage.Url : null;
            retVal.ListPrice = product.Price.ListPrice;
            retVal.ListPriceWithTax = product.Price.ListPriceWithTax;
            retVal.SalePrice = product.Price.GetTierPrice(quantity).Price;
            retVal.SalePriceWithTax = product.Price.GetTierPrice(quantity).PriceWithTax;
            retVal.ProductId = product.Id;
            retVal.Quantity = quantity;

            retVal.ThumbnailImageUrl = product.PrimaryImage != null ? product.PrimaryImage.Url : null;

            return retVal;
        }

        public static LineItem ToWebModel(this cartModel.LineItem serviceModel, Currency currency, Language language)
        {

            var retVal = new LineItem(currency, language);

            retVal.InjectFrom<NullableAndEnumValueInjecter>(serviceModel);

            if (serviceModel.TaxDetails != null)
            {
                retVal.TaxDetails = serviceModel.TaxDetails.Select(td => td.ToWebModel(currency)).ToList();
            }

            if (serviceModel.DynamicProperties != null)
            {
                retVal.DynamicProperties = serviceModel.DynamicProperties.Select(dp => dp.ToWebModel()).ToList();
            }

            if (!serviceModel.Discounts.IsNullOrEmpty())
            {
                retVal.Discounts.AddRange(serviceModel.Discounts.Select(x => x.ToWebModel(new[] { currency }, language)));
            }
            retVal.IsGift = serviceModel.IsGift == true;
            retVal.IsReccuring = serviceModel.IsReccuring == true;
            retVal.ListPrice = new Money(serviceModel.ListPrice ?? 0, currency);
            retVal.ListPriceWithTax = new Money(serviceModel.ListPriceWithTax ?? 0, currency);
            retVal.RequiredShipping = serviceModel.RequiredShipping == true;
            retVal.SalePrice = new Money(serviceModel.SalePrice ?? 0, currency);
            retVal.SalePriceWithTax = new Money(serviceModel.SalePriceWithTax ?? 0, currency);
            retVal.DiscountAmount = new Money(serviceModel.DiscountAmount ?? 0, currency);
            retVal.DiscountAmountWithTax = new Money(serviceModel.DiscountAmountWithTax ?? 0, currency);
            retVal.TaxIncluded = serviceModel.TaxIncluded == true;
            retVal.Weight = (decimal?)serviceModel.Weight;
            retVal.Width = (decimal?)serviceModel.Width;
            retVal.Height = (decimal?)serviceModel.Height;
            retVal.Length = (decimal?)serviceModel.Length;

            return retVal;
        }

        public static cartModel.LineItem ToServiceModel(this LineItem webModel)
        {
            var serviceModel = new cartModel.LineItem();

            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(webModel);

            serviceModel.Currency = webModel.Currency.Code;
            serviceModel.Discounts = webModel.Discounts.Select(d => d.ToServiceModel()).ToList();
         
            serviceModel.ListPrice = (double)webModel.ListPrice.Amount;
            serviceModel.ListPriceWithTax = (double)webModel.ListPriceWithTax.Amount;
            serviceModel.SalePrice = (double)webModel.SalePrice.Amount;
            serviceModel.SalePriceWithTax = (double)webModel.SalePriceWithTax.Amount;
            serviceModel.DiscountAmount = (double)webModel.DiscountAmount.Amount;
            serviceModel.DiscountAmountWithTax = (double)webModel.DiscountAmountWithTax.Amount;
            serviceModel.TaxDetails = webModel.TaxDetails.Select(td => td.ToCartApiModel()).ToList();
            serviceModel.DynamicProperties = webModel.DynamicProperties.Select(dp => dp.ToCartApiModel()).ToList();
            serviceModel.VolumetricWeight = (double)(webModel.VolumetricWeight ?? 0);
            serviceModel.Weight = (double?)webModel.Weight;
            serviceModel.Width = (double?)webModel.Width;
            serviceModel.Height = (double?)webModel.Height;
            serviceModel.Length = (double?)webModel.Length;

            return serviceModel;
        }

        public static CartShipmentItem ToShipmentItem(this LineItem lineItem)
        {
            var shipmentItem = new CartShipmentItem
            {
                LineItem = lineItem,
                Quantity = lineItem.Quantity
            };
            return shipmentItem;
        }

        public static PromotionProductEntry ToPromotionItem(this LineItem lineItem)
        {
            var promoItem = new PromotionProductEntry();

            promoItem.InjectFrom(lineItem);

            promoItem.Discount = new Money(lineItem.DiscountTotal.Amount, lineItem.DiscountTotal.Currency);
            promoItem.Price = new Money(lineItem.PlacedPrice.Amount, lineItem.PlacedPrice.Currency);
            promoItem.Quantity = lineItem.Quantity;
            promoItem.Variations = null; // TODO

            return promoItem;
        }

    }
}
