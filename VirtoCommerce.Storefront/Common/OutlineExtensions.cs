using System.Collections.Generic;
using System.Linq;
using catalogModel = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;

namespace VirtoCommerce.Storefront.Common
{
    public static class OutlineExtensions
    {
        /// <summary>
        /// Returns best matched outline path CategoryId/CategoryId2.
        /// </summary>
        /// <param name="outlines"></param>
        /// <param name="catalogId"></param>
        /// <returns></returns>
        public static string GetOutlinePath(this IEnumerable<catalogModel.Outline> outlines, string catalogId)
        {
            var result = string.Empty;

            if (outlines != null)
            {
                // Find any outline for given catalog
                var outline = outlines.GetOutlineForCatalog(catalogId);

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
        /// Returns first outline for given catalog (if any)
        /// </summary>
        /// <param name="outlines"></param>
        /// <param name="catalogId"></param>
        /// <returns></returns>
        public static catalogModel.Outline GetOutlineForCatalog(this IEnumerable<catalogModel.Outline> outlines, string catalogId)
        {
            catalogModel.Outline result = null;

            if (outlines != null)
            {
                // Find any outline for given catalog
                result = outlines.FirstOrDefault(o => o.Items.Any(i => i.SeoObjectType == "Catalog" && i.Id == catalogId));
            }

            return result;
        }
    }
}
