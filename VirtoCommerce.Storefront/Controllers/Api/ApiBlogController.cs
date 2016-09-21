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
            var blog = base.WorkContext.Blogs.FirstOrDefault(x => x.Name.EqualsInvariant(blogName));
            if (blog != null)
            {
                var query = blog.Articles.AsQueryable().Where(x => x.PublicationStatus != ContentPublicationStatus.Private);
                if (!string.IsNullOrEmpty(filterType) && !string.IsNullOrEmpty(criteria))
                {
                    if (filterType.EqualsInvariant("category"))
                    {
                        query = query.Where(a => !string.IsNullOrEmpty(a.Category) && a.Category.Replace(" ", "-").Equals(criteria, StringComparison.OrdinalIgnoreCase));
                    }
                    if (filterType.EqualsInvariant("tag"))
                    {
                        query = query.Where(a => a.Tags != null && a.Tags.Select(t => t.Replace(" ", "-")).Contains(criteria, StringComparer.OrdinalIgnoreCase));
                    }
                }
                articles = query.OrderByDescending(a => a.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
            return Json(articles, JsonRequestBehavior.AllowGet);
        }
    }
}