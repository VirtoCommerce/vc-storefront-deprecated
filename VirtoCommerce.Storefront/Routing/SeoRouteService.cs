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

        public virtual SeoRouteResponse HandleSeoRequest(string seoPath, WorkContext workContext)
        {
            var entity = FindEntityBySeoPath(seoPath, workContext) ?? new SeoEntity { ObjectType = "Asset", SeoPath = seoPath };
            var response = entity.SeoPath.EqualsInvariant(seoPath) ? View(entity) : Redirect(entity);
            return response;
        }


        protected virtual SeoRouteResponse View(SeoEntity entity)
        {
            var response = new SeoRouteResponse();

            switch (entity.ObjectType)
            {
                case "Category":
                    response.RouteData["controller"] = "CatalogSearch";
                    response.RouteData["action"] = "CategoryBrowsing";
                    response.RouteData["categoryId"] = entity.ObjectId;
                    break;
                case "CatalogProduct":
                    response.RouteData["controller"] = "Product";
                    response.RouteData["action"] = "ProductDetails";
                    response.RouteData["productId"] = entity.ObjectId;
                    break;
                case "Vendor":
                    response.RouteData["controller"] = "Vendor";
                    response.RouteData["action"] = "VendorDetails";
                    response.RouteData["vendorId"] = entity.ObjectId;
                    break;
                case "Page":
                    response.RouteData["controller"] = "Page";
                    response.RouteData["action"] = "GetContentPage";
                    response.RouteData["page"] = entity.ObjectInstance;
                    break;
                case "Asset":
                    response.RouteData["controller"] = "Asset";
                    response.RouteData["action"] = "HandleStaticFiles";
                    break;
            }

            return response;
        }

        protected virtual SeoRouteResponse Redirect(SeoEntity entity)
        {
            return new SeoRouteResponse
            {
                Redirect = true,
                RedirectLocation = entity.SeoPath,
            };
        }

        protected virtual SeoEntity FindEntityBySeoPath(string seoPath, WorkContext workContext)
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
                .Distinct(new SeoEntityComparer())
                .ToList();

            // Don't load objects for short SEO links
            if (workContext.CurrentStore.SeoLinksType != SeoLinksType.None && workContext.CurrentStore.SeoLinksType != SeoLinksType.Short)
            {
                foreach (var group in entities.GroupBy(e => e.ObjectType))
                {
                    LoadObjectsAndBuildFullSeoPaths(group.Key, group.ToList(), workContext.CurrentStore, workContext.CurrentLanguage);
                }

                entities = entities.Where(e => !string.IsNullOrEmpty(e.SeoPath)).ToList();
            }

            // If found multiple entities, keep those which have the requested SEO path
            if (entities.Count > 1)
            {
                entities = entities.Where(e => e.SeoPath.EqualsInvariant(seoPath)).ToList();
            }

            // If still found multiple entities, give up
            var result = entities.Count == 1 ? entities.FirstOrDefault() : null;

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
                    .Where(p => string.Equals(p.Url, seoPath, StringComparison.OrdinalIgnoreCase))
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

        protected virtual IDictionary<string, string> GetFullSeoPaths(string objectType, string[] objectIds, Store store, Language language)
        {
            IDictionary<string, string> result = null;

            var cacheKey = BuildCacheKey("GetFullSeoPaths", objectType, objectIds);

            switch (objectType)
            {
                case "Category":
                    result = _cacheManager.Get(cacheKey, "ApiRegion", () => GetCategorySeoPaths(objectIds, store, language));
                    break;
                case "CatalogProduct":
                    result = _cacheManager.Get(cacheKey, "ApiRegion", () => GetProductSeoPaths(objectIds, store, language));
                    break;
            }

            return result;
        }

        protected virtual string BuildCacheKey(string firstKeyItem, string secondKeyItem, params string[] otherKeyItems)
        {
            var cacheKeyItems = new List<string> { firstKeyItem, secondKeyItem };
            cacheKeyItems.AddRange(otherKeyItems.OrderBy(id => id));
            return string.Join(":", cacheKeyItems);
        }

        protected virtual IDictionary<string, string> GetCategorySeoPaths(string[] objectIds, Store store, Language language)
        {
            return _catalogApiFactory().CatalogModuleCategories
                .GetCategoriesByIds(objectIds, (CategoryResponseGroup.WithOutlines | CategoryResponseGroup.WithSeo).ToString())
                .ToDictionary(x => x.Id, x => x.Outlines.GetSeoPath(store, language, null));
        }

        protected virtual IDictionary<string, string> GetProductSeoPaths(string[] objectIds, Store store, Language language)
        {
            return _catalogApiFactory().CatalogModuleProducts
                .GetProductByIds(objectIds, (ItemResponseGroup.Outlines | ItemResponseGroup.Seo).ToString())
                .ToDictionary(x => x.Id, x => x.Outlines.GetSeoPath(store, language, null));
        }

        private class SeoEntityComparer : IEqualityComparer<SeoEntity>
        {
            public bool Equals(SeoEntity x, SeoEntity y)
            {
                if (x == null)
                {
                    return y == null;
                }

                if (y == null)
                {
                    return false;
                }

                return x.ObjectType == y.ObjectType && x.ObjectId == y.ObjectId && x.SeoPath == y.SeoPath;
            }

            public int GetHashCode(SeoEntity obj)
            {
                // credit: http://stackoverflow.com/a/263416/677735
                unchecked // Overflow is fine, just wrap
                {
                    var hash = base.GetHashCode();

                    if (obj.ObjectType != null)
                        hash = hash * 59 + obj.ObjectType.GetHashCode();

                    if (obj.ObjectId != null)
                        hash = hash * 59 + obj.ObjectId.GetHashCode();

                    if (obj.SeoPath != null)
                        hash = hash * 59 + obj.SeoPath.GetHashCode();

                    return hash;
                }
            }
        }
    }
}
