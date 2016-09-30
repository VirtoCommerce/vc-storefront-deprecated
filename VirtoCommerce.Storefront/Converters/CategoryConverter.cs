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
    public static class CategoryConverter
    {
        public static Category ToWebModel(this searchModel.Category product, Language currentLanguage, Store store)
        {
            return product.JsonConvert<catalogModel.Category>().ToWebModel(currentLanguage, store);
        }

        public static Category ToWebModel(this catalogModel.Category category, Language currentLanguage, Store store)
        {
            return ToWebModel(category, new Category(), currentLanguage, store);
        }

        public static T ToWebModel<T>(this catalogModel.Category category, T result, Language currentLanguage, Store store)
            where T : Category
        {
            result.InjectFrom<NullableAndEnumValueInjecter>(category);

            result.SeoInfo = category.SeoInfos.GetBestMatchedSeoInfo(store, currentLanguage).ToWebModel();
            if (result.SeoInfo == null)
            {
                result.SeoInfo = new SeoInfo { Slug = category.Id };
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
