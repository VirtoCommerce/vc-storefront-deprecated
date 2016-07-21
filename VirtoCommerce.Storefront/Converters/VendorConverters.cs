using System.Linq;
using VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.Storefront.Converters
{
    public static class VendorConverters
    {
        public static Vendor ToWebModel(this CustomerModule.Client.Model.Vendor vendor)
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
