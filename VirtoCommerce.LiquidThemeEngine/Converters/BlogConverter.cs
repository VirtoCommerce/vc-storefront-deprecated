using System.Linq;
using Omu.ValueInjecter;
using PagedList;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.Storefront.Model.Common;
using StorefrontModel = VirtoCommerce.Storefront.Model.StaticContent;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class BlogConverter
    {
        public static Blog ToShopifyModel(this StorefrontModel.Blog blog, Storefront.Model.Language language)
        {
            var retVal = new Blog();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(blog);
            retVal.Handle = blog.Name;

            if (blog.Articles != null)
            {
                retVal.Articles = new MutablePagedList<Article>((pageNumber, pageSize, sortInfos) =>
                {
                    //var articlesForLanguage = blog.Articles.Where(x => x.Language == language || x.Language.IsInvariant).GroupBy(x => x.Name).Select(x => x.OrderByDescending(y => y.Language).FirstOrDefault());
                    // ordering generating exception
                    var articlesForLanguage = blog.Articles.Where(x => x.Language == language || x.Language.IsInvariant).GroupBy(x => x.Name).Select(x => x.FirstOrDefault());
                    return new PagedList<Article>(articlesForLanguage.Select(x => x.ToShopifyModel()).OrderByDescending(x => x.CreatedAt), pageNumber, pageSize);
                }, blog.Articles.PageNumber, blog.Articles.PageSize);
            }

            retVal.Handle = blog.Name.Replace(" ", "-").ToLower();

            retVal.Categories = blog.Categories;
            if (blog.StickedArticle != null)
            {
                retVal.StickedArticle = blog.StickedArticle.ToShopifyModel();
            }
            if (blog.TrendingArticles != null)
            {
                retVal.TrendingArticles = new MutablePagedList<Article>((pageNumber, pageSize, sortInfos) =>
                {
                    var articlesForLanguage = blog.TrendingArticles.Where(x => x.Language == language || x.Language.IsInvariant).GroupBy(x => x.Name).Select(x => x.FirstOrDefault());
                    return new PagedList<Article>(articlesForLanguage.Select(x => x.ToShopifyModel()).OrderByDescending(x => x.CreatedAt), pageNumber, pageSize);
                });
            }

            return retVal;
        }
    }
}