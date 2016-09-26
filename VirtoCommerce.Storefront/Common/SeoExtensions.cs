using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Stores;
using VirtoCommerce.Storefront.Model.Catalog;
using catalogModel = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;
using customerModel = VirtoCommerce.Storefront.AutoRestClients.CustomerModuleApi.Models;

namespace VirtoCommerce.Storefront.Common
{
    /// <summary>
    /// Find best seo match based for passed store and language
    /// </summary>
    public static class SeoExtensions
    {
        /// <summary>
        /// Returns best matched outline path CategoryId/CategoryId2.
        /// </summary>
        /// <param name="outlines"></param>
        /// <param name="store"></param>
        /// <param name="language"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetOutlinePath(this IEnumerable<catalogModel.Outline> outlines)
        {
            var result = string.Empty;
            if (outlines != null)
            {
                var outline = outlines.FirstOrDefault();

                if (outline != null)
                {
                    var pathSegments = new List<string>();

                    pathSegments.AddRange(outline.Items
                        .Where(i => i.SeoObjectType != "Catalog")
                        .Select(i => i.Id));

                    if (pathSegments.All(s => s != null))
                    {
                        result = string.Join("/", pathSegments);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns product category.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public static string GetCategoryOutline(this Product product)
        {
            var result = string.Empty;
            if (product != null && !string.IsNullOrEmpty(product.Outline))
            {
                var outlineArray = product.Outline.Split(new[] { '/' });

                if (outlineArray == null || outlineArray.Length == 0)
                    return string.Empty;

                var pathSegments = outlineArray.Reverse().Skip(1).Reverse();
                if (pathSegments.All(s => s != null))
                {
                    result = string.Join("/", pathSegments);
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
        public static string GetSeoPath(this IEnumerable<catalogModel.Outline> outlines, Store store, Language language, string defaultValue)
        {
            var result = defaultValue;

            if (outlines != null && store.SeoLinksType != SeoLinksType.None)
            {
                var outline = outlines.FirstOrDefault();

                if (outline != null)
                {
                    var pathSegments = new List<string>();

                    if (store.SeoLinksType == SeoLinksType.Long)
                    {
                        pathSegments.AddRange(outline.Items
                            .Where(i => i.SeoObjectType != "Catalog")
                            .Select(i => GetBestMatchedSeoKeyword(i.SeoInfos, store, language)));
                    }
                    else if (store.SeoLinksType == SeoLinksType.Collapsed)
                    {
                        pathSegments.AddRange(outline.Items
                            .Where(i => i.SeoObjectType != "Catalog" && i.HasVirtualParent != true)
                            .Select(i => GetBestMatchedSeoKeyword(i.SeoInfos, store, language)));
                    }
                    else
                    {
                        var lastItem = outline.Items.LastOrDefault();
                        if (lastItem != null)
                        {
                            pathSegments.Add(GetBestMatchedSeoKeyword(lastItem.SeoInfos, store, language));
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
        /// Duplicate method special for Customer.SeoInfo type
        /// </summary>
        public static customerModel.SeoInfo GetBestMatchedSeoInfo(this IEnumerable<customerModel.SeoInfo> seoRecords, Store store, Language language, string slug = null)
        {
            customerModel.SeoInfo retVal = null;
            if (!seoRecords.IsNullOrEmpty())
            {
                var catalogSeoInfos = seoRecords.Select(x => x.JsonConvert<catalogModel.SeoInfo>());
                var catalogSeoInfo = GetBestMatchedSeoInfo(catalogSeoInfos, store, language, slug);
                if (catalogSeoInfo != null)
                {
                    retVal = catalogSeoInfo.JsonConvert<customerModel.SeoInfo>();
                }
            }
            return retVal;
        }

        /// <summary>
        /// Find best SEO record using score base rules
        /// http://docs.virtocommerce.com/display/vc2devguide/SEO
        /// </summary>
        /// <param name="seoRecords"></param>
        /// <param name="store"></param> 
        /// <param name="language"></param>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static catalogModel.SeoInfo GetBestMatchedSeoInfo(this IEnumerable<catalogModel.SeoInfo> seoRecords, Store store, Language language, string slug = null)
        {
            catalogModel.SeoInfo result = null;

            if (seoRecords != null)
            {
                result = seoRecords
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
                    .Select(x => x.SeoRecord)
                    .FirstOrDefault();
            }

            return result;
        }


        private static string GetBestMatchedSeoKeyword(IEnumerable<catalogModel.SeoInfo> seoRecords, Store store, Language language)
        {
            string result = null;

            if (seoRecords != null)
            {
                // Select best matched SEO by StoreId and Language
                var bestMatchedSeo = seoRecords.GetBestMatchedSeoInfo(store, language);
                if (bestMatchedSeo != null)
                {
                    result = bestMatchedSeo.SemanticUrl;
                }
            }

            return result;
        }
    }
}
