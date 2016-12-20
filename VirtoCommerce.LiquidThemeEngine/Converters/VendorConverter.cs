using Omu.ValueInjecter;
using System.Linq;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.Storefront.Model.Common;
using StorefrontModel = VirtoCommerce.Storefront.Model;
using PagedList;
using Microsoft.Practices.ServiceLocation;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class VendorConverter
    {
        public static Vendor ToShopifyModel(this StorefrontModel.Vendor vendor)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidVendor(vendor);
        }
    }

    public partial class ShopifyModelConverter
    {
        public virtual Vendor ToLiquidVendor(Storefront.Model.Vendor vendor)
        {
            var result = new Vendor();
            result.InjectFrom<NullableAndEnumValueInjecter>(vendor);
            result.Handle = vendor.SeoInfo != null ? vendor.SeoInfo.Slug : vendor.Id;

            var shopifyAddressModels = vendor.Addresses.Select(a => ToLiquidAddress(a));
            result.Addresses = new MutablePagedList<Address>(shopifyAddressModels);
            result.DynamicProperties = vendor.DynamicProperties;

            if (vendor.Products != null)
            {
                result.Products = new MutablePagedList<Product>((pageNumber, pageSize, sortInfos) =>
                {
                    vendor.Products.Slice(pageNumber, pageSize, sortInfos);
                    return new StaticPagedList<Product>(vendor.Products.Select(x => ToLiquidProduct(x)), vendor.Products);
                }, vendor.Products.PageNumber, vendor.Products.PageSize);
            }

            return result;
        }
    }

}