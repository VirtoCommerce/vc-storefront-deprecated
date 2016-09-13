using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using quoteModel = VirtoCommerce.Storefront.AutoRestClients.QuoteModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class ShippingMethodConverter
    {
        public static ShippingMethod ToWebModel(this quoteModel.ShipmentMethod serviceModel, Currency currency)
        {
            var webModel = new ShippingMethod();
            webModel.InjectFrom<NullableAndEnumValueInjecter>(serviceModel);
            webModel.Price = new Money(serviceModel.Price ?? 0, currency);
            return webModel;
        }
    }
}
