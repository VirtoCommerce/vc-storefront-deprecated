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
            if (blogArticle != null)
            {
                WorkContext.CurrentBlogArticle = blogArticle;
                WorkContext.CurrentBlog = WorkContext.Blogs.SingleOrDefault(x => x.Name.Equals(blogArticle.BlogName, StringComparison.OrdinalIgnoreCase));
                return View("article", page.Layout, WorkContext);
            }

            var contentPage = page as ContentPage;
            SetCurrentPage(contentPage);
            return View("page", page.Layout, WorkContext);
        }

        // GET: /pages/{page}
        public ActionResult GetContentPageByName(string page)
        {
            var contentPage = WorkContext.Pages
                .OfType<ContentPage>()
                .Where(x => string.Equals(x.Url, page, StringComparison.OrdinalIgnoreCase))
                .FindWithLanguage(WorkContext.CurrentLanguage);

            if (contentPage != null)
            {
                SetCurrentPage(contentPage);
                return View("page", WorkContext);
            }

            throw new HttpException(404, "Page not found. Page URL: '" + page + "'.");
        }


        private void SetCurrentPage(ContentPage contentPage)
        {
            WorkContext.CurrentPage = contentPage;
            WorkContext.CurrentPageSeo = new SeoInfo
            {
                Language = contentPage.Language,
                Title = contentPage.Title,
                Slug = contentPage.Permalink
            };
        }
    }
}
