using System.Linq;
using Microsoft.Practices.ServiceLocation;
using PagedList;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using storefrontModel = VirtoCommerce.Storefront.Model.Catalog;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class CollectionStaticConverter
    {
        public static Collection ToShopifyModel(this storefrontModel.Category category, WorkContext workContext)
        {
            var converter = ServiceLocator.Current.GetInstance<CollectionConverter>();
            return converter.ToShopifyModel(category, workContext);
        }
    }

    public class CollectionConverter
    {

        public virtual Collection ToShopifyModel(storefrontModel.Category category, WorkContext workContext)
        {
            var result = ServiceLocator.Current.GetInstance<Collection>();

            result.Id = category.Id;
            result.Description = null;
            result.Handle = category.SeoInfo != null ? category.SeoInfo.Slug : category.Id;
            result.Title = category.Name;
            result.Url = category.Url;
            result.DefaultSortBy = "manual";

            if (category.PrimaryImage != null)
            {
                result.Image = category.PrimaryImage.ToShopifyModel();
            }

            if (category.Products != null)
            {
                result.Products = new MutablePagedList<Product>((pageNumber, pageSize, sortInfos) =>
                {
                    category.Products.Slice(pageNumber, pageSize, sortInfos);
                    return new StaticPagedList<Product>(category.Products.Select(x => x.ToShopifyModel()), category.Products);
                }, category.Products.PageNumber, category.Products.PageSize);
            }

            if (workContext.Aggregations != null)
            {
                result.Tags = new TagCollection(new MutablePagedList<Tag>((pageNumber, pageSize, sortInfos) =>
                {
                    workContext.Aggregations.Slice(pageNumber, pageSize, sortInfos);
                    var tags = workContext.Aggregations.Where(a => a.Items != null)
                                           .SelectMany(a => a.Items.Select(item => item.ToShopifyModel(a.Field, a.Label)));
                    return new StaticPagedList<Tag>(tags, workContext.Aggregations);

                }, workContext.Aggregations.PageNumber, workContext.Aggregations.PageSize));
            }

            if (workContext.CurrentProductSearchCriteria.SortBy != null)
            {
                result.SortBy = workContext.CurrentProductSearchCriteria.SortBy;
            }

            if (!category.Properties.IsNullOrEmpty())
            {
                result.Metafields = new MetaFieldNamespacesCollection(new[] { new MetafieldsCollection("properties", category.Properties) });
            }

            return result;
        }
    }
}
