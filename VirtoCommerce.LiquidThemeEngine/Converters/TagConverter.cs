using Microsoft.Practices.ServiceLocation;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.LiquidThemeEngine.Objects.Factories;
using VirtoCommerce.Storefront.Model.Catalog;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class TagConverter
    {
        public static Tag ToShopifyModel(this Term term)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidTag(term);
        }

        public static Tag ToShopifyModel(this AggregationItem item, string groupName, string groupLabel)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidTag(item, groupName, groupLabel);
        }
    }

    public partial class ShopifyModelConverter
    {
        public virtual Tag ToLiquidTag(Term term)
        {
            var factory = ServiceLocator.Current.GetInstance<ShopifyModelFactory>();
            var result = factory.CreateTag(term.Name, term.Value);
            return result;
        }

        public virtual Tag ToLiquidTag(AggregationItem item, string groupName, string groupLabel)
        {
            var factory = ServiceLocator.Current.GetInstance<ShopifyModelFactory>();
            var result = factory.CreateTag(groupName, item.Value != null ? item.Value.ToString() : null);
            result.GroupLabel = groupLabel;
            result.Label = item.Label;
            result.Count = item.Count;
            return result;
        }
    }

}
