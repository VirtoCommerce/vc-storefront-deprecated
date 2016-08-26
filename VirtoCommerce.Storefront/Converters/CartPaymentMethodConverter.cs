using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Common;
using cartModel = VirtoCommerce.Storefront.AutoRestClients.CartModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CartPaymentMethodConverter
    {
        public static PaymentMethod ToWebModel(this cartModel.PaymentMethod paymentMethod)
        {
            var paymentMethodWebModel = new PaymentMethod();
            paymentMethodWebModel.InjectFrom(paymentMethod);
            paymentMethodWebModel.Priority = paymentMethod.Priority ?? 0;

            return paymentMethodWebModel;
        }

        public static Payment ToPaymentModel(this PaymentMethod paymentMethod, Money amount, Currency currency)
        {
            var paymentWebModel = new Payment(currency)
            {
                Amount = amount,
                Currency = currency,
                PaymentGatewayCode = paymentMethod.GatewayCode
            };


            return paymentWebModel;
        }
    }
}
