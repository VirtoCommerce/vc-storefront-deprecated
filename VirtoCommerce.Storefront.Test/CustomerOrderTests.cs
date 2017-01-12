using VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Order;
using VirtoCommerce.Storefront.Converters;
using Xunit;

namespace VirtoCommerce.Storefront.Test
{
    public class CustomerOrderTests : StorefrontTestBase
    {
        [Fact]
        public void CreateNewPayment_CheckThatNewPaymentStored()
        {
            var existingOrderNumber = "CU1508131823004";
            var orderApi = GetOrderApiClient();
            var orderDto = orderApi.OrderModule.GetByNumber(existingOrderNumber);
            
            var currency = new Currency(new Language("en-US"), "USD");
            var payment = new PaymentIn(currency)
            {
                GatewayCode = "DefaultManualPaymentMethod",
                Sum = new Money(1219.19, currency),
                CustomerId = GetTestWorkContext().CurrentCustomer.Id,
                Purpose = "test payment"
            };
            
            var initialCount = orderDto.InPayments.Count;
            orderDto.InPayments.Add(payment.ToOrderPaymentInDto());
            orderApi.OrderModule.Update(orderDto);
            Assert.Equal(initialCount + 1, orderDto.InPayments.Count);
        }
    }
}
