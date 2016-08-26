using VirtoCommerce.Storefront.Model.Pricing;
using pricingModel = VirtoCommerce.Storefront.AutoRestClients.PricingModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class PricelistConverter
    {
        public static Pricelist ToWebModel(this pricingModel.Pricelist pricelist)
        {
            var result = new Pricelist
            {
                Id = pricelist.Id,
                Currency = pricelist.Currency,
            };

            return result;
        }
    }
}
