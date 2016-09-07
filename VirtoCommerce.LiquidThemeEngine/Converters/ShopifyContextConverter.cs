using System.Linq;
using PagedList;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.StaticContent;
using storefrontModel = VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class ShopifyContextConverter
    {
        private static readonly string[] _poweredLinks = {
                                             "<a href=\"http://virtocommerce.com\" rel=\"nofollow\" target=\"_blank\">.NET ecommerce platform</a> by Virto",
                                             "<a href=\"http://virtocommerce.com/shopping-cart\" rel=\"nofollow\" target=\"_blank\">Shopping Cart</a> by Virto",
                                             "<a href=\"http://virtocommerce.com/shopping-cart\" rel=\"nofollow\" target=\"_blank\">.NET Shopping Cart</a> by Virto",
                                             "<a href=\"http://virtocommerce.com/shopping-cart\" rel=\"nofollow\" target=\"_blank\">ASP.NET Shopping Cart</a> by Virto",
                                             "<a href=\"http://virtocommerce.com\" rel=\"nofollow\" target=\"_blank\">.NET ecommerce</a> by Virto",
                                             "<a href=\"http://virtocommerce.com\" rel=\"nofollow\" target=\"_blank\">.NET ecommerce framework</a> by Virto",
                                             "<a href=\"http://virtocommerce.com\" rel=\"nofollow\" target=\"_blank\">ASP.NET ecommerce</a> by Virto Commerce",
                                             "<a href=\"http://virtocommerce.com\" rel=\"nofollow\" target=\"_blank\">ASP.NET ecommerce platform</a> by Virto",
                                             "<a href=\"http://virtocommerce.com\" rel=\"nofollow\" target=\"_blank\">ASP.NET ecommerce framework</a> by Virto",
                                             "<a href=\"http://virtocommerce.com\" rel=\"nofollow\" target=\"_blank\">Enterprise ecommerce</a> by Virto",
                                             "<a href=\"http://virtocommerce.com\" rel=\"nofollow\" target=\"_blank\">Enterprise ecommerce platform</a> by Virto",
                                         };

        public static ShopifyThemeWorkContext ToShopifyModel(this storefrontModel.WorkContext workContext, IStorefrontUrlBuilder urlBuilder)
        {
            var result = new ShopifyThemeWorkContext
            {
                CurrentPage = 1,
                CountryOptionTags = string.Join("\r\n", workContext.AllCountries.OrderBy(c => c.Name).Select(c => c.ToOptionTag())),
                PageDescription = workContext.CurrentPageSeo != null ? workContext.CurrentPageSeo.MetaDescription : string.Empty,
                PageTitle = workContext.CurrentPageSeo != null ? workContext.CurrentPageSeo.Title : string.Empty,
                CanonicalUrl = workContext.CurrentPageSeo != null ? urlBuilder.ToAppAbsolute(workContext.CurrentPageSeo.Slug) : null,
                Shop = workContext.CurrentStore != null ? workContext.CurrentStore.ToShopifyModel(workContext) : null,
                Cart = workContext.CurrentCart != null ? workContext.CurrentCart.ToShopifyModel(workContext) : null,
                Product = workContext.CurrentProduct != null ? workContext.CurrentProduct.ToShopifyModel() : null,
                Vendor = workContext.CurrentVendor != null ? workContext.CurrentVendor.ToShopifyModel() : null,
                Customer = workContext.CurrentCustomer != null && workContext.CurrentCustomer.IsRegisteredUser ? workContext.CurrentCustomer.ToShopifyModel(workContext, urlBuilder) : null,
                AllStores = workContext.AllStores.Select(x => x.ToShopifyModel(workContext)).ToArray(),
                CurrentCurrency = workContext.CurrentCurrency != null ? workContext.CurrentCurrency.ToShopifyModel() : null,
                CurrentLanguage = workContext.CurrentLanguage != null ? workContext.CurrentLanguage.ToShopifyModel() : null
            };

            if (workContext.CurrentProductSearchCriteria != null && workContext.CurrentProductSearchCriteria.Terms.Any())
            {
                result.CurrentTags =
                    new TagCollection(
                        workContext.CurrentProductSearchCriteria.Terms.Select(t => t.ToShopifyModel()).ToList());
            }

            if (workContext.CurrentCategory != null)
            {
                result.Collection = workContext.CurrentCategory.ToShopifyModel(workContext);
            }

            if (workContext.Categories != null)
            {
                result.Collections = new Collections(new MutablePagedList<Collection>((pageNumber, pageSize, sortInfos) =>
                {
                    workContext.Categories.Slice(pageNumber, pageSize, sortInfos);
                    return new StaticPagedList<Collection>(workContext.Categories.Select(x => x.ToShopifyModel(workContext)), workContext.Categories);
                }));
            }

            if(workContext.Products != null)
            {
                result.Products = new MutablePagedList<Product>((pageNumber, pageSize, sortInfos) =>
                {
                    workContext.Products.Slice(pageNumber, pageSize, sortInfos);
                    return new StaticPagedList<Product>(workContext.Products.Select(x => x.ToShopifyModel()), workContext.Products);
                }, workContext.Products.PageNumber, workContext.Products.PageSize);
            }

            if (workContext.Vendors != null)
            {
                result.Vendors = new MutablePagedList<Vendor>((pageNumber, pageSize, sortInfos) =>
                {
                    workContext.Vendors.Slice(pageNumber, pageSize, sortInfos);
                    return new StaticPagedList<Vendor>(workContext.Vendors.Select(x => x.ToShopifyModel()), workContext.Vendors);
                }, workContext.Vendors.PageNumber, workContext.Vendors.PageSize);
            }

            if (!string.IsNullOrEmpty(workContext.CurrentProductSearchCriteria.Keyword) && workContext.Products != null)
            {
                result.Search = workContext.Products.ToShopifyModel(workContext.CurrentProductSearchCriteria.Keyword);
            }

            if (workContext.CurrentLinkLists != null)
            {
                result.Linklists = new Linklists(workContext.CurrentLinkLists.Select(x => x.ToShopifyModel(workContext, urlBuilder)));
            }

            if (workContext.Pages != null)
            {
                result.Pages = new Pages(workContext.Pages.OfType<ContentPage>().Select(x => x.ToShopifyModel()));
                result.Blogs = new Blogs(workContext.Blogs.Select(x => x.ToShopifyModel(workContext.CurrentLanguage)));
            }

            if (workContext.CurrentOrder != null)
            {
                result.Order = workContext.CurrentOrder.ToShopifyModel(urlBuilder);
            }

            if (workContext.CurrentQuoteRequest != null)
            {
                result.QuoteRequest = workContext.CurrentQuoteRequest.ToShopifyModel();
            }

            result.PaymentFormHtml = workContext.PaymentFormHtml;

            if (workContext.CurrentPage != null)
            {
                result.Page = workContext.CurrentPage.ToShopifyModel();
            }

            if (workContext.CurrentBlog != null)
            {
                result.Blog = workContext.CurrentBlog.ToShopifyModel(workContext.CurrentLanguage);
            }

            if (workContext.CurrentBlogArticle != null)
            {
                result.Article = workContext.CurrentBlogArticle.ToShopifyModel();
            }

            if (workContext.ContactUsForm != null)
            {
                result.Form = workContext.ContactUsForm.ToShopifyModel();
            }

            if (workContext.Login != null)
            {
                result.Form = workContext.Login.ToShopifyModel();
            }

            if (workContext.StorefrontNotification != null)
            {
                result.Notification = workContext.StorefrontNotification.ToShopifyModel();
            }

            result.ExternalLoginProviders = workContext.ExternalLoginProviders.Select(p => new LoginProvider
            {
                AuthenticationType = p.AuthenticationType,
                Caption = p.Caption,
                Properties = p.Properties
            }).ToList();

            result.ApplicationSettings = new MetafieldsCollection("application_settings", workContext.ApplicationSettings);

            //Powered by link
            if (workContext.CurrentStore != null)
            {
                var storeName = workContext.CurrentStore.Name;
                var hashCode = (uint)storeName.GetHashCode();
                result.PoweredByLink = _poweredLinks[hashCode % _poweredLinks.Length];
            }

            result.CurrentPage = 1;
            if (workContext.RequestUrl != null)
            {
                result.RequestUrl = workContext.RequestUrl.ToString();

                //Populate current page number
                result.CurrentPage = workContext.PageNumber ?? 1;
                result.PageSize = workContext.PageSize ?? 0;
            }
            return result;
        }
    }
}
