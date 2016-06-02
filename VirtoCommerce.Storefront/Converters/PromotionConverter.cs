using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;

namespace VirtoCommerce.Storefront.Converters
{
    public static class PromotionConverter
    {
        public static Promotion ToWebModel(this MarketingModule.Client.Model.Promotion serviceModel)
        {
            var webModel = new Promotion();

            webModel.InjectFrom<NullableAndEnumValueInjecter>(serviceModel);

            webModel.Coupons = serviceModel.Coupons;

            return webModel;
        }
    }
}
