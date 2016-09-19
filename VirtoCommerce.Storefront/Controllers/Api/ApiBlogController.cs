using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.StaticContent;
using VirtoCommerce.Storefront.Model.StaticContent.Services;

namespace VirtoCommerce.Storefront.Controllers.Api
{
    public class ApiBlogController : StorefrontControllerBase
    {
        private readonly IStaticContentService _staticContentService;

        public ApiBlogController(WorkContext workContext, IStorefrontUrlBuilder urlBuilder, IStaticContentService staticContentService)
            : base(workContext, urlBuilder)
        {
            _staticContentService = staticContentService;
        }

        // GET: storefrontapi/blog/articles
        [HttpGet]
        public ActionResult Articles(string blogName, string filterType, string criteria, int page, int pageSize)
        {
            var articles = new List<BlogArticle>();

            var allContentItems = _staticContentService.LoadStoreStaticContent(WorkContext.CurrentStore).ToList();
            var blogs = allContentItems.OfType<Blog>().ToArray();
            var blogArticlesGroup = allContentItems.OfType<BlogArticle>().GroupBy(x => x.BlogName, x => x).ToList();
            var blogArticles = blogArticlesGroup.FirstOrDefault(x => string.Equals(x.Key, blogName, StringComparison.OrdinalIgnoreCase));
            if (blogArticles != null)
            {
                var foundedArticles = blogArticles.AsQueryable();
                if (!string.IsNullOrEmpty(filterType) && !string.IsNullOrEmpty(criteria))
                {
                    if (filterType.Equals("category", StringComparison.OrdinalIgnoreCase))
                    {
                        foundedArticles = foundedArticles.Where(a => !string.IsNullOrEmpty(a.Category) && a.Category.Equals(criteria, StringComparison.OrdinalIgnoreCase));
                    }
                    if (filterType.Equals("tag", StringComparison.OrdinalIgnoreCase))
                    {
                        foundedArticles = foundedArticles.Where(a => a.Tags != null && a.Tags.Contains(criteria, StringComparer.OrdinalIgnoreCase));
                    }
                }

                articles = foundedArticles.OrderByDescending(a => a.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            return Json(articles, JsonRequestBehavior.AllowGet);
        }
    }
}