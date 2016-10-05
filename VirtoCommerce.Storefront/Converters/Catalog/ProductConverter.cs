using System;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Catalog.Factories;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;
using VirtoCommerce.Storefront.Model.Marketing.Factories;
using VirtoCommerce.Storefront.Model.Quote;
using VirtoCommerce.Storefront.Model.Quote.Factories;
using VirtoCommerce.Storefront.Model.Stores;
using catalogModel = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;
using searchModel = VirtoCommerce.Storefront.AutoRestClients.SearchApiModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class ProductStaticConverter
    {
        public static Product ToWebModel(this searchModel.Product productDto, Language currentLanguage, Currency currentCurrency, Store store)
        {
            return productDto.JsonConvert<catalogModel.Product>().ToWebModel(currentLanguage, currentCurrency, store);         
        }

        public static Product ToWebModel(this catalogModel.Product productDto, Language currentLanguage, Currency currentCurrency, Store store)
        {
            var converter = ServiceLocator.Current.GetInstance<ProductConverter>();
            return converter.ToWebModel(productDto, currentLanguage, currentCurrency, store);
        }

        public static QuoteItem ToQuoteItem(this Product product, long quantity)
        {
            var converter = ServiceLocator.Current.GetInstance<ProductConverter>();
            return converter.ToQuoteItem(product, quantity);
        }

        public static PromotionProductEntry ToPromotionItem(this Product product)
        {
            var converter = ServiceLocator.Current.GetInstance<ProductConverter>();
            return converter.ToPromotionItem(product);
        }
    }
   
    public class ProductConverter
    {       
        public virtual Product ToWebModel(searchModel.Product productDto, Language currentLanguage, Currency currentCurrency, Store store)
        {
            return ToWebModel(productDto.JsonConvert<catalogModel.Product>(), currentLanguage, currentCurrency, store);
        }

        public virtual Product ToWebModel(catalogModel.Product productDto, Language currentLanguage, Currency currentCurrency, Store store)
        {
            var retVal = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateProduct(currentCurrency, currentLanguage);
            retVal.InjectFrom<NullableAndEnumValueInjecter>(productDto);
            retVal.Weight = (decimal?)productDto.Weight;
            retVal.Height = (decimal?)productDto.Height;
            retVal.Width = (decimal?)productDto.Width;
            retVal.Length = (decimal?)productDto.Length;
            retVal.Sku = productDto.Code;
            retVal.VendorId = productDto.Vendor;
            retVal.Outline = productDto.Outlines.GetOutlinePath(store.Catalog);
            retVal.Url = "~/" + productDto.Outlines.GetSeoPath(store, currentLanguage, "product/" + productDto.Id);

            if (productDto.Properties != null)
            {
                retVal.Properties = productDto.Properties
                    .Where(x => string.Equals(x.Type, "Product", StringComparison.InvariantCultureIgnoreCase))
                    .Select(p => p.ToWebModel(currentLanguage))
                    .ToList();

                retVal.VariationProperties = productDto.Properties
                    .Where(x => string.Equals(x.Type, "Variation", StringComparison.InvariantCultureIgnoreCase))
                    .Select(p => p.ToWebModel(currentLanguage))
                    .ToList();
            }

            if (productDto.Images != null)
            {
                retVal.Images = productDto.Images.Select(i => i.ToWebModel()).ToArray();
                retVal.PrimaryImage = retVal.Images.FirstOrDefault(x => string.Equals(x.Url, productDto.ImgSrc, StringComparison.InvariantCultureIgnoreCase));
            }

            if (productDto.Assets != null)
            {
                retVal.Assets = productDto.Assets.Select(x => x.ToWebModel()).ToList();
            }

            if (productDto.Variations != null)
            {
                retVal.Variations = productDto.Variations.Select(v => ToWebModel(v, currentLanguage, currentCurrency, store)).ToList();
            }

            if (!productDto.Associations.IsNullOrEmpty())
            {
                retVal.Associations.AddRange(productDto.Associations.Select(x => x.ToWebModel()).Where(x => x != null));
            }

            var productSeoInfo = productDto.SeoInfos.GetBestMatchedSeoInfo(store, currentLanguage);
            if (productSeoInfo != null)
            {
                retVal.SeoInfo = productSeoInfo.ToWebModel();
            }
            else
            {
                retVal.SeoInfo = new SeoInfo
                {
                    Title = productDto.Id,
                    Language = currentLanguage,
                    Slug = productDto.Code
                };
            }
            if (productDto.Reviews != null)
            {
                retVal.Descriptions = productDto.Reviews.Select(r => new EditorialReview
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
            var retVal = ServiceLocator.Current.GetInstance<QuoteFactory>().CreateQuoteItem();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(product);

            retVal.Id = null;
            retVal.ImageUrl = product.PrimaryImage != null ? product.PrimaryImage.Url : null;
            retVal.ListPrice = product.Price.ListPrice;
            retVal.ProductId = product.Id;
            retVal.SalePrice = product.Price.SalePrice;
            retVal.ProposalPrices.Add(new TierPrice(product.Price.SalePrice, quantity));
            retVal.SelectedTierPrice = retVal.ProposalPrices.First();

            return retVal;
        }

        public virtual PromotionProductEntry ToPromotionItem(Product product)
        {
            var retVal = ServiceLocator.Current.GetInstance<MarketingFactory>().CreatePromotionProductEntry();
            retVal.InjectFrom(product);

            if (product.Price != null)
            {
                retVal.Discount = product.Price.DiscountAmount;
                retVal.Price = product.Price.SalePrice;
            }

            retVal.ProductId = product.Id;
            retVal.Quantity = 1;
            retVal.Variations = product.Variations.Select(ToPromotionItem).ToList();

            return retVal;
        }
    }
}
