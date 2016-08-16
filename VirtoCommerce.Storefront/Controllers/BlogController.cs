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
            var context = WorkContext;
            context.CurrentBlog = WorkContext.Blogs.SingleOrDefault(x => x.Name.Equals(blog, StringComparison.OrdinalIgnoreCase));

            if (context.CurrentBlog != null)
            {
                context.CurrentPageSeo = new SeoInfo
                {
                    Language = context.CurrentBlog.Language,
                    MetaDescription = context.CurrentBlog.Name,
                    Title = context.CurrentBlog.Name,
                    Slug = string.Format("/blogs/{0}", blog)
                };
            }
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
                        Slug = blogArticle.Url
                    };

                    return View("article", WorkContext);
                }
                else
                {
                    blogArticle = context.CurrentBlog.Articles.FirstOrDefault(x => x.AliasesUrls.Contains(articleUrl, StringComparer.OrdinalIgnoreCase));
                    if(blogArticle != null)
                    {
                        var articleRedirectUrl = UrlBuilder.ToAppAbsolute(blogArticle.Url, WorkContext.CurrentStore, WorkContext.CurrentLanguage);
                        return RedirectPermanent(articleRedirectUrl);
                    }
                }
            }

            throw new HttpException(404, articleUrl);
        }
    }
}
