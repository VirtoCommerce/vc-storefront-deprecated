using System.Globalization;
using System.Linq;
using Omu.ValueInjecter;
using PagedList;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Customer;
using StorefrontModel = VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class CustomerConverter
    {
        public static Customer ToShopifyModel(this CustomerInfo customer, StorefrontModel.WorkContext workContext, IStorefrontUrlBuilder urlBuilder)
        {
            var result = new Customer();
            result.InjectFrom<NullableAndEnumValueInjecter>(customer);
            result.Name = customer.FullName;
            result.DefaultAddress = customer.DefaultAddress.ToShopifyModel();
            result.DefaultBillingAddress = customer.DefaultBillingAddress.ToShopifyModel();
            result.DefaultShippingAddress = customer.DefaultShippingAddress.ToShopifyModel();

            if (customer.Tags != null)
            {
                result.Tags = customer.Tags.ToList();
            }

            if (customer.Addresses != null)
            {
                var addresses = customer.Addresses.Select(a => a.ToShopifyModel()).ToList();
                result.Addresses = new MutablePagedList<Address>(addresses);
            }

            if (customer.Orders != null)
            {
                result.Orders = new MutablePagedList<Order>((pageNumber, pageSize, sortInfos) =>
                {
                    customer.Orders.Slice(pageNumber, pageSize, sortInfos);
                    return new StaticPagedList<Order>(customer.Orders.Select(x => x.ToShopifyModel(urlBuilder)), customer.Orders);
                }, customer.Orders.PageNumber, customer.Orders.PageSize);
            }

            if (customer.QuoteRequests != null)
            {
                result.QuoteRequests = new MutablePagedList<QuoteRequest>((pageNumber, pageSize, sortInfos) =>
                {
                    customer.QuoteRequests.Slice(pageNumber, pageSize, sortInfos);
                    return new StaticPagedList<QuoteRequest>(customer.QuoteRequests.Select(x => x.ToShopifyModel()), customer.QuoteRequests);
                }, customer.QuoteRequests.PageNumber, customer.QuoteRequests.PageSize);
            }

            if (customer.DynamicProperties != null)
            {
                result.Metafields = new MetaFieldNamespacesCollection(new[] { new MetafieldsCollection("dynamic_properties", workContext.CurrentLanguage, customer.DynamicProperties) });
            }
            return result;
        }
    }
}
