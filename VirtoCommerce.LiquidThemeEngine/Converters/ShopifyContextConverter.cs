using System.Linq;
using DotLiquid;
using PagedList;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.StaticContent;
using storefrontModel = VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class ShopifyContextStaticConverter
    {
        public static ShopifyThemeWorkContext ToShopifyModel(this storefrontModel.WorkContext workContext, IStorefrontUrlBuilder urlBuilder)
        {
            var converter = AbstractTypeFactory<ShopifyContextConverter>.TryCreateInstance();
            return converter.ToShopifyModel(workContext, urlBuilder);
        }
    }
    public class ShopifyContextConverter
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

        public virtual ShopifyThemeWorkContext ToShopifyModel(storefrontModel.WorkContext workContext, IStorefrontUrlBuilder urlBuilder)
        {
            var result = AbstractTypeFactory<ShopifyThemeWorkContext>.TryCreateInstance();

            result.CurrentPage = 1;
            result.CountryOptionTags = string.Join("\r\n", workContext.AllCountries.OrderBy(c => c.Name).Select(c => c.ToOptionTag()));
            result.PageDescription = workContext.CurrentPageSeo != null ? workContext.CurrentPageSeo.MetaDescription : string.Empty;
            result.PageTitle = workContext.CurrentPageSeo != null ? workContext.CurrentPageSeo.Title : string.Empty;
            result.PageImageUrl = workContext.CurrentPageSeo != null ? workContext.CurrentPageSeo.ImageUrl : string.Empty;
            result.CanonicalUrl = workContext.CurrentPageSeo != null ? urlBuilder.ToAppAbsolute(workContext.CurrentPageSeo.Slug) : null;
            result.Shop = workContext.CurrentStore != null ? workContext.CurrentStore.ToShopifyModel(workContext) : null;
            result.Cart = workContext.CurrentCart != null ? workContext.CurrentCart.ToShopifyModel(workContext) : null;
            result.Product = workContext.CurrentProduct != null ? workContext.CurrentProduct.ToShopifyModel() : null;
            result.Vendor = workContext.CurrentVendor != null ? workContext.CurrentVendor.ToShopifyModel() : null;
            result.Customer = workContext.CurrentCustomer != null && workContext.CurrentCustomer.IsRegisteredUser ? workContext.CurrentCustomer.ToShopifyModel(workContext, urlBuilder) : null;
            result.AllStores = workContext.AllStores.Select(x => x.ToShopifyModel(workContext)).ToArray();
            result.CurrentCurrency = workContext.CurrentCurrency != null ? workContext.CurrentCurrency.ToShopifyModel() : null;
            result.CurrentLanguage = workContext.CurrentLanguage != null ? workContext.CurrentLanguage.ToShopifyModel() : null;

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

            if (workContext.Products != null)
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
            else if (workContext.CurrentStaticSearchCriteria != null && !string.IsNullOrEmpty(workContext.CurrentStaticSearchCriteria.Keyword) && workContext.Pages != null)
            {
                result.Search = new Search
                {
                    Performed = true,
                    SearchIn = workContext.CurrentStaticSearchCriteria.SearchIn,
                    Terms = workContext.CurrentStaticSearchCriteria.Keyword,
                    Results = new MutablePagedList<Drop>((pageNumber, pageSize, sortInfos) =>
                    {
                        var pagedContentItems = new MutablePagedList<ContentItem>(workContext.Pages);
                        pagedContentItems.Slice(pageNumber, pageSize, sortInfos);
                        return new StaticPagedList<Drop>(workContext.Pages.Select(x => x.ToShopifyModel()), pagedContentItems);
                    })
                };
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
            if (workContext.CurrentBlogSearchCritera != null)
            {
                result.BlogSearch = workContext.CurrentBlogSearchCritera.ToShopifyModel();
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
