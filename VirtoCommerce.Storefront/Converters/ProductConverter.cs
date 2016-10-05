using System;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;
using VirtoCommerce.Storefront.Model.Quote;
using VirtoCommerce.Storefront.Model.Stores;
using catalogModel = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;
using searchModel = VirtoCommerce.Storefront.AutoRestClients.SearchApiModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class ProductStaticConverter
    {
        public static Product ToWebModel(this searchModel.Product product, Language currentLanguage, Currency currentCurrency, Store store)
        {
            var converter = AbstractTypeFactory<ProductConverter>.TryCreateInstance();
            return converter.ToWebModel(product, currentLanguage, currentCurrency, store);
        }

        public static Product ToWebModel(this catalogModel.Product product, Language currentLanguage, Currency currentCurrency, Store store)
        {
            var converter = AbstractTypeFactory<ProductConverter>.TryCreateInstance();
            return converter.ToWebModel(product, currentLanguage, currentCurrency, store);
        }

        public static QuoteItem ToQuoteItem(this Product product, long quantity)
        {
            var converter = AbstractTypeFactory<ProductConverter>.TryCreateInstance();
            return converter.ToQuoteItem(product, quantity);
        }

        public static PromotionProductEntry ToPromotionItem(this Product product)
        {
            var converter = AbstractTypeFactory<ProductConverter>.TryCreateInstance();
            return converter.ToPromotionItem(product);
        }
    }

    public class ProductConverter
    {
        public virtual Product ToWebModel(searchModel.Product product, Language currentLanguage, Currency currentCurrency, Store store)
        {
            return ToWebModel(product.JsonConvert<catalogModel.Product>(), currentLanguage, currentCurrency, store);
        }

        public virtual Product ToWebModel(catalogModel.Product product, Language currentLanguage, Currency currentCurrency, Store store)
        {
            var result = AbstractTypeFactory<Product>.TryCreateInstance();
            result.InjectFrom<NullableAndEnumValueInjecter>(product);

            result.Currency = currentCurrency;
            result.Price = new ProductPrice(currentCurrency);
            result.Weight = (decimal?)product.Weight;
            result.Height = (decimal?)product.Height;
            result.Width = (decimal?)product.Width;
            result.Length = (decimal?)product.Length;
            result.Sku = product.Code;
            result.VendorId = product.Vendor;
            result.Outline = product.Outlines.GetOutlinePath(store.Catalog);
            result.Url = "~/" + product.Outlines.GetSeoPath(store, currentLanguage, "product/" + product.Id);

            if (product.Properties != null)
            {
                result.Properties = product.Properties
                    .Where(x => string.Equals(x.Type, "Product", StringComparison.InvariantCultureIgnoreCase))
                    .Select(p => p.ToWebModel(currentLanguage))
                    .ToList();

                result.VariationProperties = product.Properties
                    .Where(x => string.Equals(x.Type, "Variation", StringComparison.InvariantCultureIgnoreCase))
                    .Select(p => p.ToWebModel(currentLanguage))
                    .ToList();
            }

            if (product.Images != null)
            {
                result.Images = product.Images.Select(i => i.ToWebModel()).ToArray();
                result.PrimaryImage = result.Images.FirstOrDefault(x => string.Equals(x.Url, product.ImgSrc, StringComparison.InvariantCultureIgnoreCase));
            }

            if (product.Assets != null)
            {
                result.Assets = product.Assets.Select(x => x.ToWebModel()).ToList();
            }

            if (product.Variations != null)
            {
                result.Variations = product.Variations.Select(v => ToWebModel(v, currentLanguage, currentCurrency, store)).ToList();
            }

            if (!product.Associations.IsNullOrEmpty())
            {
                result.Associations.AddRange(product.Associations.Select(x => x.ToWebModel()).Where(x => x != null));
            }

            var productSeoInfo = product.SeoInfos.GetBestMatchedSeoInfo(store, currentLanguage);
            if (productSeoInfo != null)
            {
                result.SeoInfo = productSeoInfo.ToWebModel();
            }

            if (product.Reviews != null)
            {
                result.Descriptions = product.Reviews.Select(r => new EditorialReview
                {
                    Language = new Language(r.LanguageCode),
                    ReviewType = r.ReviewType,
                    Value = r.Content
                }).Where(x => x.Language.Equals(currentLanguage)).ToList();
                result.Description = result.Descriptions.FindWithLanguage(currentLanguage, x => x.Value, null);
            }

            return result;
        }

        public virtual QuoteItem ToQuoteItem(Product product, long quantity)
        {
            var quoteItem = AbstractTypeFactory<QuoteItem>.TryCreateInstance();
            quoteItem.InjectFrom<NullableAndEnumValueInjecter>(product);

            quoteItem.Id = null;
            quoteItem.ImageUrl = product.PrimaryImage != null ? product.PrimaryImage.Url : null;
            quoteItem.ListPrice = product.Price.ListPrice;
            quoteItem.ProductId = product.Id;
            quoteItem.SalePrice = product.Price.SalePrice;
            quoteItem.ProposalPrices.Add(new TierPrice(product.Price.SalePrice, quantity));
            quoteItem.SelectedTierPrice = quoteItem.ProposalPrices.First();

            return quoteItem;
        }

        public virtual PromotionProductEntry ToPromotionItem(Product product)
        {
            var promoItem = AbstractTypeFactory<PromotionProductEntry>.TryCreateInstance();
            promoItem.InjectFrom(product);

            if (product.Price != null)
            {
                promoItem.Discount = product.Price.DiscountAmount;
                promoItem.Price = product.Price.SalePrice;
            }

            promoItem.ProductId = product.Id;
            promoItem.Quantity = 1;
            promoItem.Variations = product.Variations.Select(ToPromotionItem).ToList();

            return promoItem;
        }
    }
}
