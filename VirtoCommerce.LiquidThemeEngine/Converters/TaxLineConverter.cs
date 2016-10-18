using Microsoft.Practices.ServiceLocation;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.LiquidThemeEngine.Objects.Factories;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class TaxLineConverter
    {
        public static TaxLine ToShopifyModel(this Storefront.Model.TaxDetail taxDetail)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidTaxLine(taxDetail);
        }
    }

    public partial class ShopifyModelConverter
    {
        public virtual TaxLine ToLiquidTaxLine(Storefront.Model.TaxDetail taxDetail)
        {
            var factory = ServiceLocator.Current.GetInstance<ShopifyModelFactory>();
            var result = factory.CreateTaxLine();

            result.Price = taxDetail.Amount.Amount * 100;
            result.Title = taxDetail.Name;

            return result;
        }
    }
}