using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.StaticContent;
using VirtoCommerce.Storefront.Model.Stores;
using coreDto = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;

namespace VirtoCommerce.Storefront.Routing
{
    public class SeoRouteService : ISeoRouteService
    {
        private readonly ILocalCacheManager _cacheManager;
        private readonly Func<ICoreModuleApiClient> _coreApiFactory;
        private readonly Func<ICatalogModuleApiClient> _catalogApiFactory;

        public SeoRouteService(Func<ICoreModuleApiClient> coreApiFactory, Func<ICatalogModuleApiClient> catalogApiFactory, ILocalCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
            _coreApiFactory = coreApiFactory;
            _catalogApiFactory = catalogApiFactory;
        }

        public virtual SeoEntity FindEntityBySeoPath(string seoPath, WorkContext workContext)
        {
            seoPath = seoPath.Trim('/');

            var slugs = seoPath.Split('/');
            var lastSlug = slugs.LastOrDefault();

            // Get all SEO records for requested slug and also all other SEO records with different slug and languages but related to the same object
            var allSeoRecords = GetAllSeoRecords(lastSlug);
            var bestSeoRecords = allSeoRecords.GetBestMatchingSeoInfos(workContext.CurrentStore, workContext.CurrentLanguage, lastSlug);

            // Find distinct objects
            var entities = bestSeoRecords
                .Select(s => new SeoEntity { ObjectType = s.ObjectType, ObjectId = s.ObjectId, SeoPath = s.SemanticUrl })
                .ToList();

            foreach (var group in entities.GroupBy(e => e.ObjectType))
            {
                LoadObjectsAndBuildFullSeoPaths(group.Key, group.ToList(), workContext.CurrentStore, workContext.CurrentLanguage);
            }

            // If found only one entity, just return it
            // If found multiple entities, return the one with given SEO path
            var result = entities.FirstOrDefault(e => entities.Count == 1 || e.SeoPath.EqualsInvariant(seoPath));

            if (result == null)
            {
                // Try to find a static page
                var page = FindPageBySeoPath(seoPath, workContext);
                if (page != null)
                {
                    result = new SeoEntity
                    {
                        ObjectType = "Page",
                        SeoPath = page.Url,
                        ObjectInstance = page,
                    };
                }
            }

            return result;
        }


        protected virtual ContentItem FindPageBySeoPath(string seoPath, WorkContext workContext)
        {
            ContentItem result = null;

            if (workContext.Pages != null)
            {
                var pages = workContext.Pages
                    .Where(x =>
                            string.Equals(x.Permalink, seoPath, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(x.Url, seoPath, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Find page with current language
                result = pages.FirstOrDefault(x => x.Language == workContext.CurrentLanguage);

                if (result == null)
                {
                    // Find page with invariant language
                    result = pages.FirstOrDefault(x => x.Language.IsInvariant);
                }

                if (result == null)
                {
                    // Check alternate page URLs
                    result = workContext.Pages.FirstOrDefault(x => x.AliasesUrls.Contains(seoPath, StringComparer.OrdinalIgnoreCase));
                }
            }

            return result;
        }

        protected virtual IList<coreDto.SeoInfo> GetAllSeoRecords(string slug)
        {
            var result = new List<coreDto.SeoInfo>();

            if (!string.IsNullOrEmpty(slug))
            {
                var coreApi = _coreApiFactory();
                var apiResult = _cacheManager.Get(string.Join(":", "Commerce.GetSeoInfoBySlug", slug), "ApiRegion", () => coreApi.Commerce.GetSeoInfoBySlug(slug));
                result.AddRange(apiResult);
            }

            return result;
        }

        protected virtual void LoadObjectsAndBuildFullSeoPaths(string objectType, IList<SeoEntity> objects, Store store, Language language)
        {
            var objectIds = objects.Select(o => o.ObjectId).ToArray();
            var seoPaths = GetFullSeoPaths(objectType, objectIds, store, language);

            if (seoPaths != null)
            {
                foreach (var seo in objects)
                {
                    if (seoPaths.ContainsKey(seo.ObjectId))
                    {
                        seo.SeoPath = seoPaths[seo.ObjectId];
                    }
                }
            }
        }

        protected virtual Dictionary<string, string> GetFullSeoPaths(string objectType, string[] objectIds, Store store, Language language)
        {
            Dictionary<string, string> result = null;

            var cacheKeyItems = new List<string> { "GetFullSeoPaths", objectType };
            cacheKeyItems.AddRange(objectIds);
            var cacheKey = string.Join(":", cacheKeyItems);

            switch (objectType)
            {
                case "CatalogProduct":
                    result = _cacheManager.Get(cacheKey, "ApiRegion", () =>
                        _catalogApiFactory().CatalogModuleProducts
                            .GetProductByIds(objectIds, (ItemResponseGroup.Outlines | ItemResponseGroup.Seo).ToString())
                            .ToDictionary(x => x.Id, x => x.Outlines.GetSeoPath(store, language, null))
                    );
                    break;
                case "Category":
                    result = _cacheManager.Get(cacheKey, "ApiRegion", () =>
                        _catalogApiFactory().CatalogModuleCategories
                            .GetCategoriesByIds(objectIds, (CategoryResponseGroup.WithOutlines | CategoryResponseGroup.WithSeo).ToString())
                            .ToDictionary(x => x.Id, x => x.Outlines.GetSeoPath(store, language, null))
                    );
                    break;
            }

            return result;
        }
    }
}
