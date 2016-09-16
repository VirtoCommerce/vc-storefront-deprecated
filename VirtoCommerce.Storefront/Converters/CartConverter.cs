using System;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;
using cartModel = VirtoCommerce.Storefront.AutoRestClients.CartModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CartConverter
    {
        public static ShoppingCart ToWebModel(this cartModel.ShoppingCart serviceModel, Currency currency, Language language, Model.Customer.CustomerInfo customer)
        {
            var retVal = new ShoppingCart(currency, language);

            retVal.InjectFrom<NullableAndEnumValueInjecter>(serviceModel);

            retVal.Customer = customer;

            if (serviceModel.Coupon != null)
            {
                retVal.Coupon = new Coupon
                {
                    Code = serviceModel.Coupon.Code,
                    AppliedSuccessfully = serviceModel.Coupon.IsValid ?? false
                };            
            }

            if (serviceModel.Items != null)
            {
                retVal.Items = serviceModel.Items.Select(i => i.ToWebModel(currency, language)).ToList();
                retVal.HasPhysicalProducts = retVal.Items.Any(i =>
                    string.IsNullOrEmpty(i.ProductType) ||
                    !string.IsNullOrEmpty(i.ProductType) && i.ProductType.Equals("Physical", StringComparison.OrdinalIgnoreCase));
            }

            if (serviceModel.Addresses != null)
            {
                retVal.Addresses = serviceModel.Addresses.Select(a => a.ToWebModel()).ToList();

                var billingAddress = retVal.Addresses.FirstOrDefault(a => a.Type == AddressType.Billing);
                if (billingAddress == null)
                {
                    billingAddress = new Address { Type = AddressType.Billing };
                }

                if (retVal.HasPhysicalProducts)
                {
                    var shippingAddress = retVal.Addresses.FirstOrDefault(a => a.Type == AddressType.Shipping);
                    if (shippingAddress == null)
                    {
                        shippingAddress = new Address { Type = AddressType.Shipping };
                    }
                }
            }

            if (serviceModel.Payments != null)
            {
                retVal.Payments = serviceModel.Payments.Select(p => p.TowebModel(currency)).ToList();
            }

            if (serviceModel.Shipments != null)
            {
                retVal.Shipments = serviceModel.Shipments.Select(s => s.ToWebModel(retVal)).ToList();
            }

            if (serviceModel.DynamicProperties != null)
            {
                retVal.DynamicProperties = serviceModel.DynamicProperties.Select(dp => dp.ToWebModel()).ToList();
            }

            if (serviceModel.TaxDetails != null)
            {
                retVal.TaxDetails = serviceModel.TaxDetails.Select(td => td.ToWebModel(currency)).ToList();
            }
            retVal.DiscountAmount = new Money(serviceModel.DiscountAmount ?? 0, currency);
            retVal.DiscountAmountWithTax = new Money(serviceModel.DiscountAmountWithTax ?? 0, currency);
            retVal.HandlingTotal = new Money(serviceModel.HandlingTotal ?? 0, currency);
            retVal.HandlingTotalWithTax = new Money(serviceModel.HandlingTotalWithTax ?? 0, currency);
            retVal.IsAnonymous = serviceModel.IsAnonymous == true;
            retVal.IsRecuring = serviceModel.IsRecuring == true;
            retVal.VolumetricWeight = (decimal)(serviceModel.VolumetricWeight ?? 0);
            retVal.Weight = (decimal)(serviceModel.Weight ?? 0);

            return retVal;
        }


        public static cartModel.ShoppingCart ToServiceModel(this ShoppingCart webModel)
        {
            var retVal = new cartModel.ShoppingCart();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(webModel);

            retVal.Addresses = webModel.Addresses.Select(a => a.ToCartServiceModel()).ToList();
            retVal.Coupon = webModel.Coupon != null ? new cartModel.Coupon { Code = webModel.Coupon.Code } : null;
            retVal.Currency = webModel.Currency.Code;
            retVal.Discounts = webModel.Discounts.Select(d => d.ToServiceModel()).ToList();
            retVal.HandlingTotal = (double)webModel.HandlingTotal.Amount;
            retVal.HandlingTotalWithTax = (double)webModel.HandlingTotal.Amount;
            retVal.DiscountAmount = (double)webModel.DiscountAmount.Amount;
            retVal.DiscountAmountWithTax = (double)webModel.DiscountAmountWithTax.Amount;
            retVal.Items = webModel.Items.Select(i => i.ToServiceModel()).ToList();
            retVal.Payments = webModel.Payments.Select(p => p.ToServiceModel()).ToList();
            retVal.Shipments = webModel.Shipments.Select(s => s.ToServiceModel()).ToList();
            retVal.TaxDetails = webModel.TaxDetails.Select(td => td.ToCartApiModel()).ToList();
            retVal.DynamicProperties = webModel.DynamicProperties.Select(dp => dp.ToCartApiModel()).ToList();
            retVal.VolumetricWeight = (double)webModel.VolumetricWeight;
            retVal.Weight = (double)webModel.Weight;

            return retVal;
        }
    }
}
