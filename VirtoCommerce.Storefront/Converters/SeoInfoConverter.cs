using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using coreDto = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class SeoInfoConverter
    {
        public static SeoInfo ToSeoInfo(this coreDto.SeoInfo seoDto)
        {
            var retVal = new SeoInfo();
            retVal.MetaDescription = seoDto.MetaDescription;
            retVal.MetaKeywords = seoDto.MetaKeywords;
            
            retVal.Slug = seoDto.SemanticUrl;
            retVal.Title = seoDto.PageTitle;
            retVal.Language = string.IsNullOrEmpty(seoDto.LanguageCode) ? Language.InvariantLanguage : new Language(seoDto.LanguageCode);
            return retVal;
        }

    }
}
