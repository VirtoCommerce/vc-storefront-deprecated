using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Common;
using cartModel = VirtoCommerce.Storefront.AutoRestClients.CartModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CartPaymentConverter
    {
        public static Payment TowebModel(this cartModel.Payment payment, Currency currency)
        {
            var webModel = new Payment(currency);

            webModel.InjectFrom<NullableAndEnumValueInjecter>(payment);

            webModel.Amount = new Money(payment.Amount ?? 0, currency);

            if (payment.BillingAddress != null)
            {
                webModel.BillingAddress = payment.BillingAddress.ToWebModel();
            }

            webModel.Currency = currency;

            return webModel;
        }

        public static cartModel.Payment ToServiceModel(this Payment payment)
        {
            var retVal = new cartModel.Payment();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(payment);

            retVal.Amount = (double)payment.Amount.Amount;

            if (payment.BillingAddress != null)
            {
                retVal.BillingAddress = payment.BillingAddress.ToCartServiceModel();
            }

            retVal.Currency = payment.Currency.Code;

            return retVal;
        }

   
    }
}
