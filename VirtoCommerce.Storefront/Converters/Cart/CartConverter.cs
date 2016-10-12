using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Cart.Factories;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;
using VirtoCommerce.Storefront.Model.Marketing.Factories;
using VirtoCommerce.Storefront.Model.Tax;
using VirtoCommerce.Storefront.Model.Tax.Factories;
using cartDto = VirtoCommerce.Storefront.AutoRestClients.CartModuleApi.Models;
using coreDto = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;
using platformDto = VirtoCommerce.Storefront.AutoRestClients.PlatformModuleApi.Models;

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

        public static ShoppingCart ToShoppingCart(this cartDto.ShoppingCart cartDto, Currency currency, Language language, Model.Customer.CustomerInfo customer)
        {
            return CartConverterInstance.ToShoppingCart(cartDto, currency, language, customer);
        }

        public static cartDto.ShoppingCart ToShoppingCartDto(this ShoppingCart cart)
        {
            return CartConverterInstance.ToShoppingCartDto(cart);
        }

        public static PromotionEvaluationContext ToPromotionEvaluationContext(this ShoppingCart cart)
        {
            return CartConverterInstance.ToPromotionEvaluationContext(cart);
        }

        public static TaxEvaluationContext ToTaxEvalContext(this ShoppingCart cart)
        {
            return CartConverterInstance.ToTaxEvalContext(cart);
        }

        public static TaxDetail ToTaxDetail(this cartDto.TaxDetail taxDetail, Currency currency)
        {
            return CartConverterInstance.ToTaxDetail(taxDetail, currency);
        }

        public static cartDto.TaxDetail ToCartTaxDetailDto(this TaxDetail taxDetail)
        {
            return CartConverterInstance.ToCartTaxDetailDto(taxDetail);
        }

        public static LineItem ToLineItem(this Product product, Language language, int quantity)
        {
            return CartConverterInstance.ToLineItem(product, language, quantity);
        }

        public static LineItem ToLineItem(this cartDto.LineItem lineItemDto, Currency currency, Language language)
        {
            return CartConverterInstance.ToLineItem(lineItemDto, currency, language);
        }

        public static cartDto.LineItem ToLineItemDto(this LineItem lineItem)
        {
            return CartConverterInstance.ToLineItemDto(lineItem);
        }

        public static CartShipmentItem ToShipmentItem(this LineItem lineItem)
        {
            return CartConverterInstance.ToShipmentItem(lineItem);
        }

        public static PromotionProductEntry ToPromotionItem(this LineItem lineItem)
        {
            return CartConverterInstance.ToPromotionItem(lineItem);
        }

        public static cartDto.Address ToCartAddressDto(this Address address)
        {
            return CartConverterInstance.ToCartAddressDto(address);
        }

        public static Address ToAddress(this cartDto.Address addressDto)
        {
            return CartConverterInstance.ToAddress(addressDto);
        }

        public static Payment ToPayment(this cartDto.Payment paymentDto, Currency currency)
        {
            return CartConverterInstance.ToPayment(paymentDto, currency);
        }

        public static cartDto.Payment ToPaymentDto(this Payment payment)
        {
            return CartConverterInstance.ToPaymentDto(payment);
        }

        public static PaymentMethod ToPaymentMethod(this cartDto.PaymentMethod paymentMethodDto)
        {
            return CartConverterInstance.ToPaymentMethod(paymentMethodDto);
        }

        public static Payment ToCartPayment(this PaymentMethod paymentMethod, Money amount, Currency currency)
        {
            return CartConverterInstance.ToCartPayment(paymentMethod, amount, currency);
        }

        public static Shipment ToShipment(this cartDto.Shipment shipmentDto, ShoppingCart cart)
        {
            return CartConverterInstance.ToShipment(shipmentDto, cart);
        }

        public static cartDto.Shipment ToShipmentDto(this Shipment shipment)
        {
            return CartConverterInstance.ToShipmentDto(shipment);
        }

        public static CartShipmentItem ToShipmentItem(this cartDto.ShipmentItem shipmentItemDto, ShoppingCart cart)
        {
            return CartConverterInstance.ToShipmentItem(shipmentItemDto, cart);
        }

        public static cartDto.ShipmentItem ToShipmentItemDto(this CartShipmentItem shipmentItem)
        {
            return CartConverterInstance.ToShipmentItemDto(shipmentItem);
        }

        public static ShippingMethod ToShippingMethod(this cartDto.ShippingRate shippingRate, Currency currency, IEnumerable<Currency> availCurrencies)
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

        public static Discount ToDiscount(this cartDto.Discount discountDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            return CartConverterInstance.ToDiscount(discountDto, availCurrencies, language);
        }

        public static cartDto.Discount ToCartDiscountDto(this Discount discount)
        {
            return CartConverterInstance.ToCartDiscountDto(discount);
        }

        public static DynamicProperty ToDynamicProperty(this cartDto.DynamicObjectProperty propertyDto)
        {
            return CartConverterInstance.ToDynamicProperty(propertyDto);
        }

        public static cartDto.DynamicObjectProperty ToCartDynamicPropertyDto(this DynamicProperty property)
        {
            return CartConverterInstance.ToCartDynamicPropertyDto(property);
        }
    }

    public class CartConverter
    {
        public virtual DynamicProperty ToDynamicProperty(cartDto.DynamicObjectProperty propertyDto)
        {
            return propertyDto.JsonConvert<coreDto.DynamicObjectProperty>().ToDynamicProperty();
        }

        public virtual cartDto.DynamicObjectProperty ToCartDynamicPropertyDto(DynamicProperty property)
        {
            return property.ToDynamicPropertyDto().JsonConvert<cartDto.DynamicObjectProperty>();
        }

        public virtual Discount ToDiscount(cartDto.Discount discountDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(discountDto.Currency)) ?? new Currency(language, discountDto.Currency);

            var result = ServiceLocator.Current.GetInstance<MarketingFactory>().CreateDiscount(currency);

            result.InjectFrom<NullableAndEnumValueInjecter>(discountDto);
            result.Amount = new Money(discountDto.DiscountAmount ?? 0, currency);

            return result;
        }

        public virtual cartDto.Discount ToCartDiscountDto(Discount webModel)
        {
            var result = new cartDto.Discount();

            result.InjectFrom<NullableAndEnumValueInjecter>(webModel);

            result.Currency = webModel.Amount.Currency.Code;
            result.DiscountAmount = (double)webModel.Amount.Amount;

            return result;
        }

        public virtual ShippingMethod ToShippingMethod(cartDto.ShippingRate shippingRate, Currency currency, IEnumerable<Currency> availCurrencies)
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
                    result.Settings = shippingRate.ShippingMethod.Settings.Select(x => x.JsonConvert<platformDto.Setting>().ToSettingEntry()).ToList();
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

        public virtual CartShipmentItem ToShipmentItem(cartDto.ShipmentItem shipmentItemDto, ShoppingCart cart)
        {
            var result = ServiceLocator.Current.GetInstance<CartFactory>().CreateShipmentItem();

            result.InjectFrom<NullableAndEnumValueInjecter>(shipmentItemDto);

            result.LineItem = cart.Items.FirstOrDefault(x => x.Id == shipmentItemDto.LineItemId);

            return result;
        }

        public virtual cartDto.ShipmentItem ToShipmentItemDto(CartShipmentItem webModel)
        {
            var result = new cartDto.ShipmentItem();

            result.InjectFrom<NullableAndEnumValueInjecter>(webModel);

            result.LineItem = webModel.LineItem.ToLineItemDto();

            return result;
        }

        public virtual Shipment ToShipment(cartDto.Shipment shipmentDto, ShoppingCart cart)
        {
            var retVal = ServiceLocator.Current.GetInstance<CartFactory>().CreateShipment(cart.Currency);

            retVal.InjectFrom(shipmentDto);
            retVal.Currency = cart.Currency;
            retVal.Price = new Money(shipmentDto.Price ?? 0, cart.Currency);
            retVal.DiscountAmount = new Money(shipmentDto.DiscountAmount ?? 0, cart.Currency);
            retVal.TaxPercentRate = (decimal?)shipmentDto.TaxPercentRate ?? 0m;

            if (shipmentDto.DeliveryAddress != null)
            {
                retVal.DeliveryAddress = ToAddress(shipmentDto.DeliveryAddress);
            }

            if (shipmentDto.Items != null)
            {
                retVal.Items = shipmentDto.Items.Select(i => ToShipmentItem(i, cart)).ToList();
            }

            if (shipmentDto.TaxDetails != null)
            {
                retVal.TaxDetails = shipmentDto.TaxDetails.Select(td => ToTaxDetail(td, cart.Currency)).ToList();
            }

            if (!shipmentDto.Discounts.IsNullOrEmpty())
            {
                retVal.Discounts.AddRange(shipmentDto.Discounts.Select(x => ToDiscount(x, new[] { cart.Currency }, cart.Language)));
            }
            return retVal;
        }

        public virtual cartDto.Shipment ToShipmentDto(Shipment shipment)
        {
            var retVal = new cartDto.Shipment();

            retVal.InjectFrom(shipment);
            retVal.Currency = shipment.Currency != null ? shipment.Currency.Code : null;
            retVal.DiscountAmount = shipment.DiscountAmount != null ? (double?)shipment.DiscountAmount.Amount : null;
            retVal.Price = shipment.Price != null ? (double?)shipment.Price.Amount : null;
            retVal.TaxPercentRate = (double)shipment.TaxPercentRate;

            if (shipment.DeliveryAddress != null)
            {
                retVal.DeliveryAddress = ToCartAddressDto(shipment.DeliveryAddress);
            }

            if (shipment.Discounts != null)
            {
                retVal.Discounts = shipment.Discounts.Select(ToCartDiscountDto).ToList();
            }

            if (shipment.Items != null)
            {
                retVal.Items = shipment.Items.Select(ToShipmentItemDto).ToList();
            }

            if (shipment.TaxDetails != null)
            {
                retVal.TaxDetails = shipment.TaxDetails.Select(ToCartTaxDetailDto).ToList();
            }

            return retVal;
        }

        public virtual PaymentMethod ToPaymentMethod(cartDto.PaymentMethod paymentMethodDto)
        {
            var retVal = ServiceLocator.Current.GetInstance<CartFactory>().CreatePaymentMethod();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(paymentMethodDto);
            retVal.Priority = paymentMethodDto.Priority ?? 0;

            if (paymentMethodDto.Settings != null)
            {
                retVal.Settings = paymentMethodDto.Settings.Select(x => x.JsonConvert<platformDto.Setting>().ToSettingEntry()).ToList();
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

        public virtual Payment ToPayment(cartDto.Payment paymentDto, Currency currency)
        {
            var result = ServiceLocator.Current.GetInstance<CartFactory>().CreatePayment(currency);

            result.InjectFrom<NullableAndEnumValueInjecter>(paymentDto);

            result.Amount = new Money(paymentDto.Amount ?? 0, currency);

            if (paymentDto.BillingAddress != null)
            {
                result.BillingAddress = ToAddress(paymentDto.BillingAddress);
            }

            result.Currency = currency;

            return result;
        }

        public virtual cartDto.Payment ToPaymentDto(Payment payment)
        {
            var result = new cartDto.Payment();

            result.InjectFrom<NullableAndEnumValueInjecter>(payment);

            result.Amount = (double)payment.Amount.Amount;

            if (payment.BillingAddress != null)
            {
                result.BillingAddress = ToCartAddressDto(payment.BillingAddress);
            }

            result.Currency = payment.Currency.Code;

            return result;
        }

        public virtual cartDto.Address ToCartAddressDto(Address address)
        {
            return address.ToCoreAddressDto().JsonConvert<cartDto.Address>();
        }

        public virtual Address ToAddress(cartDto.Address addressDto)
        {
            return addressDto.JsonConvert<coreDto.Address>().ToAddress();
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

        public virtual ShoppingCart ToShoppingCart(cartDto.ShoppingCart cartDto, Currency currency, Language language, Model.Customer.CustomerInfo customer)
        {
            var result = ServiceLocator.Current.GetInstance<CartFactory>().CreateCart(currency, language);

            result.InjectFrom<NullableAndEnumValueInjecter>(cartDto);

            result.Customer = customer;

            if (cartDto.Coupon != null)
            {
                result.Coupon = new Coupon
                {
                    Code = cartDto.Coupon.Code,
                    AppliedSuccessfully = cartDto.Coupon.IsValid ?? false
                };
            }

            if (cartDto.Items != null)
            {
                result.Items = cartDto.Items.Select(i => ToLineItem(i, currency, language)).ToList();
                result.HasPhysicalProducts = result.Items.Any(i =>
                    string.IsNullOrEmpty(i.ProductType) ||
                    !string.IsNullOrEmpty(i.ProductType) && i.ProductType.Equals("Physical", StringComparison.OrdinalIgnoreCase));
            }

            if (cartDto.Addresses != null)
            {
                result.Addresses = cartDto.Addresses.Select(ToAddress).ToList();

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

            if (cartDto.Payments != null)
            {
                result.Payments = cartDto.Payments.Select(p => ToPayment(p, currency)).ToList();
            }

            if (cartDto.Shipments != null)
            {
                result.Shipments = cartDto.Shipments.Select(s => ToShipment(s, result)).ToList();
            }

            if (cartDto.DynamicProperties != null)
            {
                result.DynamicProperties = cartDto.DynamicProperties.Select(ToDynamicProperty).ToList();
            }

            if (cartDto.TaxDetails != null)
            {
                result.TaxDetails = cartDto.TaxDetails.Select(td => ToTaxDetail(td, currency)).ToList();
            }
            result.DiscountAmount = new Money(cartDto.DiscountAmount ?? 0, currency);
            result.HandlingTotal = new Money(cartDto.HandlingTotal ?? 0, currency);
            result.HandlingTotalWithTax = new Money(cartDto.HandlingTotalWithTax ?? 0, currency);
            result.IsAnonymous = cartDto.IsAnonymous == true;
            result.IsRecuring = cartDto.IsRecuring == true;
            result.VolumetricWeight = (decimal)(cartDto.VolumetricWeight ?? 0);
            result.Weight = (decimal)(cartDto.Weight ?? 0);

            return result;
        }


        public virtual cartDto.ShoppingCart ToShoppingCartDto(ShoppingCart cart)
        {
            var result = new cartDto.ShoppingCart();

            result.InjectFrom<NullableAndEnumValueInjecter>(cart);

            if(cart.Language != null)
            {
                result.LanguageCode = cart.Language.CultureName;
            }
            result.Addresses = cart.Addresses.Select(ToCartAddressDto).ToList();
            result.Coupon = cart.Coupon != null ? new cartDto.Coupon { Code = cart.Coupon.Code, IsValid = cart.Coupon.AppliedSuccessfully } : null;
            result.Currency = cart.Currency.Code;
            result.Discounts = cart.Discounts.Select(ToCartDiscountDto).ToList();
            result.HandlingTotal = (double)cart.HandlingTotal.Amount;
            result.HandlingTotalWithTax = (double)cart.HandlingTotal.Amount;
            result.DiscountAmount = (double)cart.DiscountAmount.Amount;
            result.Items = cart.Items.Select(ToLineItemDto).ToList();
            result.Payments = cart.Payments.Select(ToPaymentDto).ToList();
            result.Shipments = cart.Shipments.Select(ToShipmentDto).ToList();
            result.TaxDetails = cart.TaxDetails.Select(ToCartTaxDetailDto).ToList();
            result.DynamicProperties = cart.DynamicProperties.Select(ToCartDynamicPropertyDto).ToList();
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

        public virtual TaxDetail ToTaxDetail(cartDto.TaxDetail taxDeatilDto, Currency currency)
        {
            var result = new TaxDetail(currency);
            result.InjectFrom(taxDeatilDto);
            return result;
        }

        public virtual cartDto.TaxDetail ToCartTaxDetailDto(TaxDetail taxDetail)
        {
            var result = new cartDto.TaxDetail();
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

        public virtual LineItem ToLineItem(cartDto.LineItem lineItemDto, Currency currency, Language language)
        {
            var result = ServiceLocator.Current.GetInstance<CartFactory>().CreateLineItem(currency, language);

            result.InjectFrom<NullableAndEnumValueInjecter>(lineItemDto);

            if (lineItemDto.TaxDetails != null)
            {
                result.TaxDetails = lineItemDto.TaxDetails.Select(td => ToTaxDetail(td, currency)).ToList();
            }

            if (lineItemDto.DynamicProperties != null)
            {
                result.DynamicProperties = lineItemDto.DynamicProperties.Select(ToDynamicProperty).ToList();
            }

            if (!lineItemDto.Discounts.IsNullOrEmpty())
            {
                result.Discounts.AddRange(lineItemDto.Discounts.Select(x => ToDiscount(x, new[] { currency }, language)));
            }
            result.Comment = lineItemDto.Note;
            result.IsGift = lineItemDto.IsGift == true;
            result.IsReccuring = lineItemDto.IsReccuring == true;
            result.ListPrice = new Money(lineItemDto.ListPrice ?? 0, currency);
            result.RequiredShipping = lineItemDto.RequiredShipping == true;
            result.SalePrice = new Money(lineItemDto.SalePrice ?? 0, currency);
            result.TaxPercentRate = (decimal?)lineItemDto.TaxPercentRate ?? 0m;
            result.DiscountAmount = new Money(lineItemDto.DiscountAmount ?? 0, currency);
            result.TaxIncluded = lineItemDto.TaxIncluded == true;
            result.Weight = (decimal?)lineItemDto.Weight;
            result.Width = (decimal?)lineItemDto.Width;
            result.Height = (decimal?)lineItemDto.Height;
            result.Length = (decimal?)lineItemDto.Length;

            return result;
        }

        public virtual cartDto.LineItem ToLineItemDto(LineItem lineItem)
        {
            var retVal = new cartDto.LineItem();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(lineItem);

            retVal.Currency = lineItem.Currency.Code;
            retVal.Discounts = lineItem.Discounts.Select(ToCartDiscountDto).ToList();

            retVal.ListPrice = (double)lineItem.ListPrice.Amount;
            retVal.SalePrice = (double)lineItem.SalePrice.Amount;
            retVal.TaxPercentRate = (double)lineItem.TaxPercentRate;
            retVal.DiscountAmount = (double)lineItem.DiscountAmount.Amount;
            retVal.TaxDetails = lineItem.TaxDetails.Select(ToCartTaxDetailDto).ToList();
            retVal.DynamicProperties = lineItem.DynamicProperties.Select(ToCartDynamicPropertyDto).ToList();
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
