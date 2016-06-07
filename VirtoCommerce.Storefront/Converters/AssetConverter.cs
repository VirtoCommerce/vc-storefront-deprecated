using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using catalogModel = VirtoCommerce.CatalogModule.Client.Model;

namespace VirtoCommerce.Storefront.Converters
{
    public static class AssetConverter
    {
        public static Image ToWebModel(this CatalogModule.Client.Model.Image image)
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
