using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.StaticContent;

namespace VirtoCommerce.Storefront.Controllers
{
    [OutputCache(CacheProfile = "StaticContentCachingProfile")]
    public class PageController : StorefrontControllerBase
    {
        public PageController(WorkContext context, IStorefrontUrlBuilder urlBuilder)
            : base(context, urlBuilder)
        {
        }

        //Called from SEO route by page permalink
        public ActionResult GetContentPage(ContentItem page)
        {
            var blogArticle = page as BlogArticle;
            var contentPage = page as ContentPage;
            if (blogArticle != null)
            {
                WorkContext.CurrentBlogArticle = blogArticle;
                WorkContext.CurrentBlog = WorkContext.Blogs.SingleOrDefault(x => x.Name.Equals(blogArticle.BlogName, StringComparison.OrdinalIgnoreCase));
                return View("article", page.Layout, WorkContext);
            }
            else
            {
                WorkContext.CurrentPage = contentPage;
                return View("page", page.Layout, WorkContext);
            }
        }

        // GET: /pages/{page}
        public ActionResult GetContentPageByName(string page)
        {

            var contentPages = WorkContext.Pages.Where(x => string.Equals(x.Url, page, StringComparison.OrdinalIgnoreCase));
            var contentPage = contentPages.FindWithLanguage(WorkContext.CurrentLanguage);
            if (contentPage != null)
            {
                WorkContext.CurrentPage = contentPage as ContentPage;

                return View("page", WorkContext);
            }
            throw new HttpException(404, "Page with " + page + " not found.");
        }
    }
}
