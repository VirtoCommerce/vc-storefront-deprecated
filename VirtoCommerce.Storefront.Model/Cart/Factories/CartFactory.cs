using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model.Cart.Factories
{
    public class CartFactory
    {
        public virtual ShippingMethod CreateShippingMethod(Currency currency)
        {
            return new ShippingMethod(currency);
        }

        public virtual CartShipmentItem CreateShipmentItem()
        {
            return new CartShipmentItem();
        }

        public virtual PaymentMethod CreatePaymentMethod()
        {
            return new PaymentMethod();
        }

        public virtual Shipment CreateShipment(Currency currency)
        {
            return new Shipment(currency);
        }

        public virtual Payment CreatePayment(Currency currency)
        {
            return new Payment(currency);
        }

        public virtual ShoppingCart CreateCart(Currency currency, Language language)
        {
            return new ShoppingCart(currency, language);
        }

        public virtual LineItem CreateLineItem(Currency currency, Language language)
        {
            return new LineItem(currency, language);
        }
    }
}
