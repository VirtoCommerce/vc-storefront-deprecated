using Omu.ValueInjecter;
using System.Linq;
using VirtoCommerce.LiquidThemeEngine.Objects;
using StorefrontModel = VirtoCommerce.Storefront.Model;
using Microsoft.Practices.ServiceLocation;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class LanguageConverter
    {
        public static Language ToShopifyModel(this StorefrontModel.Language language)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidLanguage(language);
        }
    }

    public partial class ShopifyModelConverter
    {
        public virtual Language ToLiquidLanguage(StorefrontModel.Language language)
        {
            var result = new Language();
            result.InjectFrom<StorefrontModel.Common.NullableAndEnumValueInjecter>(language);

            return result;
        }
    }
}