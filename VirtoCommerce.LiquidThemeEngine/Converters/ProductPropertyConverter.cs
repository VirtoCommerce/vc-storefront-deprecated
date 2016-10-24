using Omu.ValueInjecter;
using System.Linq;
using VirtoCommerce.LiquidThemeEngine.Objects;
using StorefrontModel = VirtoCommerce.Storefront.Model;
using Microsoft.Practices.ServiceLocation;
using VirtoCommerce.LiquidThemeEngine.Objects.Factories;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class ProductPropertyConverter
    {
        public static ProductProperty ToShopifyModel(this StorefrontModel.Catalog.CatalogProperty property)
        {
            var converter = ServiceLocator.Current.GetInstance<ShopifyModelConverter>();
            return converter.ToLiquidProductProperty(property);
        }
    }

    public partial class ShopifyModelConverter
    {
        public virtual ProductProperty ToLiquidProductProperty(StorefrontModel.Catalog.CatalogProperty property)
        {
            var factory = ServiceLocator.Current.GetInstance<ShopifyModelFactory>();
            var result = factory.CreateProductProperty();
            result.ValueType = property.ValueType;
            result.Value = property.Value;
            result.Name = property.Name;
            
            return result;
        }
    }

}