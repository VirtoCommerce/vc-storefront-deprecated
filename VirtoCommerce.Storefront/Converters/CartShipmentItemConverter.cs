using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CartShipmentItemConverter
    {
        public static CartShipmentItem ToWebModel(this CartModule.Client.Model.ShipmentItem serviceModel, ShoppingCart cart)
        {
            var webModel = new CartShipmentItem();

            webModel.InjectFrom<NullableAndEnumValueInjecter>(serviceModel);

            webModel.LineItem = cart.Items.FirstOrDefault(x => x.Id == serviceModel.LineItemId);

            return webModel;
        }

        public static CartModule.Client.Model.ShipmentItem ToServiceModel(this CartShipmentItem webModel)
        {
            var result = new CartModule.Client.Model.ShipmentItem();

            result.InjectFrom<NullableAndEnumValueInjecter>(webModel);

            result.LineItem = webModel.LineItem.ToServiceModel();

            return result;
        }
    }
}
