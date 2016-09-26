using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.Storefront.Model.Common;
using StorefrontModel = VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class LineItemConverter
    {
        public static LineItem ToShopifyModel(this StorefrontModel.Cart.LineItem lineItem, StorefrontModel.WorkContext workContext)
        {
            var retVal = new LineItem();

            //shopifyModel.Product = lineItem.Product.ToShopifyModel();
            retVal.Fulfillment = null; // TODO
            retVal.Grams = lineItem.Weight ?? 0m;
            retVal.Id = lineItem.Id;
            retVal.Image = new Image
            {
                Alt = lineItem.Name,
                Name = lineItem.Name,
                ProductId = lineItem.ProductId,
                Src = lineItem.ImageUrl
            };
            retVal.LinePrice = lineItem.ExtendedPrice.Amount * 100;
            retVal.LinePriceWithTax = lineItem.ExtendedPriceWithTax.Amount * 100;
            retVal.Price = lineItem.PlacedPrice.Amount * 100;
            retVal.PriceWithTax = lineItem.PlacedPriceWithTax.Amount * 100;
            retVal.ProductId = lineItem.ProductId;
            //shopifyModel.Properties = null; // TODO
            retVal.Quantity = lineItem.Quantity;
            retVal.RequiresShipping = lineItem.RequiredShipping;
            retVal.Sku = lineItem.Sku;
            retVal.Taxable = lineItem.TaxIncluded;
            retVal.Title = lineItem.Name;
            retVal.Type = null; // TODO
            retVal.Url = null; // TODO
            retVal.Variant = null; // TODO
            retVal.VariantId = lineItem.ProductId;
            retVal.Vendor = null; // TODO
            
            retVal.Properties = new MetafieldsCollection("properties", workContext.CurrentLanguage, lineItem.DynamicProperties);

            return retVal;
        }

        public static LineItem ToShopifyModel(this StorefrontModel.Order.LineItem lineItem, IStorefrontUrlBuilder urlBuilder)
        {
            var result = new LineItem
            {
                Fulfillment = null,
                Grams = lineItem.Weight ?? 0m,
                Id = lineItem.Id,
                Quantity = lineItem.Quantity ?? 0,
                Price = lineItem.PlacedPrice.Amount * 100,
                PriceWithTax = lineItem.PlacedPriceWithTax.Amount * 100,
                LinePrice = lineItem.ExtendedPrice.Amount * 100,
                LinePriceWithTax = lineItem.ExtendedPriceWithTax.Amount * 100,
                ProductId = lineItem.ProductId,
                Sku = lineItem.Name,
                Title = lineItem.Name,
                Url = urlBuilder.ToAppAbsolute("/product/" + lineItem.ProductId),
            };
            result.Product = new Product
            {
                Id = result.ProductId,
                Url = result.Url
            };

            //result.Image = lineItem.Product.PrimaryImage != null ? lineItem.Product.PrimaryImage.ToShopifyModel() : null;
            //result.RequiresShipping = lineItem.RequiredShipping;
            //result.Taxable = lineItem.TaxIncluded;

            return result;
        }
    }
}