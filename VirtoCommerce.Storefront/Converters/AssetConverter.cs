using Omu.ValueInjecter;
using VirtoCommerce.CatalogModule.Client.Model;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;

namespace VirtoCommerce.Storefront.Converters
{
    public static class AssetConverter
    {
        public static Image ToWebModel(this VirtoCommerceCatalogModuleWebModelImage image)
        {
            var retVal = new Image();
            retVal.InjectFrom(image);
            return retVal;
        }

        public static Asset ToWebModel(this VirtoCommerceCatalogModuleWebModelAsset asset)
        {
            var retVal = new Asset();
            retVal.InjectFrom(asset);
            return retVal;
        }
    }
}
