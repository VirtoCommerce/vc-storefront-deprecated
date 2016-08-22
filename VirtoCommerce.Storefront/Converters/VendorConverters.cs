using System.Linq;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Stores;

namespace VirtoCommerce.Storefront.Converters
{
    public static class VendorConverters
    {
        public static Vendor ToWebModel(this CustomerModule.Client.Model.Vendor vendor, Language currentLanguage, Store store)
        {
            Vendor result = null;

            if (vendor != null)
            {
                result = new Vendor
                {
                    Id = vendor.Id,
                    Name = vendor.Name,
                    Description = vendor.Description,
                    LogoUrl = vendor.LogoUrl,
                    SiteUrl = vendor.SiteUrl,
                    GroupName = vendor.GroupName,
                };
                var vendorSeoInfo = vendor.SeoInfos.GetBestMatchedSeoInfo(store, currentLanguage);
                if(vendorSeoInfo != null)
                {
                    result.SeoInfo = vendorSeoInfo.ToWebModel();
                }
                else
                {
                    result.SeoInfo = new SeoInfo
                    {
                        Title = vendor.Name,
                        Slug = string.Concat("/vendor/", result.Id)
                    };
                }
                if (vendor.Addresses != null)
                {
                    result.Addresses = vendor.Addresses.Select(a => a.ToWebModel()).ToList();
                }

                if (vendor.DynamicProperties != null)
                {
                    result.DynamicProperties = vendor.DynamicProperties.Select(a => a.ToWebModel()).ToList();
                }
            }

            return result;
        }
    }
}
