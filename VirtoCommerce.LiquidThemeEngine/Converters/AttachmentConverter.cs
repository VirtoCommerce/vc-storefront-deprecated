using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.LiquidThemeEngine.Objects;
using Microsoft.Practices.ServiceLocation;
using VirtoCommerce.LiquidThemeEngine.Objects.Factories;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class AttachmentConverter
    {
        public static Attachment ToShopifyModel(this VirtoCommerce.Storefront.Model.Attachment attachment)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidAttachment(attachment);
        }

    }

    public partial class ShopifyModelConverter
    {
        public virtual Attachment ToLiquidAttachment(VirtoCommerce.Storefront.Model.Attachment attachment)
        {
            var factory = ServiceLocator.Current.GetInstance<ShopifyModelFactory>();
            var retVal = factory.CreateAttachment();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(attachment);

            return retVal;
        }
    }
}