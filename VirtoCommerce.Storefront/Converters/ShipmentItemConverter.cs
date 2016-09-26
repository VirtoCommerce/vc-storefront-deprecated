using System.Collections.Generic;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Order;
using orderModel = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class ShipmentItemConverter
    {
        public static ShipmentItem ToWebModel(this orderModel.ShipmentItem shipmentItem, IEnumerable<Currency> availCurrencies, Language language)
        {
            var webModel = new ShipmentItem();

            webModel.InjectFrom<NullableAndEnumValueInjecter>(shipmentItem);

            if (shipmentItem.LineItem != null)
            {
                webModel.LineItem = shipmentItem.LineItem.ToWebModel(availCurrencies, language);
            }

            return webModel;
        }
    }
}
