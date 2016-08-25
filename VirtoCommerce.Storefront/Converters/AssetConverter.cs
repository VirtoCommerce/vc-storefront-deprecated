using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using catalogModel = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class AssetConverter
    {
        public static Image ToWebModel(this catalogModel.Image image)
        {
            var retVal = new Image();
            retVal.InjectFrom(image);
            return retVal;
        }

        public static Asset ToWebModel(this catalogModel.Asset asset)
        {
            var retVal = new Asset();
            retVal.InjectFrom(asset);
            return retVal;
        }
    }
}
