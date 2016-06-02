using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using catalogModel = VirtoCommerce.CatalogModule.Client.Model;
using coreModel = VirtoCommerce.CoreModule.Client.Model;
using storeModel = VirtoCommerce.StoreModule.Client.Model;

namespace VirtoCommerce.Storefront.Converters
{
    public static class SeoInfoConverter
    {
        public static SeoInfo ToWebModel(this catalogModel.VirtoCommerceDomainCommerceModelSeoInfo seoDto)
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

        public static SeoInfo ToWebModel(this storeModel.SeoInfo seoDto)
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

        public static catalogModel.VirtoCommerceDomainCommerceModelSeoInfo ToCatalogModel(this coreModel.VirtoCommerceDomainCommerceModelSeoInfo seoDto)
        {
            catalogModel.VirtoCommerceDomainCommerceModelSeoInfo retVal = null;

            if (seoDto != null)
            {
                retVal = new catalogModel.VirtoCommerceDomainCommerceModelSeoInfo();
                retVal.InjectFrom(seoDto);
            }

            return retVal;
        }
    }
}
