using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Catalog.Factories;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;
using VirtoCommerce.Storefront.Model.Marketing.Factories;
using VirtoCommerce.Storefront.Model.Stores;
using catalogDto = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;
using coreDto = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;
using searchDto = VirtoCommerce.Storefront.AutoRestClients.SearchApiModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class CatalogConverterExtension
    {
        public static CatalogConverter CatalogConverterInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CatalogConverter>();
            }
        }


        public static Product ToProduct(this searchDto.Product productDto, Language currentLanguage, Currency currentCurrency, Store store)
        {
            return CatalogConverterInstance.ToProduct(productDto, currentLanguage, currentCurrency, store);
        }

        public static Product ToProduct(this catalogDto.Product productDto, Language currentLanguage, Currency currentCurrency, Store store)
        {
            return CatalogConverterInstance.ToProduct(productDto, currentLanguage, currentCurrency, store);
        }


        public static PromotionProductEntry ToPromotionItem(this Product product)
        {
            return CatalogConverterInstance.ToPromotionItem(product);
        }


        public static TaxLine[] ToTaxLines(this Product product)
        {
            return CatalogConverterInstance.ToTaxLines(product);
        }

        public static Image ToImage(this catalogDto.Image imageDto)
        {
            return CatalogConverterInstance.ToImage(imageDto);
        }

        public static Asset ToAsset(this catalogDto.Asset assetDto)
        {
            return CatalogConverterInstance.ToAsset(assetDto);
        }

        public static Category ToCategory(this searchDto.Category categoryDto, Language currentLanguage, Store store)
        {
            return CatalogConverterInstance.ToCategory(categoryDto, currentLanguage, store);
        }

        public static Category ToCategory(this catalogDto.Category categoryDto, Language currentLanguage, Store store)
        {
            return CatalogConverterInstance.ToCategory(categoryDto, currentLanguage, store);
        }

        public static Association ToAssociation(this catalogDto.ProductAssociation associationDto)
        {
            return CatalogConverterInstance.ToAssociation(associationDto);
        }

        public static searchDto.CategorySearch ToCategorySearchDto(this CategorySearchCriteria criteria, WorkContext workContext)
        {
            return CatalogConverterInstance.ToCategorySearchDto(criteria, workContext);
        }

        public static searchDto.ProductSearch ToProductSearchDto(this ProductSearchCriteria criteria, WorkContext workContext)
        {
            return CatalogConverterInstance.ToProductSearchDto(criteria, workContext);
        }

        public static CatalogProperty ToProperty(this catalogDto.Property propertyDto, Language currentLanguage)
        {
            return CatalogConverterInstance.ToProperty(propertyDto, currentLanguage);
        }

        public static Aggregation ToAggregation(this searchDto.Aggregation aggregationDto, string currentLanguage)
        {
            return CatalogConverterInstance.ToAggregation(aggregationDto, currentLanguage);
        }

        public static AggregationItem ToAggregationItem(this searchDto.AggregationItem itemDto, string currentLanguage)
        {
            return CatalogConverterInstance.ToAggregationItem(itemDto, currentLanguage);
        }

        public static SeoInfo ToSeoInfo(this catalogDto.SeoInfo seoDto)
        {
            return CatalogConverterInstance.ToSeoInfo(seoDto);
        }
    }

    public class CatalogConverter
    {
        public virtual SeoInfo ToSeoInfo(catalogDto.SeoInfo seoDto)
        {
            return seoDto.JsonConvert<coreDto.SeoInfo>().ToSeoInfo();
        }

        public virtual Aggregation ToAggregation(searchDto.Aggregation aggregationDto, string currentLanguage)
        {
            var result = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateAggregation();
            result.InjectFrom<NullableAndEnumValueInjecter>(aggregationDto);

            if (aggregationDto.Items != null)
            {
                result.Items = aggregationDto.Items
                    .Select(i => i.ToAggregationItem(currentLanguage))
                    .ToArray();
            }

            if (aggregationDto.Labels != null)
            {
                result.Label =
                    aggregationDto.Labels.Where(l => string.Equals(l.Language, currentLanguage, StringComparison.OrdinalIgnoreCase))
                        .Select(l => l.Label)
                        .FirstOrDefault();
            }

            if (string.IsNullOrEmpty(result.Label))
            {
                result.Label = aggregationDto.Field;
            }

            return result;
        }

        public virtual AggregationItem ToAggregationItem(searchDto.AggregationItem itemDto, string currentLanguage)
        {
            var result = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateAggregationItem();
            result.InjectFrom<NullableAndEnumValueInjecter>(itemDto);

            if (itemDto.Labels != null)
            {
                result.Label =
                    itemDto.Labels.Where(l => string.Equals(l.Language, currentLanguage, StringComparison.OrdinalIgnoreCase))
                        .Select(l => l.Label)
                        .FirstOrDefault();
            }

            if (string.IsNullOrEmpty(result.Label) && itemDto.Value != null)
            {
                result.Label = itemDto.Value.ToString();
            }

            return result;
        }

        public virtual CatalogProperty ToProperty(catalogDto.Property propertyDto, Language currentLanguage)
        {
            var retVal = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateProperty();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(propertyDto);
            //Set display names and set current display name for requested language
            if (propertyDto.DisplayNames != null)
            {
                retVal.DisplayNames = propertyDto.DisplayNames.Select(x => new LocalizedString(new Language(x.LanguageCode), x.Name)).ToList();
                retVal.DisplayName = retVal.DisplayNames.FindWithLanguage(currentLanguage, x => x.Value, null);
            }

            //if display name for requested language not set get system property name
            if (string.IsNullOrEmpty(retVal.DisplayName))
            {
                retVal.DisplayName = propertyDto.Name;
            }

            //For multilingual properties need populate LocalizedValues collection and set value for requested language
            if (propertyDto.Multilanguage ?? false)
            {
                if (propertyDto.Values != null)
                {
                    retVal.LocalizedValues = propertyDto.Values.Where(x => x.Value != null).Select(x => new LocalizedString(new Language(x.LanguageCode), x.Value.ToString())).ToList();
                }
            }

            //Set property value
            if (propertyDto.Values != null)
            {
                var propValue = propertyDto.Values.FirstOrDefault(x => x.Value != null);
                if (propValue != null)
                {
                    //Use only one prop value (reserve multi-values to other scenarios)
                    retVal.Value = propValue.Value;
                    retVal.ValueId = propValue.ValueId;
                }
            }

            //Try to set value for requested language
            if (retVal.LocalizedValues.Any())
            {
                retVal.Value = retVal.LocalizedValues.FindWithLanguage(currentLanguage, x => x.Value, retVal.Value);
            }

            return retVal;
        }

        public virtual searchDto.ProductSearch ToProductSearchDto(ProductSearchCriteria criteria, WorkContext workContext)
        {
            var result = new searchDto.ProductSearch()
            {
                SearchPhrase = criteria.Keyword,
                Outline = criteria.Outline,
                Currency = criteria.Currency == null ? workContext.CurrentCurrency.Code : criteria.Currency.Code,
                Terms = criteria.Terms.ToStrings(),
                PriceLists = workContext.CurrentPricelists.Where(p => p.Currency.Equals(workContext.CurrentCurrency)).Select(p => p.Id).ToList(),
                Skip = criteria.Start,
                Take = criteria.PageSize
            };

            // Add vendor id to terms
            if (!string.IsNullOrEmpty(criteria.VendorId))
            {
                if (result.Terms == null)
                {
                    result.Terms = new List<string>();
                }

                result.Terms.Add(string.Format("vendor:{0}", criteria.VendorId));
            }

            if (criteria.SortBy != null)
                result.Sort = new string[] { criteria.SortBy };

            return result;
        }

        public virtual searchDto.CategorySearch ToCategorySearchDto(CategorySearchCriteria criteria, WorkContext workContext)
        {
            var result = new searchDto.CategorySearch()
            {
                Skip = criteria.Start,
                Take = criteria.PageSize,
                Outline = criteria.Outline
            };

            if (criteria.SortBy != null)
                result.Sort = new string[] { criteria.SortBy };

            return result;
        }

        public virtual Association ToAssociation(catalogDto.ProductAssociation associationDto)
        {
            Association retVal = null;
            if (associationDto.AssociatedObjectType.EqualsInvariant("product"))
            {
                retVal = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateProductAssociation(associationDto.AssociatedObjectId);
            }
            else if (associationDto.AssociatedObjectType.EqualsInvariant("category"))
            {
                retVal = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateCategoryAssociation(associationDto.AssociatedObjectId);
            }
            if (retVal != null)
            {
                retVal.InjectFrom<NullableAndEnumValueInjecter>(associationDto);
                retVal.Image = new Image { Url = associationDto.AssociatedObjectImg };
            }

            return retVal;
        }

        public virtual Category ToCategory(searchDto.Category categoryDto, Language currentLanguage, Store store)
        {
            return ToCategory(categoryDto.JsonConvert<catalogDto.Category>(), currentLanguage, store);
        }

        public virtual Category ToCategory(catalogDto.Category categoryDto, Language currentLanguage, Store store)
        {
            var result = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateCategory();
            result.InjectFrom<NullableAndEnumValueInjecter>(categoryDto);

            if (!categoryDto.SeoInfos.IsNullOrEmpty())
            {
                var seoInfoDto = categoryDto.SeoInfos.Select(x => x.JsonConvert<coreDto.SeoInfo>())
                    .GetBestMatchingSeoInfos(store, currentLanguage)
                    .FirstOrDefault();

                if (seoInfoDto != null)
                {
                    result.SeoInfo = seoInfoDto.ToSeoInfo();
                }
            }

            if (result.SeoInfo == null)
            {
                result.SeoInfo = new SeoInfo
                {
                    Slug = categoryDto.Id,
                    Title = categoryDto.Name
                };
            }

            result.Url = "~/" + categoryDto.Outlines.GetSeoPath(store, currentLanguage, "category/" + categoryDto.Id);
            result.Outline = categoryDto.Outlines.GetOutlinePath(store.Catalog);

            if (categoryDto.Images != null)
            {
                result.Images = categoryDto.Images.Select(i => i.ToImage()).ToArray();
                result.PrimaryImage = result.Images.FirstOrDefault();
            }

            if (categoryDto.Properties != null)
            {
                result.Properties = categoryDto.Properties
                    .Where(x => string.Equals(x.Type, "Category", StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.ToProperty(currentLanguage))
                    .ToList();
            }

            return result;
        }

        public virtual Image ToImage(catalogDto.Image imageDto)
        {
            var result = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateImage();
            result.InjectFrom<NullableAndEnumValueInjecter>(imageDto);
            return result;
        }

        public virtual Asset ToAsset(catalogDto.Asset assetDto)
        {
            var result = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateAsset();
            result.InjectFrom<NullableAndEnumValueInjecter>(assetDto);
            return result;
        }

        public virtual Product ToProduct(searchDto.Product productDto, Language currentLanguage, Currency currentCurrency, Store store)
        {
            return ToProduct(productDto.JsonConvert<catalogDto.Product>(), currentLanguage, currentCurrency, store);
        }

        public virtual Product ToProduct(catalogDto.Product productDto, Language currentLanguage, Currency currentCurrency, Store store)
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
                    .Select(p => p.ToProperty(currentLanguage))
                    .ToList();

                retVal.VariationProperties = productDto.Properties
                    .Where(x => string.Equals(x.Type, "Variation", StringComparison.InvariantCultureIgnoreCase))
                    .Select(p => p.ToProperty(currentLanguage))
                    .ToList();
            }

            if (productDto.Images != null)
            {
                retVal.Images = productDto.Images.Select(ToImage).ToArray();
                retVal.PrimaryImage = retVal.Images.FirstOrDefault(x => string.Equals(x.Url, productDto.ImgSrc, StringComparison.InvariantCultureIgnoreCase));
            }

            if (productDto.Assets != null)
            {
                retVal.Assets = productDto.Assets.Select(ToAsset).ToList();
            }

            if (productDto.Variations != null)
            {
                retVal.Variations = productDto.Variations.Select(v => ToProduct(v, currentLanguage, currentCurrency, store)).ToList();
            }

            if (!productDto.Associations.IsNullOrEmpty())
            {
                retVal.Associations.AddRange(productDto.Associations.Select(ToAssociation).Where(x => x != null));
            }

            if (!productDto.SeoInfos.IsNullOrEmpty())
            {
                var seoInfoDto = productDto.SeoInfos.Select(x => x.JsonConvert<coreDto.SeoInfo>())
                    .GetBestMatchingSeoInfos(store, currentLanguage)
                    .FirstOrDefault();

                if (seoInfoDto != null)
                {
                    retVal.SeoInfo = seoInfoDto.ToSeoInfo();
                }
            }

            if (retVal.SeoInfo == null)
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

        public virtual TaxLine[] ToTaxLines(Product product)
        {
            var retVal = new List<TaxLine>
            {
                new TaxLine(product.Currency)
                {
                    Id = product.Id,
                    Code = product.Sku,
                    Name = product.Name,
                    TaxType = product.TaxType,
                    Amount =  product.Price.ActualPrice
                }
            };

            //Need generate tax line for each tier price
            foreach (var tierPrice in product.Price.TierPrices)
            {
                retVal.Add(new TaxLine(tierPrice.Price.Currency)
                {
                    Id = product.Id,
                    Code = product.Sku,
                    Name = product.Name,
                    TaxType = product.TaxType,
                    Quantity = tierPrice.Quantity,
                    Amount = tierPrice.Price
                });
            }
            return retVal.ToArray();
        }

    }
}
