using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Controllers
{
    [OutputCache(CacheProfile = "StaticContentCachingProfile")]
    public class BlogController : StorefrontControllerBase
    {
        public BlogController(WorkContext context, IStorefrontUrlBuilder urlBuilder)
            : base(context, urlBuilder)
        {
        }

        // GET: /blogs/{blog}
        public ActionResult GetBlog(string blog)
        {
            WorkContext.CurrentBlog = WorkContext.Blogs.SingleOrDefault(x => x.Name.Equals(blog, StringComparison.OrdinalIgnoreCase));

            return View("blog", WorkContext);
        }

        // GET: /blogs/{blog}/{article}
        public ActionResult GetBlogArticle(string blog, string article)
        {
            var context = WorkContext;
            var articleUrl = string.Join("/", "blogs", blog, article);

            context.CurrentBlog = context.Blogs.SingleOrDefault(x => x.Name.Equals(blog, StringComparison.OrdinalIgnoreCase));
            if (context.CurrentBlog != null)
            {
                var blogArticles = context.CurrentBlog.Articles.Where(x => x.Url.Equals(articleUrl)).ToList();

                // Return article with current or invariant language
                var blogArticle = blogArticles.FirstOrDefault(x => x.Language == context.CurrentLanguage);

                if (blogArticle == null)
                {
                    blogArticle = blogArticles.FirstOrDefault(x => x.Language.IsInvariant);
                }

                if (blogArticle != null)
                {
                    context.CurrentBlogArticle = blogArticle;

                    context.CurrentPageSeo = new SeoInfo
                    {
                        Language = blogArticle.Language,
                        MetaDescription = blogArticle.Excerpt,
                        Title = blogArticle.Title,
                        Slug = blogArticle.Permalink
                    };

                    return View("article", WorkContext);
                }
            }

            throw new HttpException(404, articleUrl);
        }
    }
}
