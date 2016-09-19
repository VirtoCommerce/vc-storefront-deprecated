using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model.StaticContent
{
    public class Blog : ContentItem
    {
        public Blog()
        {
            Permalink = ":folder";
        }

        public IMutablePagedList<BlogArticle> Articles { get; set; }

        public BlogArticle StickedArticle { get; set; }

        public IMutablePagedList<BlogArticle> TrendingArticles { get; set; }
    }
}