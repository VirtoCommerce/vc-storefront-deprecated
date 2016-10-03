using System;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Stores;
using catalogModel = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;
using searchModel = VirtoCommerce.Storefront.AutoRestClients.SearchApiModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CategoryStaticConverter
    {
        public static Category ToWebModel(this searchModel.Category category, Language currentLanguage, Store store)
        {
            var converter = AbstractTypeFactory<CategoryConverter>.TryCreateInstance();
            return converter.ToWebModel(category, currentLanguage, store);
        }

        public static Category ToWebModel(this catalogModel.Category category, Language currentLanguage, Store store)
        {
            var converter = AbstractTypeFactory<CategoryConverter>.TryCreateInstance();
            return converter.ToWebModel(category, currentLanguage, store);
        }
    }

    public class CategoryConverter
    {
        public virtual Category ToWebModel(searchModel.Category category, Language currentLanguage, Store store)
        {
            return ToWebModel(category.JsonConvert<catalogModel.Category>(), currentLanguage, store);
        }

        public virtual Category ToWebModel(catalogModel.Category category, Language currentLanguage, Store store)
        {
            var result = AbstractTypeFactory<Category>.TryCreateInstance();
            result.InjectFrom<NullableAndEnumValueInjecter>(category);

            result.SeoInfo = category.SeoInfos.GetBestMatchedSeoInfo(store, currentLanguage).ToWebModel();
            if (result.SeoInfo == null)
            {
                result.SeoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
                result.SeoInfo.Slug = category.Id;
            }

            result.Url = "~/" + category.Outlines.GetSeoPath(store, currentLanguage, "category/" + category.Id);
            result.Outline = category.Outlines.GetOutlinePath(store.Catalog);

            if (category.Images != null)
            {
                result.Images = category.Images.Select(i => i.ToWebModel()).ToArray();
                result.PrimaryImage = result.Images.FirstOrDefault();
            }

            if (category.Properties != null)
            {
                result.Properties = category.Properties
                    .Where(x => string.Equals(x.Type, "Category", StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.ToWebModel(currentLanguage))
                    .ToList();
            }

            return result;
        }
    }
}
