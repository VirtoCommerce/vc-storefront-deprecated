using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using catalogModel = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;
using coreModel = VirtoCommerce.CoreModule.Client.Model;
using customerModel = VirtoCommerce.CustomerModule.Client.Model;
using storeModel = VirtoCommerce.Storefront.AutoRestClients.StoreModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class SeoInfoConverter
    {
        public static SeoInfo ToWebModel(this customerModel.SeoInfo seoDto)
        {
            return seoDto.JsonConvert<catalogModel.SeoInfo>().ToWebModel();
        }

        public static SeoInfo ToWebModel(this storeModel.SeoInfo seoDto)
        {
            return seoDto.JsonConvert<catalogModel.SeoInfo>().ToWebModel();
        }

        public static SeoInfo ToWebModel(this catalogModel.SeoInfo seoDto)
        {
            SeoInfo retVal = null;

            if (seoDto != null)
            {
                retVal = new SeoInfo();
                retVal.InjectFrom(seoDto);

                retVal.Slug = seoDto.SemanticUrl;
                retVal.Title = seoDto.PageTitle;
                retVal.Language = string.IsNullOrEmpty(seoDto.LanguageCode) ? Language.InvariantLanguage : new Language(seoDto.LanguageCode);
            }

            return retVal;
        }


        public static catalogModel.SeoInfo ToCatalogModel(this coreModel.SeoInfo seoDto)
        {
            catalogModel.SeoInfo retVal = null;

            if (seoDto != null)
            {
                retVal = new catalogModel.SeoInfo();
                retVal.InjectFrom(seoDto);
            }

            return retVal;
        }
    }
}
