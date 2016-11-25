using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Stores;
using catalogDto = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;
using coreDto = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;

namespace VirtoCommerce.Storefront.Common
{
    public static class SeoExtensions
    {
        /// <summary>
        /// Returns product's category outline.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public static string GetCategoryOutline(this Product product)
        {
            var result = string.Empty;

            if (product != null && !string.IsNullOrEmpty(product.Outline))
            {
                var i = product.Outline.LastIndexOf('/');
                if (i >= 0)
                {
                    result = product.Outline.Substring(0, i);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns SEO path if all outline items of the first outline have SEO keywords, otherwise returns default value.
        /// Path: GrandParentCategory/ParentCategory/ProductCategory/Product
        /// </summary>
        /// <param name="outlines"></param>
        /// <param name="store"></param>
        /// <param name="language"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetSeoPath(this IEnumerable<catalogDto.Outline> outlines, Store store, Language language, string defaultValue)
        {
            var result = defaultValue;

            if (outlines != null && store.SeoLinksType != SeoLinksType.None)
            {
                // Find any outline for store catalog
                var outline = outlines.GetOutlineForCatalog(store.Catalog);

                if (outline != null)
                {
                    var pathSegments = new List<string>();

                    if (store.SeoLinksType == SeoLinksType.Long)
                    {
                        pathSegments.AddRange(outline.Items
                            .Where(i => i.SeoObjectType != "Catalog")
                            .Select(i => GetBestMatchingSeoKeyword(i.SeoInfos, store, language)));
                    }
                    else if (store.SeoLinksType == SeoLinksType.Collapsed)
                    {
                        var lastItem = outline.Items.Last();
                        if (lastItem.SeoObjectType != "Category" || lastItem.HasVirtualParent != true)
                        {
                            pathSegments.AddRange(outline.Items
                                .Where(i => i.SeoObjectType != "Catalog" && (lastItem.SeoObjectType != "Category" || i.HasVirtualParent != true))
                                .Select(i => GetBestMatchingSeoKeyword(i.SeoInfos, store, language)));
                        }
                    }
                    else
                    {
                        var lastItem = outline.Items.LastOrDefault();
                        if (lastItem != null)
                        {
                            pathSegments.Add(GetBestMatchingSeoKeyword(lastItem.SeoInfos, store, language));
                        }
                    }

                    if (pathSegments.All(s => s != null))
                    {
                        result = string.Join("/", pathSegments);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns SEO records with highest score
        /// http://docs.virtocommerce.com/display/vc2devguide/SEO
        /// </summary>
        /// <param name="seoRecords"></param>
        /// <param name="store"></param>
        /// <param name="language"></param>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static IList<coreDto.SeoInfo> GetBestMatchingSeoInfos(this IEnumerable<coreDto.SeoInfo> seoRecords, Store store, Language language, string slug = null)
        {
            var result = new List<coreDto.SeoInfo>();

            if (seoRecords != null)
            {
                var items = seoRecords
                    .Select(s =>
                    {
                        var score = 0;
                        score += s.IsActive != false ? 16 : 0;
                        if (!string.IsNullOrEmpty(slug))
                        {
                            score += slug.EqualsInvariant(s.SemanticUrl) ? 8 : 0;
                        }
                        score += store.Id.EqualsInvariant(s.StoreId) ? 4 : 0;
                        score += language.Equals(s.LanguageCode) ? 2 : 0;
                        score += store.DefaultLanguage.Equals(s.LanguageCode) ? 1 : 0;
                        return new { SeoRecord = s, Score = score };
                    })
                    .OrderByDescending(x => x.Score)
                    .ToList();

                var first = items.FirstOrDefault();
                if (first != null)
                {
                    result.AddRange(items.Where(i => i.Score == first.Score).Select(i => i.SeoRecord));
                }
            }

            return result;
        }

        private static string GetBestMatchingSeoKeyword(IEnumerable<catalogDto.SeoInfo> seoRecords, Store store, Language language)
        {
            string result = null;

            if (seoRecords != null)
            {
                // Select best matching SEO by StoreId and Language
                var bestMatchedSeo = seoRecords.Select(x => x.JsonConvert<coreDto.SeoInfo>())
                    .GetBestMatchingSeoInfos(store, language)
                    .FirstOrDefault();

                if (bestMatchedSeo != null)
                {
                    result = bestMatchedSeo.SemanticUrl;
                }
            }

            return result;
        }
    }
}
