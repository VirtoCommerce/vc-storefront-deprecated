using System.Collections.Generic;
using System.Threading.Tasks;
using PagedList;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model.Customer.Services
{
    public interface ICustomerService
    {
        Task<CustomerInfo> GetCustomerByIdAsync(string customerId);
        Task CreateCustomerAsync(CustomerInfo customer);
        Task UpdateCustomerAsync(CustomerInfo customer);
        Task<bool> CanLoginOnBehalfAsync(string storeId, string customerId);
        Task<Vendor[]> GetVendorsByIdsAsync(params string[] vendorIds);
        Vendor[] GetVendorsByIds(params string[] vendorIds);
        IPagedList<Vendor> SearchVendors(string keyword, int pageNumber, int pageSize, IEnumerable<SortInfo> sortInfos);
    }
}
