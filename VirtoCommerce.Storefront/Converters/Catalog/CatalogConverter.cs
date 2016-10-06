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
using catalogDTO = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;
using searchDTO = VirtoCommerce.Storefront.AutoRestClients.SearchApiModuleApi.Models;
using coreDTO = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;

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


        public static Product ToProduct(this searchDTO.Product productDTO, Language currentLanguage, Currency currentCurrency, Store store)
        {
            return CatalogConverterInstance.ToProduct(productDTO, currentLanguage, currentCurrency, store);
        }

        public static Product ToProduct(this catalogDTO.Product productDTO, Language currentLanguage, Currency currentCurrency, Store store)
        {
            return CatalogConverterInstance.ToProduct(productDTO, currentLanguage, currentCurrency, store);
        }

   
        public static PromotionProductEntry ToPromotionItem(this Product product)
        {
            return CatalogConverterInstance.ToPromotionItem(product);
        }


        public static TaxLine[] ToTaxLines(this Product product)
        {
            return CatalogConverterInstance.ToTaxLines(product);
        }

        public static Image ToImage(this catalogDTO.Image imageDTO)
        {
            return CatalogConverterInstance.ToImage(imageDTO);
        }

        public static Asset ToAsset(this catalogDTO.Asset assetDTO)
        {
            return CatalogConverterInstance.ToAsset(assetDTO);
        }

        public static Category ToCategory(this searchDTO.Category categoryDTO, Language currentLanguage, Store store)
        {
            return CatalogConverterInstance.ToCategory(categoryDTO, currentLanguage, store);
        }

        public static Category ToCategory(this catalogDTO.Category categoryDto, Language currentLanguage, Store store)
        {
            return CatalogConverterInstance.ToCategory(categoryDto, currentLanguage, store);
        }

        public static Association ToAssociation(this catalogDTO.ProductAssociation associationDTO)
        {
            return CatalogConverterInstance.ToAssociation(associationDTO);
        }

        public static searchDTO.CategorySearch ToCategorySearchDto(this CategorySearchCriteria criteria, WorkContext workContext)
        {
            return CatalogConverterInstance.ToCategorySearchDTO(criteria, workContext);
        }

        public static searchDTO.ProductSearch ToProductSearchDTO(this ProductSearchCriteria criteria, WorkContext workContext)
        {
            return CatalogConverterInstance.ToProductSearchDTO(criteria, workContext);
        }

        public static CatalogProperty ToProperty(this catalogDTO.Property propertyDTO, Language currentLanguage)
        {
            return CatalogConverterInstance.ToProperty(propertyDTO, currentLanguage);
        }

        public static Aggregation ToAggregation(this searchDTO.Aggregation aggregationDTO, string currentLanguage)
        {
            return CatalogConverterInstance.ToAggregation(aggregationDTO, currentLanguage);
        }

        public static AggregationItem ToAggregationItem(this searchDTO.AggregationItem itemDto, string currentLanguage)
        {
            return CatalogConverterInstance.ToAggregationItem(itemDto, currentLanguage);
        }

        public static SeoInfo ToSeoInfo(this catalogDTO.SeoInfo seoDTO)
        {
            return CatalogConverterInstance.ToSeoInfo(seoDTO);
        }
    }
   
    public class CatalogConverter
    {
        public virtual SeoInfo ToSeoInfo(catalogDTO.SeoInfo seoDTO)
        {
            return seoDTO.JsonConvert<coreDTO.SeoInfo>().ToSeoInfo();
        }

        public virtual Aggregation ToAggregation(searchDTO.Aggregation aggregationDTO, string currentLanguage)
        {
            var result = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateAggregation();
            result.InjectFrom<NullableAndEnumValueInjecter>(aggregationDTO);

            if (aggregationDTO.Items != null)
            {
                result.Items = aggregationDTO.Items
                    .Select(i => i.ToAggregationItem(currentLanguage))
                    .ToArray();
            }

            if (aggregationDTO.Labels != null)
            {
                result.Label =
                    aggregationDTO.Labels.Where(l => string.Equals(l.Language, currentLanguage, StringComparison.OrdinalIgnoreCase))
                        .Select(l => l.Label)
                        .FirstOrDefault();
            }

            if (string.IsNullOrEmpty(result.Label))
            {
                result.Label = aggregationDTO.Field;
            }

            return result;
        }

        public virtual AggregationItem ToAggregationItem(searchDTO.AggregationItem itemDTO, string currentLanguage)
        {
            var result = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateAggregationItem();
            result.InjectFrom<NullableAndEnumValueInjecter>(itemDTO);

            if (itemDTO.Labels != null)
            {
                result.Label =
                    itemDTO.Labels.Where(l => string.Equals(l.Language, currentLanguage, StringComparison.OrdinalIgnoreCase))
                        .Select(l => l.Label)
                        .FirstOrDefault();
            }

            if (string.IsNullOrEmpty(result.Label) && itemDTO.Value != null)
            {
                result.Label = itemDTO.Value.ToString();
            }

            return result;
        }

        public virtual CatalogProperty ToProperty(catalogDTO.Property propertyDTO, Language currentLanguage)
        {
            var retVal = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateProperty();

            retVal.InjectFrom<NullableAndEnumValueInjecter>(propertyDTO);
            //Set display names and set current display name for requested language
            if (propertyDTO.DisplayNames != null)
            {
                retVal.DisplayNames = propertyDTO.DisplayNames.Select(x => new LocalizedString(new Language(x.LanguageCode), x.Name)).ToList();
                retVal.DisplayName = retVal.DisplayNames.FindWithLanguage(currentLanguage, x => x.Value, null);
            }

            //if display name for requested language not set get system property name
            if (string.IsNullOrEmpty(retVal.DisplayName))
            {
                retVal.DisplayName = propertyDTO.Name;
            }

            //For multilingual properties need populate LocalizedValues collection and set value for requested language
            if (propertyDTO.Multilanguage ?? false)
            {
                if (propertyDTO.Values != null)
                {
                    retVal.LocalizedValues = propertyDTO.Values.Where(x => x.Value != null).Select(x => new LocalizedString(new Language(x.LanguageCode), x.Value.ToString())).ToList();
                }
            }

            //Set property value
            if (propertyDTO.Values != null)
            {
                var propValue = propertyDTO.Values.FirstOrDefault(x => x.Value != null);
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

        public virtual searchDTO.ProductSearch ToProductSearchDTO(ProductSearchCriteria criteria, WorkContext workContext)
        {
            var result = new searchDTO.ProductSearch()
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

        public virtual searchDTO.CategorySearch ToCategorySearchDTO(CategorySearchCriteria criteria, WorkContext workContext)
        {
            var result = new searchDTO.CategorySearch()
            {
                Skip = criteria.Start,
                Take = criteria.PageSize,
                Outline = criteria.Outline
            };

            if (criteria.SortBy != null)
                result.Sort = new string[] { criteria.SortBy };

            return result;
        }

        public virtual Association ToAssociation(catalogDTO.ProductAssociation associationDTO)
        {
            Association retVal = null;
            if (associationDTO.AssociatedObjectType.EqualsInvariant("product"))
            {
                retVal = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateProductAssociation(associationDTO.AssociatedObjectId);
            }
            else if (associationDTO.AssociatedObjectType.EqualsInvariant("category"))
            {
                retVal = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateCategoryAssociation(associationDTO.AssociatedObjectId);
            }
            if (retVal != null)
            {
                retVal.InjectFrom<NullableAndEnumValueInjecter>(associationDTO);
                retVal.Image = new Image { Url = associationDTO.AssociatedObjectImg };
            }

            return retVal;
        }

        public virtual Category ToCategory(searchDTO.Category categoryDTO, Language currentLanguage, Store store)
        {
            return ToCategory(categoryDTO.JsonConvert<catalogDTO.Category>(), currentLanguage, store);
        }

        public virtual Category ToCategory(catalogDTO.Category categoryDTO, Language currentLanguage, Store store)
        {
            var result = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateCategory();
            result.InjectFrom<NullableAndEnumValueInjecter>(categoryDTO);

            if(!categoryDTO.SeoInfos.IsNullOrEmpty())
            {
                var seoInfoDTO = categoryDTO.SeoInfos.Select(x => x.JsonConvert<coreDTO.SeoInfo>())
                                            .GetBestMatchedSeoInfo(store, currentLanguage);
                if (seoInfoDTO != null)
                {
                    result.SeoInfo = seoInfoDTO.ToSeoInfo();
                }
            }
 
            if (result.SeoInfo == null)
            {
                result.SeoInfo = new SeoInfo
                {
                    Slug = categoryDTO.Id,
                    Title = categoryDTO.Name
                };
            }

            result.Url = "~/" + categoryDTO.Outlines.GetSeoPath(store, currentLanguage, "category/" + categoryDTO.Id);
            result.Outline = categoryDTO.Outlines.GetOutlinePath(store.Catalog);

            if (categoryDTO.Images != null)
            {
                result.Images = categoryDTO.Images.Select(i => i.ToImage()).ToArray();
                result.PrimaryImage = result.Images.FirstOrDefault();
            }

            if (categoryDTO.Properties != null)
            {
                result.Properties = categoryDTO.Properties
                    .Where(x => string.Equals(x.Type, "Category", StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.ToProperty(currentLanguage))
                    .ToList();
            }

            return result;
        }

        public virtual Image ToImage(catalogDTO.Image imageDTO)
        {
            var result = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateImage();
            result.InjectFrom<NullableAndEnumValueInjecter>(imageDTO);
            return result;
        }

        public virtual Asset ToAsset(catalogDTO.Asset assetDTO)
        {
            var result = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateAsset();
            result.InjectFrom<NullableAndEnumValueInjecter>(assetDTO);
            return result;
        }

        public virtual Product ToProduct(searchDTO.Product productDTO, Language currentLanguage, Currency currentCurrency, Store store)
        {
            return ToProduct(productDTO.JsonConvert<catalogDTO.Product>(), currentLanguage, currentCurrency, store);
        }

        public virtual Product ToProduct(catalogDTO.Product productDTO, Language currentLanguage, Currency currentCurrency, Store store)
        {
            var retVal = ServiceLocator.Current.GetInstance<CatalogFactory>().CreateProduct(currentCurrency, currentLanguage);
            retVal.InjectFrom<NullableAndEnumValueInjecter>(productDTO);
            retVal.Weight = (decimal?)productDTO.Weight;
            retVal.Height = (decimal?)productDTO.Height;
            retVal.Width = (decimal?)productDTO.Width;
            retVal.Length = (decimal?)productDTO.Length;
            retVal.Sku = productDTO.Code;
            retVal.VendorId = productDTO.Vendor;
            retVal.Outline = productDTO.Outlines.GetOutlinePath(store.Catalog);
            retVal.Url = "~/" + productDTO.Outlines.GetSeoPath(store, currentLanguage, "product/" + productDTO.Id);

            if (productDTO.Properties != null)
            {
                retVal.Properties = productDTO.Properties
                    .Where(x => string.Equals(x.Type, "Product", StringComparison.InvariantCultureIgnoreCase))
                    .Select(p => p.ToProperty(currentLanguage))
                    .ToList();

                retVal.VariationProperties = productDTO.Properties
                    .Where(x => string.Equals(x.Type, "Variation", StringComparison.InvariantCultureIgnoreCase))
                    .Select(p => p.ToProperty(currentLanguage))
                    .ToList();
            }

            if (productDTO.Images != null)
            {
                retVal.Images = productDTO.Images.Select(ToImage).ToArray();
                retVal.PrimaryImage = retVal.Images.FirstOrDefault(x => string.Equals(x.Url, productDTO.ImgSrc, StringComparison.InvariantCultureIgnoreCase));
            }

            if (productDTO.Assets != null)
            {
                retVal.Assets = productDTO.Assets.Select(ToAsset).ToList();
            }

            if (productDTO.Variations != null)
            {
                retVal.Variations = productDTO.Variations.Select(v => ToProduct(v, currentLanguage, currentCurrency, store)).ToList();
            }

            if (!productDTO.Associations.IsNullOrEmpty())
            {
                retVal.Associations.AddRange(productDTO.Associations.Select(ToAssociation).Where(x => x != null));
            }

            if(!productDTO.SeoInfos.IsNullOrEmpty())
            {
                var seoInfoDTO = productDTO.SeoInfos.Select(x=> x.JsonConvert<coreDTO.SeoInfo>())
                                            .GetBestMatchedSeoInfo(store, currentLanguage);
                if(seoInfoDTO != null)
                {
                    retVal.SeoInfo = seoInfoDTO.ToSeoInfo();
                }
            }

            if (retVal.SeoInfo == null)
            {
                retVal.SeoInfo = new SeoInfo
                {
                    Title = productDTO.Id,
                    Language = currentLanguage,
                    Slug = productDTO.Code
                };
            }

            if (productDTO.Reviews != null)
            {
                retVal.Descriptions = productDTO.Reviews.Select(r => new EditorialReview
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
