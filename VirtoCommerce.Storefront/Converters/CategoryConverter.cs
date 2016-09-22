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

        public static Category ToWebModel(this catalogModel.Category category, Language currentLanguage, Store store, catalogModel.Product[] products = null)
        {
            var retVal = new Category();
            retVal.InjectFrom<NullableAndEnumValueInjecter>(category);

            retVal.SeoInfo = category.SeoInfos.GetBestMatchedSeoInfo(store, currentLanguage).ToWebModel();
            if(retVal.SeoInfo == null)
            {
                retVal.SeoInfo = new SeoInfo { Slug = category.Id };
            }
            retVal.Url = "~/" + category.Outlines.GetSeoPath(store, currentLanguage, "category/" + category.Id);
            retVal.Outline = category.Outlines.GetOutlinePath();

            if (category.Images != null)
            {
                retVal.Images = category.Images.Select(i => i.ToWebModel()).ToArray();
                retVal.PrimaryImage = retVal.Images.FirstOrDefault();
            }

            if (category.Properties != null)
            {
                retVal.Properties = category.Properties
                    .Where(x => string.Equals(x.Type, "Category", StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.ToWebModel(currentLanguage))
                    .ToList();
            }

            return retVal;
        }
    }
}
