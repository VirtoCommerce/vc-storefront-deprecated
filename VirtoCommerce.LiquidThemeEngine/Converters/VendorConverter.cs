using Omu.ValueInjecter;
using System.Linq;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.Storefront.Model.Common;
using StorefrontModel = VirtoCommerce.Storefront.Model;
using PagedList;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class VendorConverter
    {
        public static Vendor ToShopifyModel(this StorefrontModel.Vendor vendor)
        {
            var retVal = new Vendor();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(vendor);
            retVal.Handle = vendor.SeoInfo != null ? vendor.SeoInfo.Slug : vendor.Id;

            var shopifyAddressModels = vendor.Addresses.Select(a => a.ToShopifyModel());
            retVal.Addresses = new MutablePagedList<Address>(shopifyAddressModels);
            retVal.DynamicProperties = vendor.DynamicProperties;

            if (vendor.Products != null)
            {
                retVal.Products = new MutablePagedList<Product>((pageNumber, pageSize, sortInfos) =>
                {
                    vendor.Products.Slice(pageNumber, pageSize, sortInfos);
                    return new StaticPagedList<Product>(vendor.Products.Select(x => x.ToShopifyModel()), vendor.Products);
                }, vendor.Products.PageNumber, vendor.Products.PageSize);
            }

            return retVal;
        }
    }
}