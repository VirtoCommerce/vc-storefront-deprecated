namespace VirtoCommerce.Storefront.Model.Catalog.Services
{
    public interface IProductAvailabilityService
    {
        long? GetAvailableQuantity(Product product);

        bool IsAvailable(Product product, long requestedQuantity);
    }
}