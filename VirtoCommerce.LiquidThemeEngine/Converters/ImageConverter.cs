using Omu.ValueInjecter;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.Storefront.Model.Common;
using storefrontModel = VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class ImageStaticConverter
    {
        public static Image ToShopifyModel(this storefrontModel.Image image)
        {
            var converter = AbstractTypeFactory<ImageConverter>.TryCreateInstance();
            return converter.ToShopifyModel(image);
        }
    }

    public class ImageConverter
    {
        public virtual Image ToShopifyModel(storefrontModel.Image image)
        {
            var shopifyModel = new Image();
            shopifyModel.InjectFrom<NullableAndEnumValueInjecter>(image);

            shopifyModel.Name = image.Title;
            shopifyModel.Src = image.Url;

            return shopifyModel;
        }
    }
}
