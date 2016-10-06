using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using coreDTO = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class SeoInfoConverter
    {
        public static SeoInfo ToSeoInfo(this coreDTO.SeoInfo seoDto)
        {
            var retVal = new SeoInfo();
            retVal.InjectFrom(seoDto);

            retVal.Slug = seoDto.SemanticUrl;
            retVal.Title = seoDto.PageTitle;
            retVal.Language = string.IsNullOrEmpty(seoDto.LanguageCode) ? Language.InvariantLanguage : new Language(seoDto.LanguageCode);
            return retVal;
        }

    }
}
