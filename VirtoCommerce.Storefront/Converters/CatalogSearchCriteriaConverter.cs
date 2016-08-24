using System.Linq;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using catalogModel = VirtoCommerce.CatalogModule.Client.Model;
using searchModel = VirtoCommerce.Storefront.AutoRestClients.SearchModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CatalogSearchCriteriaConverter
    {
        public static catalogModel.SearchCriteria ToCatalogApiModel(this CatalogSearchCriteria criteria, WorkContext workContext)
        {
            var result = new catalogModel.SearchCriteria
            {
                StoreId = workContext.CurrentStore.Id,
                Keyword = criteria.Keyword,
                ResponseGroup = criteria.ResponseGroup.ToString(),
                SearchInChildren = criteria.SearchInChildren,
                CategoryId = criteria.CategoryId,
                CatalogId = criteria.CatalogId,
                VendorId = criteria.VendorId,
                Currency = criteria.Currency == null ? workContext.CurrentCurrency.Code : criteria.Currency.Code,
                HideDirectLinkedCategories = true,
                Terms = criteria.Terms.ToStrings(),
                PricelistIds = workContext.CurrentPricelists.Where(p => p.Currency == workContext.CurrentCurrency.Code).Select(p => p.Id).ToList(),
                Skip = criteria.Start,
                Take = criteria.PageSize,
                Sort = criteria.SortBy
            };

            if (criteria.VendorIds != null)
            {
                result.VendorIds = criteria.VendorIds.ToList();
            }

            return result;
        }

        public static searchModel.SearchCriteria ToSearchApiModel(this CatalogSearchCriteria criteria, WorkContext workContext)
        {
            var result = new searchModel.SearchCriteria
            {
                StoreId = workContext.CurrentStore.Id,
                Keyword = criteria.Keyword,
                ResponseGroup = criteria.ResponseGroup.ToString(),
                SearchInChildren = criteria.SearchInChildren,
                CategoryId = criteria.CategoryId,
                CatalogId = criteria.CatalogId,
                VendorId = criteria.VendorId,
                Currency = criteria.Currency == null ? workContext.CurrentCurrency.Code : criteria.Currency.Code,
                HideDirectLinkedCategories = true,
                Terms = criteria.Terms.ToStrings(),
                PricelistIds = workContext.CurrentPricelists.Where(p => p.Currency == workContext.CurrentCurrency.Code).Select(p => p.Id).ToList(),
                Skip = criteria.Start,
                Take = criteria.PageSize,
                Sort = criteria.SortBy
            };

            if (criteria.VendorIds != null)
            {
                result.VendorIds = criteria.VendorIds.ToList();
            }

            return result;
        }
    }
}
