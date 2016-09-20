using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.StaticContent;

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
            return View("blog", WorkContext.CurrentBlog.Layout, WorkContext);
        }

        // GET: /blogs/{blogname}/category/{category}
        public ActionResult GetArticlesByCategory(string blogName, string category)
        {
            var blog = WorkContext.Blogs.FirstOrDefault(b => b.Name.Equals(blogName, StringComparison.OrdinalIgnoreCase));
            if (blog != null)
            {
                var blogClone = new Blog();
                //Need to clone exist blog because it may be memory cached
                blogClone.InjectFrom<NullableAndEnumValueInjecter>(blog);
                var seoInfo = new SeoInfo
                {
                    Language = blog.Language,
                    MetaDescription = blog.Title,
                    Slug = string.Format("/blogs/{0}/category/{1}", blog, category),
                    Title = blog.Title
                };

                var articles = blog.Articles.Where(a => !string.IsNullOrEmpty(a.Category) && a.Category.EqualsInvariant(category) && a.PublicationStatus != ContentPublicationStatus.Private);
                if (articles != null)
                {
                    blogClone.Articles = new MutablePagedList<BlogArticle>(articles);
                }

                WorkContext.CurrentBlog = blogClone;
                WorkContext.CurrentPageSeo = seoInfo;
            }

            return View("blog", blog.Layout, WorkContext);
        }

        // GET: /blogs/{blogname}/tag/{tag}
        public ActionResult GetArticlesByTag(string blogName, string tag)
        {
            var blog = WorkContext.Blogs.FirstOrDefault(b => b.Name.Equals(blogName, StringComparison.OrdinalIgnoreCase));
            if (blog != null)
            {
                var blogClone = new Blog();
                //Need to clone exist blog because it may be memory cached
                blogClone.InjectFrom<NullableAndEnumValueInjecter>(blog);
                var seoInfo = new SeoInfo
                {
                    Language = blog.Language,
                    MetaDescription = blog.Title,
                    Slug = string.Format("/blogs/{0}/tag/{1}", blog, tag),
                    Title = blog.Title
                };

                var articles = blog.Articles.Where(a => a.Tags != null && a.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase) && a.PublicationStatus != ContentPublicationStatus.Private);
                if (articles != null)
                {
                    blogClone.Articles = new MutablePagedList<BlogArticle>(articles);
                }

                WorkContext.CurrentBlog = blogClone;
                WorkContext.CurrentPageSeo = seoInfo;            }

            return View("blog", blog.Layout, WorkContext);
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
                        Slug = blogArticle.Url,
                        ImageUrl = blogArticle.ImageUrl
                    };
                    var layout = string.IsNullOrEmpty(blogArticle.Layout) ? context.CurrentBlog.Layout : blogArticle.Layout;
                    return View("article", layout, WorkContext);
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
