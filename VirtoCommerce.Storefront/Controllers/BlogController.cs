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

        // GET: /blogs/{blog}, /blog, /blog/category/category, /blogs/{blog}/category/{category}, /blogs/{blog}/tag/{tag}, /blog/tag/{tag}
        public ActionResult GetBlog(string blog = null, string category = null, string tag = null)
        {
            var context = WorkContext;
            context.CurrentBlog = WorkContext.Blogs.FirstOrDefault();
            if (!string.IsNullOrEmpty(blog))
            {
                context.CurrentBlog = WorkContext.Blogs.FirstOrDefault(x => x.Name.EqualsInvariant(blog));
            }
            WorkContext.CurrentBlogSearchCritera.Category = category;
            WorkContext.CurrentBlogSearchCritera.Tag = tag;
            if (context.CurrentBlog != null)
            {
                context.CurrentPageSeo = new SeoInfo
                {
                    Language = context.CurrentBlog.Language,
                    MetaDescription = context.CurrentBlog.Name,
                    Title = context.CurrentBlog.Name,
                    Slug = context.RequestUrl.AbsolutePath
                };
                return View("blog", context.CurrentBlog.Layout, WorkContext);
            }
            throw new HttpException(404, blog);
        }         
    }
}
