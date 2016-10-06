using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Marketing;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Order;
using VirtoCommerce.Storefront.Model.Order.Factories;
using orderDTO = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;
using VirtoCommerce.Storefront.Model.Marketing.Factories;
using VirtoCommerce.Storefront.Common;
using coreDTO = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;

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

        public static CustomerOrder ToCustomerOrder(this orderDTO.CustomerOrder orderDTO, ICollection<Currency> availCurrencies, Language language)
        {           
            return OrderConverterInstance.ToCustomerOrder(orderDTO, availCurrencies, language);
        }
        
        public static TaxDetail ToTaxDetail(this orderDTO.TaxDetail taxDetailDTO, Currency currency)
        {
            return OrderConverterInstance.ToTaxDetail(taxDetailDTO, currency);
        }

        public static Discount ToDiscount(this orderDTO.Discount discountDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            return OrderConverterInstance.ToDiscount(discountDTO, availCurrencies, language);
        }

        public static PaymentIn ToOrderInPayment(this orderDTO.PaymentIn paymentInDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            return OrderConverterInstance.ToOrderInPayment(paymentInDTO, availCurrencies, language);
        }

        public static LineItem ToOrderLineItem(this orderDTO.LineItem lineItemDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            return OrderConverterInstance.ToOrderLineItem(lineItemDTO, availCurrencies, language);
        }

        public static Model.Order.Shipment ToOrderShipment(this orderDTO.Shipment shipmentDTO, ICollection<Currency> availCurrencies, Language language)
        {
            return OrderConverterInstance.ToOrderShipment(shipmentDTO, availCurrencies, language);
        }

        public static ShipmentItem ToShipmentItem(this orderDTO.ShipmentItem shipmentItemDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            return OrderConverterInstance.ToShipmentItem(shipmentItemDTO, availCurrencies, language);
        }

        public static ShipmentPackage ToShipmentPackage(this orderDTO.ShipmentPackage shipmentPackageDTO, IEnumerable<Currency> currencies, Language language)
        {
            return OrderConverterInstance.ToShipmentPackage(shipmentPackageDTO, currencies, language);
        }

        public static orderDTO.Address ToOrderAddressDTO(this Address address)
        {
            return OrderConverterInstance.ToOrderAddressDTO(address);
        }

        public static Address ToAddress(this orderDTO.Address addressDTO)
        {
            return OrderConverterInstance.ToAddress(addressDTO);
        }

        public static DynamicProperty ToDynamicProperty(this orderDTO.DynamicObjectProperty propertyDTO)
        {
            return OrderConverterInstance.ToDynamicProperty(propertyDTO);
        }

        public static orderDTO.DynamicObjectProperty ToOrderDynamicPropertyDTO(this DynamicProperty property)
        {
            return OrderConverterInstance.ToOrderDynamicPropertyDTO(property);
        }
    }

    public class OrderConverter
    {
        public virtual DynamicProperty ToDynamicProperty(orderDTO.DynamicObjectProperty propertyDTO)
        {
            return propertyDTO.JsonConvert<coreDTO.DynamicObjectProperty>().ToDynamicProperty();
        }

        public virtual orderDTO.DynamicObjectProperty ToOrderDynamicPropertyDTO(DynamicProperty property)
        {
            return property.ToDynamicPropertyDTO().JsonConvert<orderDTO.DynamicObjectProperty>();
        }

        public virtual orderDTO.Address ToOrderAddressDTO(Address address)
        {
            return address.ToCoreAddressDTO().JsonConvert<orderDTO.Address>();
        }

        public virtual Address ToAddress(orderDTO.Address addressDTO)
        {
            return addressDTO.JsonConvert<coreDTO.Address>().ToAddress();
        }

        public virtual ShipmentPackage ToShipmentPackage(orderDTO.ShipmentPackage shipmentPackageDTO, IEnumerable<Currency> currencies, Language language)
        {
            var webModel = ServiceLocator.Current.GetInstance<OrderFactory>().CreateShipmentPackage();

            webModel.InjectFrom<NullableAndEnumValueInjecter>(shipmentPackageDTO);

            if (shipmentPackageDTO.Items != null)
            {
                webModel.Items = shipmentPackageDTO.Items.Select(i => ToShipmentItem(i, currencies, language)).ToList();
            }

            return webModel;
        }

        public virtual ShipmentItem ToShipmentItem(orderDTO.ShipmentItem shipmentItemDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            var result = ServiceLocator.Current.GetInstance<OrderFactory>().CreateShipmentItem();

            result.InjectFrom<NullableAndEnumValueInjecter>(shipmentItemDTO);

            if (shipmentItemDTO.LineItem != null)
            {
                result.LineItem = ToOrderLineItem(shipmentItemDTO.LineItem, availCurrencies, language);
            }

            return result;
        }

        public virtual Shipment ToOrderShipment(orderDTO.Shipment shipmentDTO, ICollection<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(shipmentDTO.Currency)) ?? new Currency(language, shipmentDTO.Currency);
            var result = ServiceLocator.Current.GetInstance<OrderFactory>().CreateShipment(currency);

            result.InjectFrom<NullableAndEnumValueInjecter>(shipmentDTO);

            result.Currency = currency;

            if (shipmentDTO.DeliveryAddress != null)
            {
                result.DeliveryAddress = ToAddress(shipmentDTO.DeliveryAddress);
            }

            if (!shipmentDTO.Discounts.IsNullOrEmpty())
            {
                result.Discounts.AddRange(shipmentDTO.Discounts.Select(x => ToDiscount(x, new[] { currency }, language)));
            }

            if (shipmentDTO.DynamicProperties != null)
            {
                result.DynamicProperties = shipmentDTO.DynamicProperties.Select(ToDynamicProperty).ToList();
            }

            if (shipmentDTO.InPayments != null)
            {
                result.InPayments = shipmentDTO.InPayments.Select(p =>ToOrderInPayment(p, availCurrencies, language)).ToList();
            }

            if (shipmentDTO.Items != null)
            {
                result.Items = shipmentDTO.Items.Select(i => ToShipmentItem(i, availCurrencies, language)).ToList();
            }

            if (shipmentDTO.Packages != null)
            {
                result.Packages = shipmentDTO.Packages.Select(p => ToShipmentPackage(p, availCurrencies, language)).ToList();
            }

            result.Price = new Money(shipmentDTO.Price ?? 0, currency);
            result.PriceWithTax = new Money(shipmentDTO.PriceWithTax ?? 0, currency);
            result.DiscountAmount = new Money(shipmentDTO.DiscountAmount ?? 0, currency);
            result.DiscountAmountWithTax = new Money(shipmentDTO.DiscountAmountWithTax ?? 0, currency);
            result.Total = new Money(shipmentDTO.Total ?? 0, currency);
            result.TotalWithTax = new Money(shipmentDTO.TotalWithTax ?? 0, currency);
            result.TaxTotal = new Money(shipmentDTO.TaxTotal ?? 0, currency);
            result.TaxPercentRate = (decimal?)shipmentDTO.TaxPercentRate ?? 0m;
            if (shipmentDTO.TaxDetails != null)
            {
                result.TaxDetails = shipmentDTO.TaxDetails.Select(td => ToTaxDetail(td, currency)).ToList();
            }
            return result;
        }

        public virtual LineItem ToOrderLineItem(orderDTO.LineItem lineItemDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(lineItemDTO.Currency)) ?? new Currency(language, lineItemDTO.Currency);

            var result = ServiceLocator.Current.GetInstance<OrderFactory>().CreateLineItem(currency);
            result.InjectFrom<NullableAndEnumValueInjecter>(lineItemDTO);

            result.Currency = currency;
            result.DiscountAmount = new Money(lineItemDTO.DiscountAmount ?? 0, currency);

            if (lineItemDTO.DynamicProperties != null)
            {
                result.DynamicProperties = lineItemDTO.DynamicProperties.Select(ToDynamicProperty).ToList();
            }
            result.Price = new Money(lineItemDTO.Price ?? 0, currency);
            result.PriceWithTax = new Money(lineItemDTO.PriceWithTax ?? 0, currency);
            result.DiscountAmount = new Money(lineItemDTO.DiscountAmount ?? 0, currency);
            result.DiscountAmountWithTax = new Money(lineItemDTO.DiscountAmountWithTax ?? 0, currency);
            result.PlacedPrice = new Money(lineItemDTO.PlacedPrice ?? 0, currency);
            result.PlacedPriceWithTax = new Money(lineItemDTO.PlacedPriceWithTax ?? 0, currency);
            result.ExtendedPrice = new Money(lineItemDTO.ExtendedPrice ?? 0, currency);
            result.ExtendedPriceWithTax = new Money(lineItemDTO.ExtendedPriceWithTax ?? 0, currency);
            result.DiscountTotal = new Money(lineItemDTO.DiscountTotal ?? 0, currency);
            result.DiscountTotalWithTax = new Money(lineItemDTO.DiscountTotalWithTax ?? 0, currency);
            result.TaxTotal = new Money(lineItemDTO.TaxTotal ?? 0, currency);
            result.TaxPercentRate = (decimal?)lineItemDTO.TaxPercentRate ?? 0m;
            if (!lineItemDTO.Discounts.IsNullOrEmpty())
            {
                result.Discounts.AddRange(lineItemDTO.Discounts.Select(x => ToDiscount(x, new[] { currency }, language)));
            }
            if (lineItemDTO.TaxDetails != null)
            {
                result.TaxDetails = lineItemDTO.TaxDetails.Select(td => ToTaxDetail(td, currency)).ToList();
            }

            return result;
        }

        public virtual PaymentIn ToOrderInPayment(orderDTO.PaymentIn paymentIn, IEnumerable<Currency> availCurrencies, Language language)
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

        public virtual Discount ToDiscount(orderDTO.Discount discountDTO, IEnumerable<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(discountDTO.Currency)) ?? new Currency(language, discountDTO.Currency);
            var result = ServiceLocator.Current.GetInstance<MarketingFactory>().CreateDiscount(currency);

            result.InjectFrom<NullableAndEnumValueInjecter>(discountDTO);

            result.Amount = new Money(discountDTO.DiscountAmount ?? 0, currency);

            return result;
        }

        public virtual CustomerOrder ToCustomerOrder(orderDTO.CustomerOrder order, ICollection<Currency> availCurrencies, Language language)
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

        public virtual TaxDetail ToTaxDetail(orderDTO.TaxDetail taxDetailDTO, Currency currency)
        {
            var result = new TaxDetail(currency);
            result.InjectFrom(taxDetailDTO);
            return result;
        }
    }
}
