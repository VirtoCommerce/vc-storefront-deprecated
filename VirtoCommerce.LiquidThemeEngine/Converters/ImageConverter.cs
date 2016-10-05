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
            var result = AbstractTypeFactory<Image>.TryCreateInstance();
            result.InjectFrom<NullableAndEnumValueInjecter>(image);

            result.Name = image.Title;
            result.Src = image.Url;

            return result;
        }
    }
}
