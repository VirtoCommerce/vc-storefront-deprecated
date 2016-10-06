using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model.Order.Factories
{
    public class OrderFactory
    {
        public virtual PaymentIn CreatePaymentIn(Currency currency)
        {
            return new PaymentIn(currency);
        }
        public virtual Shipment CreateShipment(Currency currency)
        {
            return new Shipment(currency);
        }

        public virtual LineItem CreateLineItem(Currency currency)
        {
            return new LineItem(currency);
        }
        public virtual CustomerOrder CreateCustomerOrder(Currency currency)
        {
            return new CustomerOrder(currency);
        }

        public virtual ShipmentItem CreateShipmentItem()
        {
            return new ShipmentItem();
        }

        public virtual ShipmentPackage CreateShipmentPackage()
        {
            return new ShipmentPackage();
        }
    }
}
