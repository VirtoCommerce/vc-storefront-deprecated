using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Order;
using orderModel = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class ShipmentPackageConverter
    {
        public static ShipmentPackage ToWebModel(this orderModel.ShipmentPackage shipmentPackage, IEnumerable<Currency> currencies, Language language)
        {
            var webModel = new ShipmentPackage();

            webModel.InjectFrom<NullableAndEnumValueInjecter>(shipmentPackage);

            if (shipmentPackage.Items != null)
            {
                webModel.Items = shipmentPackage.Items.Select(i => i.ToWebModel(currencies, language)).ToList();
            }

            return webModel;
        }
    }
}
