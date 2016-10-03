using System;
using System.Linq;
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
    public class ProductConverter
    {
        protected Func<Product> ProductFactory { get; set; }

        public ProductConverter(Func<Product> productFactory)
        {
            ProductFactory = productFactory;
        }

        public virtual Product ToWebModel(searchModel.Product product, Language currentLanguage, Currency currentCurrency, Store store)
        {
            return ToWebModel(product.JsonConvert<catalogModel.Product>(), currentLanguage, currentCurrency, store);
        }

        public virtual Product ToWebModel(catalogModel.Product product, Language currentLanguage, Currency currentCurrency, Store store)
        {
            var retVal = ProductFactory();
            retVal.InjectFrom<NullableAndEnumValueInjecter>(product);

            retVal.Currency = currentCurrency;
            retVal.Price = new ProductPrice(currentCurrency);
            retVal.Weight = (decimal?)product.Weight;
            retVal.Height = (decimal?)product.Height;
            retVal.Width = (decimal?)product.Width;
            retVal.Length = (decimal?)product.Length;
            retVal.Sku = product.Code;
            retVal.VendorId = product.Vendor;
            retVal.Outline = product.Outlines.GetOutlinePath(store.Catalog);
            retVal.Url = "~/" + product.Outlines.GetSeoPath(store, currentLanguage, "product/" + product.Id);

            if (product.Properties != null)
            {
                retVal.Properties = product.Properties
                    .Where(x => string.Equals(x.Type, "Product", StringComparison.InvariantCultureIgnoreCase))
                    .Select(p => p.ToWebModel(currentLanguage))
                    .ToList();

                retVal.VariationProperties = product.Properties
                    .Where(x => string.Equals(x.Type, "Variation", StringComparison.InvariantCultureIgnoreCase))
                    .Select(p => p.ToWebModel(currentLanguage))
                    .ToList();
            }

            if (product.Images != null)
            {
                retVal.Images = product.Images.Select(i => i.ToWebModel()).ToArray();
                retVal.PrimaryImage = retVal.Images.FirstOrDefault(x => string.Equals(x.Url, product.ImgSrc, StringComparison.InvariantCultureIgnoreCase));
            }

            if (product.Assets != null)
            {
                retVal.Assets = product.Assets.Select(x => x.ToWebModel()).ToList();
            }

            if (product.Variations != null)
            {
                retVal.Variations = product.Variations.Select(v => ToWebModel(v, currentLanguage, currentCurrency, store)).ToList();
            }

            if (!product.Associations.IsNullOrEmpty())
            {
                retVal.Associations.AddRange(product.Associations.Select(x => x.ToWebModel()).Where(x => x != null));
            }

            var productSeoInfo = product.SeoInfos.GetBestMatchedSeoInfo(store, currentLanguage);
            if (productSeoInfo != null)
            {
                retVal.SeoInfo = productSeoInfo.ToWebModel();
            }

            if (product.Reviews != null)
            {
                retVal.Descriptions = product.Reviews.Select(r => new EditorialReview
                {
                    Language = new Language(r.LanguageCode),
                    ReviewType = r.ReviewType,
                    Value = r.Content
                }).Where(x => x.Language.Equals(currentLanguage)).ToList();
                retVal.Description = retVal.Descriptions.FindWithLanguage(currentLanguage, x => x.Value, null);
            }

            return retVal;
        }

        public virtual QuoteItem ToQuoteItem(Product product, long quantity)
        {
            var quoteItem = new QuoteItem();

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
            var promoItem = new PromotionProductEntry();

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
