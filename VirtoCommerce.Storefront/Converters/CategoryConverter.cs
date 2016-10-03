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
    public class CategoryConverter
    {
        protected Func<Category> CategoryFactory { get; set; }

        public CategoryConverter(Func<Category> categoryFactory)
        {
            CategoryFactory = categoryFactory;
        }

        public virtual Category ToWebModel(searchModel.Category category, Language currentLanguage, Store store)
        {
            return ToWebModel(category.JsonConvert<catalogModel.Category>(), currentLanguage, store);
        }

        public virtual Category ToWebModel(catalogModel.Category category, Language currentLanguage, Store store)
        {
            return ToWebModel(category, CategoryFactory(), currentLanguage, store);
        }

        public virtual T ToWebModel<T>(catalogModel.Category category, T result, Language currentLanguage, Store store)
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
