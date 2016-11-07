using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;
using VirtoCommerce.Storefront.Model.Marketing.Factories;
using VirtoCommerce.Storefront.Model.Order;
using VirtoCommerce.Storefront.Model.Order.Factories;
using coreDto = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;
using orderDto = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class OrderConverterExtension
    {
        public static OrderConverter OrderConverterInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<OrderConverter>();
            }
        }

        public static CustomerOrder ToCustomerOrder(this orderDto.CustomerOrder orderDto, ICollection<Currency> availCurrencies, Language language)
        {
            return OrderConverterInstance.ToCustomerOrder(orderDto, availCurrencies, language);
        }

        public static TaxDetail ToTaxDetail(this orderDto.TaxDetail taxDetailDto, Currency currency)
        {
            return OrderConverterInstance.ToTaxDetail(taxDetailDto, currency);
        }

        public static Discount ToDiscount(this orderDto.Discount discountDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            return OrderConverterInstance.ToDiscount(discountDto, availCurrencies, language);
        }

        public static PaymentIn ToOrderInPayment(this orderDto.PaymentIn paymentInDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            return OrderConverterInstance.ToOrderInPayment(paymentInDto, availCurrencies, language);
        }

        public static LineItem ToOrderLineItem(this orderDto.LineItem lineItemDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            return OrderConverterInstance.ToOrderLineItem(lineItemDto, availCurrencies, language);
        }

        public static Model.Order.Shipment ToOrderShipment(this orderDto.Shipment shipmentDto, ICollection<Currency> availCurrencies, Language language)
        {
            return OrderConverterInstance.ToOrderShipment(shipmentDto, availCurrencies, language);
        }

        public static ShipmentItem ToShipmentItem(this orderDto.ShipmentItem shipmentItemDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            return OrderConverterInstance.ToShipmentItem(shipmentItemDto, availCurrencies, language);
        }

        public static ShipmentPackage ToShipmentPackage(this orderDto.ShipmentPackage shipmentPackageDto, IEnumerable<Currency> currencies, Language language)
        {
            return OrderConverterInstance.ToShipmentPackage(shipmentPackageDto, currencies, language);
        }

        public static orderDto.Address ToOrderAddressDto(this Address address)
        {
            return OrderConverterInstance.ToOrderAddressDto(address);
        }

        public static Address ToAddress(this orderDto.Address addressDto)
        {
            return OrderConverterInstance.ToAddress(addressDto);
        }

        public static DynamicProperty ToDynamicProperty(this orderDto.DynamicObjectProperty propertyDto)
        {
            return OrderConverterInstance.ToDynamicProperty(propertyDto);
        }

        public static orderDto.DynamicObjectProperty ToOrderDynamicPropertyDto(this DynamicProperty property)
        {
            return OrderConverterInstance.ToOrderDynamicPropertyDto(property);
        }
    }

    public class OrderConverter
    {
        public virtual DynamicProperty ToDynamicProperty(orderDto.DynamicObjectProperty propertyDto)
        {
            return propertyDto.JsonConvert<coreDto.DynamicObjectProperty>().ToDynamicProperty();
        }

        public virtual orderDto.DynamicObjectProperty ToOrderDynamicPropertyDto(DynamicProperty property)
        {
            return property.ToDynamicPropertyDto().JsonConvert<orderDto.DynamicObjectProperty>();
        }

        public virtual orderDto.Address ToOrderAddressDto(Address address)
        {
            return address.ToCoreAddressDto().JsonConvert<orderDto.Address>();
        }

        public virtual Address ToAddress(orderDto.Address addressDto)
        {
            return addressDto.JsonConvert<coreDto.Address>().ToAddress();
        }

        public virtual ShipmentPackage ToShipmentPackage(orderDto.ShipmentPackage shipmentPackageDto, IEnumerable<Currency> currencies, Language language)
        {
            var webModel = ServiceLocator.Current.GetInstance<OrderFactory>().CreateShipmentPackage();

            webModel.InjectFrom<NullableAndEnumValueInjecter>(shipmentPackageDto);

            if (shipmentPackageDto.Items != null)
            {
                webModel.Items = shipmentPackageDto.Items.Select(i => ToShipmentItem(i, currencies, language)).ToList();
            }

            return webModel;
        }

        public virtual ShipmentItem ToShipmentItem(orderDto.ShipmentItem shipmentItemDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            var result = ServiceLocator.Current.GetInstance<OrderFactory>().CreateShipmentItem();

            result.InjectFrom<NullableAndEnumValueInjecter>(shipmentItemDto);

            if (shipmentItemDto.LineItem != null)
            {
                result.LineItem = ToOrderLineItem(shipmentItemDto.LineItem, availCurrencies, language);
            }

            return result;
        }

        public virtual Shipment ToOrderShipment(orderDto.Shipment shipmentDto, ICollection<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(shipmentDto.Currency)) ?? new Currency(language, shipmentDto.Currency);
            var result = ServiceLocator.Current.GetInstance<OrderFactory>().CreateShipment(currency);

            result.InjectFrom<NullableAndEnumValueInjecter>(shipmentDto);

            result.Currency = currency;

            if (shipmentDto.DeliveryAddress != null)
            {
                result.DeliveryAddress = ToAddress(shipmentDto.DeliveryAddress);
            }

            if (!shipmentDto.Discounts.IsNullOrEmpty())
            {
                result.Discounts.AddRange(shipmentDto.Discounts.Select(x => ToDiscount(x, new[] { currency }, language)));
            }

            if (shipmentDto.DynamicProperties != null)
            {
                result.DynamicProperties = shipmentDto.DynamicProperties.Select(ToDynamicProperty).ToList();
            }

            if (shipmentDto.InPayments != null)
            {
                result.InPayments = shipmentDto.InPayments.Select(p => ToOrderInPayment(p, availCurrencies, language)).ToList();
            }

            if (shipmentDto.Items != null)
            {
                result.Items = shipmentDto.Items.Select(i => ToShipmentItem(i, availCurrencies, language)).ToList();
            }

            if (shipmentDto.Packages != null)
            {
                result.Packages = shipmentDto.Packages.Select(p => ToShipmentPackage(p, availCurrencies, language)).ToList();
            }

            result.Price = new Money(shipmentDto.Price ?? 0, currency);
            result.PriceWithTax = new Money(shipmentDto.PriceWithTax ?? 0, currency);
            result.DiscountAmount = new Money(shipmentDto.DiscountAmount ?? 0, currency);
            result.DiscountAmountWithTax = new Money(shipmentDto.DiscountAmountWithTax ?? 0, currency);
            result.Total = new Money(shipmentDto.Total ?? 0, currency);
            result.TotalWithTax = new Money(shipmentDto.TotalWithTax ?? 0, currency);
            result.TaxTotal = new Money(shipmentDto.TaxTotal ?? 0, currency);
            result.TaxPercentRate = (decimal?)shipmentDto.TaxPercentRate ?? 0m;
            if (shipmentDto.TaxDetails != null)
            {
                result.TaxDetails = shipmentDto.TaxDetails.Select(td => ToTaxDetail(td, currency)).ToList();
            }
            return result;
        }

        public virtual LineItem ToOrderLineItem(orderDto.LineItem lineItemDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(lineItemDto.Currency)) ?? new Currency(language, lineItemDto.Currency);

            var result = ServiceLocator.Current.GetInstance<OrderFactory>().CreateLineItem(currency);
            result.InjectFrom<NullableAndEnumValueInjecter>(lineItemDto);
            result.ImageUrl = result.ImageUrl.RemoveLeadingUriScheme();
            result.Currency = currency;
            result.DiscountAmount = new Money(lineItemDto.DiscountAmount ?? 0, currency);

            if (lineItemDto.DynamicProperties != null)
            {
                result.DynamicProperties = lineItemDto.DynamicProperties.Select(ToDynamicProperty).ToList();
            }
            result.Price = new Money(lineItemDto.Price ?? 0, currency);
            result.PriceWithTax = new Money(lineItemDto.PriceWithTax ?? 0, currency);
            result.DiscountAmount = new Money(lineItemDto.DiscountAmount ?? 0, currency);
            result.DiscountAmountWithTax = new Money(lineItemDto.DiscountAmountWithTax ?? 0, currency);
            result.PlacedPrice = new Money(lineItemDto.PlacedPrice ?? 0, currency);
            result.PlacedPriceWithTax = new Money(lineItemDto.PlacedPriceWithTax ?? 0, currency);
            result.ExtendedPrice = new Money(lineItemDto.ExtendedPrice ?? 0, currency);
            result.ExtendedPriceWithTax = new Money(lineItemDto.ExtendedPriceWithTax ?? 0, currency);
            result.DiscountTotal = new Money(lineItemDto.DiscountTotal ?? 0, currency);
            result.DiscountTotalWithTax = new Money(lineItemDto.DiscountTotalWithTax ?? 0, currency);
            result.TaxTotal = new Money(lineItemDto.TaxTotal ?? 0, currency);
            result.TaxPercentRate = (decimal?)lineItemDto.TaxPercentRate ?? 0m;
            if (!lineItemDto.Discounts.IsNullOrEmpty())
            {
                result.Discounts.AddRange(lineItemDto.Discounts.Select(x => ToDiscount(x, new[] { currency }, language)));
            }
            if (lineItemDto.TaxDetails != null)
            {
                result.TaxDetails = lineItemDto.TaxDetails.Select(td => ToTaxDetail(td, currency)).ToList();
            }

            return result;
        }

        public virtual PaymentIn ToOrderInPayment(orderDto.PaymentIn paymentIn, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(paymentIn.Currency)) ?? new Currency(language, paymentIn.Currency);
            var retVal = ServiceLocator.Current.GetInstance<OrderFactory>().CreatePaymentIn(currency);

            retVal.InjectFrom(paymentIn);

            if (paymentIn.DynamicProperties != null)
            {
                retVal.DynamicProperties = paymentIn.DynamicProperties.Select(ToDynamicProperty).ToList();
            }

            retVal.Sum = new Money(paymentIn.Sum ?? 0, currency);

            if (paymentIn.PaymentMethod != null)
            {
                retVal.GatewayCode = paymentIn.PaymentMethod.Code;
            }
            return retVal;
        }

        public virtual Discount ToDiscount(orderDto.Discount discountDto, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(discountDto.Currency)) ?? new Currency(language, discountDto.Currency);
            var result = ServiceLocator.Current.GetInstance<MarketingFactory>().CreateDiscount(currency);

            result.InjectFrom<NullableAndEnumValueInjecter>(discountDto);

            result.Amount = new Money(discountDto.DiscountAmount ?? 0, currency);

            return result;
        }

        public virtual CustomerOrder ToCustomerOrder(orderDto.CustomerOrder order, ICollection<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(order.Currency)) ?? new Currency(language, order.Currency);

            var result = ServiceLocator.Current.GetInstance<OrderFactory>().CreateCustomerOrder(currency);

            result.InjectFrom<NullableAndEnumValueInjecter>(order);

            if (order.Addresses != null)
            {
                result.Addresses = order.Addresses.Select(ToAddress).ToList();
            }


            if (order.DynamicProperties != null)
            {
                result.DynamicProperties = order.DynamicProperties.Select(ToDynamicProperty).ToList();
            }

            if (order.InPayments != null)
            {
                result.InPayments = order.InPayments.Select(p => ToOrderInPayment(p, availCurrencies, language)).ToList();
            }

            if (order.Items != null)
            {
                result.Items = order.Items.Select(i => ToOrderLineItem(i, availCurrencies, language)).ToList();
            }

            if (order.Shipments != null)
            {
                result.Shipments = order.Shipments.Select(s => ToOrderShipment(s, availCurrencies, language)).ToList();
            }

            if (!order.Discounts.IsNullOrEmpty())
            {
                result.Discounts.AddRange(order.Discounts.Select(x => ToDiscount(x, new[] { currency }, language)));
            }
            if (order.TaxDetails != null)
            {
                result.TaxDetails = order.TaxDetails.Select(td => ToTaxDetail(td, currency)).ToList();
            }

            result.DiscountAmount = new Money(order.DiscountAmount ?? 0, currency);
            result.Total = new Money(order.Total ?? 0, currency);
            result.SubTotal = new Money(order.SubTotal ?? 0, currency);
            result.SubTotalWithTax = new Money(order.SubTotalWithTax ?? 0, currency);
            result.TaxTotal = new Money(order.TaxTotal ?? 0, currency);
            result.ShippingTotal = new Money(order.ShippingTotal ?? 0, currency);
            result.ShippingTotalWithTax = new Money(order.ShippingTotalWithTax ?? 0, currency);
            result.ShippingTaxTotal = new Money(order.ShippingTaxTotal ?? 0, currency);
            result.SubTotalTaxTotal = new Money(order.SubTotalTaxTotal ?? 0, currency);
            result.SubTotalDiscount = new Money(order.SubTotalDiscount ?? 0, currency);
            result.SubTotalDiscountWithTax = new Money(order.SubTotalDiscountWithTax ?? 0, currency);


            return result;
        }

        public virtual TaxDetail ToTaxDetail(orderDto.TaxDetail taxDetailDto, Currency currency)
        {
            var result = new TaxDetail(currency);
            result.InjectFrom(taxDetailDto);
            return result;
        }
    }
}
