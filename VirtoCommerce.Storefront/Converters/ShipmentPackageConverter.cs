using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Order;

namespace VirtoCommerce.Storefront.Converters
{
    public static class ShipmentPackageConverter
    {
        public static ShipmentPackage ToWebModel(this OrderModule.Client.Model.ShipmentPackage shipmentPackage, IEnumerable<Currency> currencies, Language language)
        {
            var webModel = new ShipmentPackage();

            webModel.InjectFrom(shipmentPackage);

            if (shipmentPackage.Items != null)
            {
                webModel.Items = shipmentPackage.Items.Select(i => i.ToWebModel(currencies, language)).ToList();
            }

            return webModel;
        }
    }
}
