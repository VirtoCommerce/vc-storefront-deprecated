using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.LiquidThemeEngine.Objects.Factories
{
    public class ShopifyModelFactory
    {
        public virtual ShopifyThemeWorkContext CreateContext()
        {
            return new ShopifyThemeWorkContext();
        }

        public virtual Cart CreateCart()
        {
            return new Cart();
        }

        public virtual Customer CreateCustomer()
        {
            return new Customer();
        }

        public virtual QuoteRequest CreateQuoteRequest()
        {
            return new QuoteRequest();
        }

        public virtual QuoteItem CreateQuoteItem()
        {
            return new QuoteItem();
        }

        public virtual QuoteRequestTotals CreateQuoteRequestTotals()
        {
            return new QuoteRequestTotals();
        }

        public virtual Order CreateOrder()
        {
            return new Order();
        }

        public virtual Vendor CreateVendor()
        {
            return new Vendor();
        }

        public virtual Transaction CreateTransaction()
        {
            return new Transaction();
        }

        public virtual TierPrice CreateTierPrice()
        {
            return new TierPrice();
        }

        public virtual TaxLine CreateTaxLine()
        {
            return new TaxLine();
        }

        public virtual Tag CreateTag(string groupName, string value)
        {
            return new Tag(groupName, value);
        }
        public virtual Collection CreateCollection()
        {
            return new Collection();
        }

        public virtual Shop CreateShop()
        {
            return new Shop();
        }

        public virtual ShippingMethod CreateShippingMethod()
        {
            return new ShippingMethod();
        }

        public virtual Search CreateSearch()
        {
            return new Search();
        }

        public virtual ProductProperty CreateProductProperty()
        {
            return new ProductProperty();
        }

        public virtual Product CreateProduct()
        {
            return new Product();
        }

        public virtual Page CreatePage()
        {
            return new Page();
        }

        public virtual Notification CreateNotification()
        {
            return new Notification();
        }
        public virtual Link CreateLink()
        {
            return new Link();
        }

        public virtual Linklist CreateLinklist()
        {
            return new Linklist();
        }
        public virtual LineItem CreateLineItem()
        {
            return new LineItem();
        }

        public virtual Language CreateLanguage()
        {
            return new Language();
        }

        public virtual Image CreateImage()
        {
            return new Image();
        }

        public virtual BlogSearch CreateBlogSearch()
        {
            return new BlogSearch();
        }

        public virtual Attachment CreateAttachment()
        {
            return new Attachment();
        }

        public virtual Address CreateAddress()
        {
            return new Address();
        }

        public virtual Article CreateArticle()
        {
            return new Article();
        }

        public virtual Blog CreateBlog()
        {
            return new Blog();
        }
    }
}
