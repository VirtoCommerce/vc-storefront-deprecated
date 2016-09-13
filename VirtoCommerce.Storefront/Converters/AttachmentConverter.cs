using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using quoteModel = VirtoCommerce.Storefront.AutoRestClients.QuoteModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class AttachmentConverter
    {
        public static Attachment ToWebModel(this quoteModel.QuoteAttachment serviceModel)
        {
            var webModel = new Attachment();
            webModel.InjectFrom<NullableAndEnumValueInjecter>(serviceModel);
            return webModel;
        }

        public static quoteModel.QuoteAttachment ToQuoteServiceModel(this Attachment webModel)
        {
            var serviceModel = new quoteModel.QuoteAttachment();
            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(webModel);
            return serviceModel;
        }
    }
}
