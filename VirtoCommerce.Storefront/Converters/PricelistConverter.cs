using VirtoCommerce.Storefront.Model.Pricing;

namespace VirtoCommerce.Storefront.Converters
{
    public static class PricelistConverter
    {
        public static Pricelist ToWebModel(this PricingModule.Client.Model.Pricelist pricelist)
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
