using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Converters
{
    public static class AttachmentConverter
    {
        public static Attachment ToWebModel(this QuoteModule.Client.Model.QuoteAttachment serviceModel)
        {
            var webModel = new Attachment();
            webModel.InjectFrom<NullableAndEnumValueInjecter>(serviceModel);
            return webModel;
        }

        public static QuoteModule.Client.Model.QuoteAttachment ToQuoteServiceModel(this Attachment webModel)
        {
            var serviceModel = new QuoteModule.Client.Model.QuoteAttachment();
            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(webModel);
            return serviceModel;
        }
    }
}
