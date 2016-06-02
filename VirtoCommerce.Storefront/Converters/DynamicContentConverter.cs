using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Converters
{
    public static class DynamicContentConverter
    {
        public static DynamicContentItem ToWebModel(this MarketingModule.Client.Model.DynamicContentItem serviceModel)
        {
            var webModel = new DynamicContentItem();

            webModel.InjectFrom<NullableAndEnumValueInjecter>(serviceModel);

            if (serviceModel.DynamicProperties != null)
            {
                webModel.DynamicProperties = serviceModel.DynamicProperties.Select(dp => dp.ToWebModel()).ToList();
            }

            return webModel;
        }
    }
}
