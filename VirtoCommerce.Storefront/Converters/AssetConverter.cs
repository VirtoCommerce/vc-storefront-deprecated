using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using catalogModel = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class AssetStaticConverter
    {
        public static Image ToWebModel(this catalogModel.Image image)
        {
            var converter = AbstractTypeFactory<AssetConverter>.TryCreateInstance();
            return converter.ToWebModel(image);
        }

        public static Asset ToWebModel(this catalogModel.Asset asset)
        {
            var converter = AbstractTypeFactory<AssetConverter>.TryCreateInstance();
            return converter.ToWebModel(asset);
        }
    }

    public class AssetConverter
    {
        public virtual Image ToWebModel(catalogModel.Image image)
        {
            var result = AbstractTypeFactory<Image>.TryCreateInstance();
            result.InjectFrom<NullableAndEnumValueInjecter>(image);
            return result;
        }

        public virtual Asset ToWebModel(catalogModel.Asset asset)
        {
            var result = AbstractTypeFactory<Asset>.TryCreateInstance();
            result.InjectFrom<NullableAndEnumValueInjecter>(asset);
            return result;
        }
    }
}
