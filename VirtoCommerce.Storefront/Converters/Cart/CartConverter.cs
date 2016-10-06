using System;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Cart.Factories;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;
using VirtoCommerce.Storefront.Model.Marketing.Factories;
using VirtoCommerce.Storefront.Model.Tax;
using VirtoCommerce.Storefront.Model.Tax.Factories;
using VirtoCommerce.Storefront.Model.Catalog;
using cartDTO = VirtoCommerce.Storefront.AutoRestClients.CartModuleApi.Models;
using coreDTO = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;
using platformDTO = VirtoCommerce.Storefront.AutoRestClients.PlatformModuleApi.Models;
using VirtoCommerce.Storefront.Common;
using System.Collections.Generic;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CartConverterExtension
    {
        public static CartConverter CartConverterInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CartConverter>();
            }
        }

        public static ShoppingCart ToShoppingCart(this cartDTO.ShoppingCart cartDTO, Currency currency, Language language, Model.Customer.CustomerInfo customer)
        {
            return CartConverterInstance.ToShoppingCart(cartDTO, currency, language, customer);
        }

        public static cartDTO.ShoppingCart ToShoppingCartDTO(this ShoppingCart cart)
        {            
            return CartConverterInstance.ToShoppingCartDTO(cart);
        }

        public static PromotionEvaluationContext ToPromotionEvaluationContext(this ShoppingCart cart)
        {
           return CartConverterInstance.ToPromotionEvaluationContext(cart);
        }

        public static TaxEvaluationContext ToTaxEvalContext(this ShoppingCart cart)
        {
            return CartConverterInstance.ToTaxEvalContext(cart);
        }

        public static TaxDetail ToTaxDetail(this cartDTO.TaxDetail taxDetail, Currency currency)
        {
            return CartConverterInstance.ToTaxDetail(taxDetail, currency);
        }

        public static cartDTO.TaxDetail ToCartTaxDetailDTO(this TaxDetail taxDetail)
        {
            return CartConverterInstance.ToCartTaxDetailDTO(taxDetail);
        }

        public static LineItem ToLineItem(this Product product, Language language, int quantity)
        {
             return CartConverterInstance.ToLineItem(product, language, quantity);
        }

        public static LineItem ToLineItem(this cartDTO.LineItem lineItemDTO, Currency currency, Language language)
        {
            return CartConverterInstance.ToLineItem(lineItemDTO, currency, language);
        }

        public static cartDTO.LineItem ToLineItemDTO(this LineItem lineItem)
        {
            return CartConverterInstance.ToLineItemDTO(lineItem);
        }

        public static CartShipmentItem ToShipmentItem(this LineItem lineItem)
        {
             return CartConverterInstance.ToShipmentItem(lineItem);
        }

        public static PromotionProductEntry ToPromotionItem(this LineItem lineItem)
        {
            return CartConverterInstance.ToPromotionItem(lineItem);
        }

        public static cartDTO.Address ToCartAddressDTO(this Address address)
        {
            return CartConverterInstance.ToCartAddressDTO(address);
        }

        public static Address ToAddress(this cartDTO.Address addressDTO)
        {
            return CartConverterInstance.ToAddress(addressDTO);
        }

        public static Payment ToPayment(this cartDTO.Payment paymentDTO, Currency currency)
        {
            return CartConverterInstance.ToPayment(paymentDTO, currency);
        }

        public static cartDTO.Payment ToPaymentDTO(this Payment payment)
        {
            return CartConverterInstance.ToPaymentDTO(payment);
        }

        public static PaymentMethod ToPaymentMethod(this cartDTO.PaymentMethod paymentMethodDTO)
        {
            return CartConverterInstance.ToPaymentMethod(paymentMethodDTO);
        }

        public static Payment ToCartPayment(this PaymentMethod paymentMethod, Money amount, Currency currency)
        {
            return CartConverterInstance.ToCartPayment(paymentMethod, amount, currency);
        }

        public static Shipment ToShipment(this cartDTO.Shipment shipmentDTO, ShoppingCart cart)
        {
            return CartConverterInstance.ToShipment(shipmentDTO, cart);
        }

        public static cartDTO.Shipment ToShipmentDTO(this Shipment shipment)
        {
            return CartConverterInstance.ToShipmentDTO(shipment);
        }

        public static CartShipmentItem ToShipmentItem(this cartDTO.ShipmentItem shipmentItemDTO, ShoppingCart cart)
        {
            return CartConverterInstance.ToShipmentItem(shipmentItemDTO, cart);
        }

        public static cartDTO.ShipmentItem ToShipmentItemDTO(this CartShipmentItem shipmentItem)
        {
            return CartConverterInstance.ToShipmentItemDTO(shipmentItem);
        }

        public static ShippingMethod ToShippingMethod(this cartDTO.ShippingRate shippingRate, Currency currency, IEnumerable<Currency> availCurrencies)
        {
            return CartConverterInstance.ToShippingMethod(shippingRate, currency, availCurrencies);

        }
        public static Shipment ToCartShipment(this ShippingMethod shippingMethod, Currency currency)
        {
            return CartConverterInstance.ToCartShipment(shippingMethod, currency);
        }

        public static TaxLine[] ToTaxLines(this ShippingMethod shipmentMethod)
        {
            return CartConverterInstance.ToTaxLines(shipmentMethod);
        }

        public static Discount ToDiscount(this cartDTO.Discount discountDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            return CartConverterInstance.ToDiscount(discountDTO, availCurrencies, language);
        }

        public static cartDTO.Discount ToCartDiscountDTO(this Discount discount)
        {
            return CartConverterInstance.ToCartDiscountDTO(discount);
        }

        public static DynamicProperty ToDynamicProperty(this cartDTO.DynamicObjectProperty propertyDTO)
        {
            return CartConverterInstance.ToDynamicProperty(propertyDTO);
        }

        public static cartDTO.DynamicObjectProperty ToCartDynamicPropertyDTO(this DynamicProperty property)
        {
            return CartConverterInstance.ToCartDynamicPropertyDTO(property);
        }
    }

    public class CartConverter
    {
        public virtual DynamicProperty ToDynamicProperty(cartDTO.DynamicObjectProperty propertyDTO)
        {
            return propertyDTO.JsonConvert<coreDTO.DynamicObjectProperty>().ToDynamicProperty();
        }

        public virtual cartDTO.DynamicObjectProperty ToCartDynamicPropertyDTO(DynamicProperty property)
        {
            return property.ToDynamicPropertyDTO().JsonConvert<cartDTO.DynamicObjectProperty>();
        }

        public virtual Discount ToDiscount(cartDTO.Discount discountDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(discountDTO.Currency)) ?? new Currency(language, discountDTO.Currency);

            var result = ServiceLocator.Current.GetInstance<MarketingFactory>().CreateDiscount(currency);

            result.InjectFrom<NullableAndEnumValueInjecter>(discountDTO);
            result.Amount = new Money(discountDTO.DiscountAmount ?? 0, currency);

            return result;
        }

        public virtual cartDTO.Discount ToCartDiscountDTO(Discount webModel)
        {
            var result = new cartDTO.Discount();

            result.InjectFrom<NullableAndEnumValueInjecter>(webModel);

            result.Currency = webModel.Amount.Currency.Code;
            result.DiscountAmount = (double)webModel.Amount.Amount;

            return result;
        }

        public virtual ShippingMethod ToShippingMethod(cartDTO.ShippingRate shippingRate, Currency currency, IEnumerable<Currency> availCurrencies)
        {
            var rateCurrency = availCurrencies.FirstOrDefault(x => x.Equals(shippingRate.Currency)) ?? new Currency(new Language(currency.CultureName), shippingRate.Currency);
            var ratePrice = new Money(shippingRate.Rate ?? 0, rateCurrency);
            var ratePriceWithTax = new Money(shippingRate.RateWithTax ?? 0, rateCurrency);
            var rateDiscount = new Money(shippingRate.DiscountAmount ?? 0, rateCurrency);
            var rateDiscountWithTax = new Money(shippingRate.DiscountAmountWithTax ?? 0, rateCurrency);
            if (rateCurrency != currency)
            {
                ratePrice = ratePrice.ConvertTo(currency);
                ratePriceWithTax = ratePriceWithTax.ConvertTo(currency);
                rateDiscount = rateDiscount.ConvertTo(currency);
                rateDiscountWithTax = rateDiscountWithTax.ConvertTo(currency);
            }

            var result = ServiceLocator.Current.GetInstance<CartFactory>().CreateShippingMethod(currency);
            result.InjectFrom<NullableAndEnumValueInjecter>(shippingRate);

            result.Price = ratePrice;
            result.DiscountAmount = rateDiscount;

            if (shippingRate.ShippingMethod != null)
            {
                result.InjectFrom<NullableAndEnumValueInjecter>(shippingRate.ShippingMethod);
                result.ShipmentMethodCode = shippingRate.ShippingMethod.Code;
                if (shippingRate.ShippingMethod.Settings != null)
                {
                    result.Settings = shippingRate.ShippingMethod.Settings.Select(x => x.JsonConvert<platformDTO.Setting>().ToSettingEntry()).ToList();
                }
            }
            return result;
        }

        public virtual Shipment ToCartShipment(ShippingMethod shippingMethod, Currency currency)
        {
            var result = ServiceLocator.Current.GetInstance<CartFactory>().CreateShipment(currency);
            result.ShipmentMethodCode = shippingMethod.ShipmentMethodCode;
            result.Price = shippingMethod.Price;
            result.TaxType = shippingMethod.TaxType;
            return result;
        }

        public virtual TaxLine[] ToTaxLines(ShippingMethod shipmentMethod)
        {
            var retVal = new List<TaxLine>
            {
                new TaxLine(shipmentMethod.Currency)
                {
                    Id = string.Join("&", shipmentMethod.ShipmentMethodCode, shipmentMethod.OptionName),
                    Code = shipmentMethod.ShipmentMethodCode,
                    TaxType = shipmentMethod.TaxType,
                    Amount = shipmentMethod.Total
                }
            };
            return retVal.ToArray();
        }

        public virtual CartShipmentItem ToShipmentItem(cartDTO.ShipmentItem shipmentItemDTO, ShoppingCart cart)
        {
            var result = ServiceLocator.Current.GetInstance<CartFactory>().CreateShipmentItem();

            result.InjectFrom<NullableAndEnumValueInjecter>(shipmentItemDTO);

            result.LineItem = cart.Items.FirstOrDefault(x => x.Id == shipmentItemDTO.LineItemId);

            return result;
        }

        public virtual cartDTO.ShipmentItem ToShipmentItemDTO(CartShipmentItem webModel)
        {
            var result = new cartDTO.ShipmentItem();

            result.InjectFrom<NullableAndEnumValueInjecter>(webModel);

            result.LineItem = webModel.LineItem.ToLineItemDTO();

            return result;
        }

        public virtual Shipment ToShipment(cartDTO.Shipment shipmentDTO, ShoppingCart cart)
        {
            var retVal = ServiceLocator.Current.GetInstance<CartFactory>().CreateShipment(cart.Currency);

            retVal.InjectFrom(shipmentDTO);
            retVal.Currency = cart.Currency;
            retVal.Price = new Money(shipmentDTO.Price ?? 0, cart.Currency);
            retVal.DiscountAmount = new Money(shipmentDTO.DiscountAmount ?? 0, cart.Currency);
            retVal.TaxPercentRate = (decimal?)shipmentDTO.TaxPercentRate ?? 0m;

            if (shipmentDTO.DeliveryAddress != null)
            {
                retVal.DeliveryAddress = ToAddress(shipmentDTO.DeliveryAddress);
            }

            if (shipmentDTO.Items != null)
            {
                retVal.Items = shipmentDTO.Items.Select(i => ToShipmentItem(i, cart)).ToList();
            }

            if (shipmentDTO.TaxDetails != null)
            {
                retVal.TaxDetails = shipmentDTO.TaxDetails.Select(td => ToTaxDetail(td, cart.Currency)).ToList();
            }

            if (!shipmentDTO.Discounts.IsNullOrEmpty())
            {
                retVal.Discounts.AddRange(shipmentDTO.Discounts.Select(x => ToDiscount(x, new[] { cart.Currency }, cart.Language)));
            }
            return retVal;
        }

        public virtual cartDTO.Shipment ToShipmentDTO(Shipment shipment)
        {
            var retVal = new cartDTO.Shipment();

            retVal.InjectFrom(shipment);
            retVal.Currency = shipment.Currency != null ? shipment.Currency.Code : null;
            retVal.DiscountAmount = shipment.DiscountAmount != null ? (double?)shipment.DiscountAmount.Amount : null;
            retVal.Price = shipment.Price != null ? (double?)shipment.Price.Amount : null;
            retVal.TaxPercentRate = (double)shipment.TaxPercentRate;

            if (shipment.DeliveryAddress != null)
            {
                retVal.DeliveryAddress = ToCartAddressDTO(shipment.DeliveryAddress);
            }

            if (shipment.Discounts != null)
            {
                retVal.Discounts = shipment.Discounts.Select(ToCartDiscountDTO).ToList();
            }

            if (shipment.Items != null)
            {
                retVal.Items = shipment.Items.Select(ToShipmentItemDTO).ToList();
            }

            if (shipment.TaxDetails != null)
            {
                retVal.TaxDetails = shipment.TaxDetails.Select(ToCartTaxDetailDTO).ToList();
            }

            return retVal;
        }

        public virtual PaymentMethod ToPaymentMethod(cartDTO.PaymentMethod paymentMethodDTO)
        {
            var retVal = ServiceLocator.Current.GetInstance<CartFactory>().CreatePaymentMethod();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(paymentMethodDTO);
            retVal.Priority = paymentMethodDTO.Priority ?? 0;

            if (paymentMethodDTO.Settings != null)
            {
                retVal.Settings = paymentMethodDTO.Settings.Select(x => x.JsonConvert<platformDTO.Setting>().ToSettingEntry()).ToList();
            }

            return retVal;
        }

        public virtual Payment ToCartPayment(PaymentMethod paymentMethod, Money amount, Currency currency)
        {
            var paymentWebModel = new Payment(currency)
            {
                Amount = amount,
                Currency = currency,
                PaymentGatewayCode = paymentMethod.Code
            };
            return paymentWebModel;
        }

        public virtual Payment ToPayment(cartDTO.Payment paymentDTO, Currency currency)
        {
            var result = ServiceLocator.Current.GetInstance<CartFactory>().CreatePayment(currency);

            result.InjectFrom<NullableAndEnumValueInjecter>(paymentDTO);

            result.Amount = new Money(paymentDTO.Amount ?? 0, currency);

            if (paymentDTO.BillingAddress != null)
            {
                result.BillingAddress = ToAddress(paymentDTO.BillingAddress);
            }

            result.Currency = currency;

            return result;
        }

        public virtual cartDTO.Payment ToPaymentDTO(Payment payment)
        {
            var result = new cartDTO.Payment();

            result.InjectFrom<NullableAndEnumValueInjecter>(payment);

            result.Amount = (double)payment.Amount.Amount;

            if (payment.BillingAddress != null)
            {
                result.BillingAddress = ToCartAddressDTO(payment.BillingAddress);
            }

            result.Currency = payment.Currency.Code;

            return result;
        }

        public virtual cartDTO.Address ToCartAddressDTO(Address address)
        {
            return address.ToCoreAddressDTO().JsonConvert<cartDTO.Address>();
        }

        public virtual Address ToAddress(cartDTO.Address addressDTO)
        {
            return addressDTO.JsonConvert<coreDTO.Address>().ToAddress();
        }

        public virtual PromotionEvaluationContext ToPromotionEvaluationContext(ShoppingCart cart)
        {
            var promotionItems = cart.Items.Select(ToPromotionItem).ToList();

            var result = ServiceLocator.Current.GetInstance<MarketingFactory>().CreatePromotionEvaluationContext();
            result.CartPromoEntries = promotionItems;
            result.CartTotal = cart.Total;
            result.Coupon = cart.Coupon != null ? cart.Coupon.Code : null;
            result.Currency = cart.Currency;
            result.CustomerId = cart.Customer.Id;
            result.IsRegisteredUser = cart.Customer.IsRegisteredUser;
            result.Language = cart.Language;
            result.PromoEntries = promotionItems;
            result.StoreId = cart.StoreId;

            return result;
        }

        public virtual ShoppingCart ToShoppingCart(cartDTO.ShoppingCart cartDTO, Currency currency, Language language, Model.Customer.CustomerInfo customer)
        {
            var result = ServiceLocator.Current.GetInstance<CartFactory>().CreateCart(currency, language);

            result.InjectFrom<NullableAndEnumValueInjecter>(cartDTO);

            result.Customer = customer;

            if (cartDTO.Coupon != null)
            {
                result.Coupon = new Coupon
                {
                    Code = cartDTO.Coupon.Code,
                    AppliedSuccessfully = cartDTO.Coupon.IsValid ?? false
                };            
            }

            if (cartDTO.Items != null)
            {
                result.Items = cartDTO.Items.Select(i => ToLineItem(i, currency, language)).ToList();
                result.HasPhysicalProducts = result.Items.Any(i =>
                    string.IsNullOrEmpty(i.ProductType) ||
                    !string.IsNullOrEmpty(i.ProductType) && i.ProductType.Equals("Physical", StringComparison.OrdinalIgnoreCase));
            }

            if (cartDTO.Addresses != null)
            {
                result.Addresses = cartDTO.Addresses.Select(ToAddress).ToList();

                var billingAddress = result.Addresses.FirstOrDefault(a => a.Type == AddressType.Billing);
                if (billingAddress == null)
                {
                    billingAddress = new Address { Type = AddressType.Billing };
                }

                if (result.HasPhysicalProducts)
                {
                    var shippingAddress = result.Addresses.FirstOrDefault(a => a.Type == AddressType.Shipping);
                    if (shippingAddress == null)
                    {
                        shippingAddress = new Address { Type = AddressType.Shipping };
                    }
                }
            }

            if (cartDTO.Payments != null)
            {
                result.Payments = cartDTO.Payments.Select(p => ToPayment(p, currency)).ToList();
            }

            if (cartDTO.Shipments != null)
            {
                result.Shipments = cartDTO.Shipments.Select(s => ToShipment(s, result)).ToList();
            }

            if (cartDTO.DynamicProperties != null)
            {
                result.DynamicProperties = cartDTO.DynamicProperties.Select(ToDynamicProperty).ToList();
            }

            if (cartDTO.TaxDetails != null)
            {
                result.TaxDetails = cartDTO.TaxDetails.Select(td => ToTaxDetail(td, currency)).ToList();
            }
            result.DiscountAmount = new Money(cartDTO.DiscountAmount ?? 0, currency);
            result.HandlingTotal = new Money(cartDTO.HandlingTotal ?? 0, currency);
            result.HandlingTotalWithTax = new Money(cartDTO.HandlingTotalWithTax ?? 0, currency);
            result.IsAnonymous = cartDTO.IsAnonymous == true;
            result.IsRecuring = cartDTO.IsRecuring == true;
            result.VolumetricWeight = (decimal)(cartDTO.VolumetricWeight ?? 0);
            result.Weight = (decimal)(cartDTO.Weight ?? 0);

            return result;
        }


        public virtual cartDTO.ShoppingCart ToShoppingCartDTO(ShoppingCart cart)
        {
            var result = new cartDTO.ShoppingCart();

            result.InjectFrom<NullableAndEnumValueInjecter>(cart);

            result.Addresses = cart.Addresses.Select(ToCartAddressDTO).ToList();
            result.Coupon = cart.Coupon != null ? new cartDTO.Coupon { Code = cart.Coupon.Code, IsValid = cart.Coupon.AppliedSuccessfully } : null;
            result.Currency = cart.Currency.Code;
            result.Discounts = cart.Discounts.Select(ToCartDiscountDTO).ToList();
            result.HandlingTotal = (double)cart.HandlingTotal.Amount;
            result.HandlingTotalWithTax = (double)cart.HandlingTotal.Amount;
            result.DiscountAmount = (double)cart.DiscountAmount.Amount;
            result.Items = cart.Items.Select(ToLineItemDTO).ToList();
            result.Payments = cart.Payments.Select(ToPaymentDTO).ToList();
            result.Shipments = cart.Shipments.Select(ToShipmentDTO).ToList();
            result.TaxDetails = cart.TaxDetails.Select(ToCartTaxDetailDTO).ToList();
            result.DynamicProperties = cart.DynamicProperties.Select(ToCartDynamicPropertyDTO).ToList();
            result.VolumetricWeight = (double)cart.VolumetricWeight;
            result.Weight = (double)cart.Weight;

            return result;
        }

        public virtual TaxEvaluationContext ToTaxEvalContext(ShoppingCart cart)
        {
            var result = ServiceLocator.Current.GetInstance<TaxFactory>().CreateTaxEvaluationContext(cart.StoreId);

            result.Id = cart.Id;
            result.Code = cart.Name;
            result.Currency = cart.Currency;
            result.Type = "Cart";
           
            foreach (var lineItem in cart.Items)
            {
                result.Lines.Add(new TaxLine(lineItem.Currency)
                {
                    Id = lineItem.Id,
                    Code = lineItem.Sku,
                    Name = lineItem.Name,
                    TaxType = lineItem.TaxType,
                    Amount = lineItem.ExtendedPrice
                });
            }

            foreach (var shipment in cart.Shipments)
            {
                var totalTaxLine = new TaxLine(shipment.Currency)
                {
                    Id = shipment.Id,
                    Code = shipment.ShipmentMethodCode,
                    Name = shipment.ShipmentMethodOption,
                    TaxType = shipment.TaxType,
                    Amount = shipment.Total
                };
                result.Lines.Add(totalTaxLine);

                if (shipment.DeliveryAddress != null)
                {
                    result.Address = shipment.DeliveryAddress;
                }
                result.Customer = cart.Customer;
            }

            return result;
        }

        public virtual TaxDetail ToTaxDetail(cartDTO.TaxDetail taxDeatilDTO, Currency currency)
        {
            var result = new TaxDetail(currency);
            result.InjectFrom(taxDeatilDTO);
            return result;
        }

        public virtual cartDTO.TaxDetail ToCartTaxDetailDTO(TaxDetail taxDetail)
        {
            var result = new cartDTO.TaxDetail();
            result.InjectFrom(taxDetail);
            return result;
        }

        public virtual LineItem ToLineItem(Product product, Language language, int quantity)
        {
            var result = ServiceLocator.Current.GetInstance<CartFactory>().CreateLineItem(product.Price.Currency, language);

            result.InjectFrom<NullableAndEnumValueInjecter>(product);
            result.Id = null;
            result.ImageUrl = product.PrimaryImage != null ? product.PrimaryImage.Url : null;
            result.ListPrice = product.Price.ListPrice;
            result.SalePrice = product.Price.GetTierPrice(quantity).Price;
            result.TaxPercentRate = product.Price.TaxPercentRate;
            result.DiscountAmount = product.Price.DiscountAmount;
            result.ProductId = product.Id;
            result.Quantity = quantity;
            result.ThumbnailImageUrl = product.PrimaryImage != null ? product.PrimaryImage.Url : null;

            return result;
        }

        public virtual LineItem ToLineItem(cartDTO.LineItem lineItemDTO, Currency currency, Language language)
        {
            var result = ServiceLocator.Current.GetInstance<CartFactory>().CreateLineItem(currency, language);

            result.InjectFrom<NullableAndEnumValueInjecter>(lineItemDTO);

            if (lineItemDTO.TaxDetails != null)
            {
                result.TaxDetails = lineItemDTO.TaxDetails.Select(td => ToTaxDetail(td, currency)).ToList();
            }

            if (lineItemDTO.DynamicProperties != null)
            {
                result.DynamicProperties = lineItemDTO.DynamicProperties.Select(ToDynamicProperty).ToList();
            }

            if (!lineItemDTO.Discounts.IsNullOrEmpty())
            {
                result.Discounts.AddRange(lineItemDTO.Discounts.Select(x => ToDiscount(x, new[] { currency }, language)));
            }
            result.Comment = lineItemDTO.Note;
            result.IsGift = lineItemDTO.IsGift == true;
            result.IsReccuring = lineItemDTO.IsReccuring == true;
            result.ListPrice = new Money(lineItemDTO.ListPrice ?? 0, currency);
            result.RequiredShipping = lineItemDTO.RequiredShipping == true;
            result.SalePrice = new Money(lineItemDTO.SalePrice ?? 0, currency);
            result.TaxPercentRate = (decimal?)lineItemDTO.TaxPercentRate ?? 0m;
            result.DiscountAmount = new Money(lineItemDTO.DiscountAmount ?? 0, currency);
            result.TaxIncluded = lineItemDTO.TaxIncluded == true;
            result.Weight = (decimal?)lineItemDTO.Weight;
            result.Width = (decimal?)lineItemDTO.Width;
            result.Height = (decimal?)lineItemDTO.Height;
            result.Length = (decimal?)lineItemDTO.Length;

            return result;
        }

        public virtual cartDTO.LineItem ToLineItemDTO(LineItem lineItem)
        {
            var retVal = new cartDTO.LineItem();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(lineItem);

            retVal.Currency = lineItem.Currency.Code;
            retVal.Discounts = lineItem.Discounts.Select(ToCartDiscountDTO).ToList();

            retVal.ListPrice = (double)lineItem.ListPrice.Amount;
            retVal.SalePrice = (double)lineItem.SalePrice.Amount;
            retVal.TaxPercentRate = (double)lineItem.TaxPercentRate;
            retVal.DiscountAmount = (double)lineItem.DiscountAmount.Amount;
            retVal.TaxDetails = lineItem.TaxDetails.Select(ToCartTaxDetailDTO).ToList();
            retVal.DynamicProperties = lineItem.DynamicProperties.Select(ToCartDynamicPropertyDTO).ToList();
            retVal.VolumetricWeight = (double)(lineItem.VolumetricWeight ?? 0);
            retVal.Weight = (double?)lineItem.Weight;
            retVal.Width = (double?)lineItem.Width;
            retVal.Height = (double?)lineItem.Height;
            retVal.Length = (double?)lineItem.Length;

            return retVal;
        }

        public virtual CartShipmentItem ToShipmentItem(LineItem lineItem)
        {
            var shipmentItem = new CartShipmentItem
            {
                LineItem = lineItem,
                Quantity = lineItem.Quantity
            };
            return shipmentItem;
        }

        public virtual PromotionProductEntry ToPromotionItem(LineItem lineItem)
        {
            var promoItem = ServiceLocator.Current.GetInstance<MarketingFactory>().CreatePromotionProductEntry();

            promoItem.InjectFrom(lineItem);

            promoItem.Discount = new Money(lineItem.DiscountTotal.Amount, lineItem.DiscountTotal.Currency);
            promoItem.Price = new Money(lineItem.PlacedPrice.Amount, lineItem.PlacedPrice.Currency);
            promoItem.Quantity = lineItem.Quantity;
            promoItem.Variations = null; // TODO

            return promoItem;
        }
    }
}
