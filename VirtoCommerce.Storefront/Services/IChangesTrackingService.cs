using System.Threading.Tasks;

namespace VirtoCommerce.Storefront.Services
{
    public interface IChangesTrackingService
    {
        Task<bool> HasChanges();
    }
}
