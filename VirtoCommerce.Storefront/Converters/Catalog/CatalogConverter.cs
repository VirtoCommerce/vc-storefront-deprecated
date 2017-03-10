using System;
using System.Collections.Generic;
using System.Linq;
using Markdig;
using Microsoft.Practices.ServiceLocation;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Stores;
using catalogDto = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;
using coreDto = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;
using marketingDto = VirtoCommerce.Storefront.AutoRestClients.MarketingModuleApi.Models;
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

        public static marketingDto.ProductPromoEntry ToProductPromoEntryDto(this Product product)
        {
            return CatalogConverterInstance.ToProductPromoEntryDto(product);
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
        private readonly MarkdownPipeline _markdownPipeline;

        public CatalogConverter()
        {
            _markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        }

        public virtual SeoInfo ToSeoInfo(catalogDto.SeoInfo seoDto)
        {
            return seoDto.JsonConvert<coreDto.SeoInfo>().ToSeoInfo();
        }

        public virtual Aggregation ToAggregation(searchDto.Aggregation aggregationDto, string currentLanguage)
        {
            var result = new Aggregation();
            result.AggregationType = aggregationDto.AggregationType;
            result.Field = aggregationDto.Field;

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
            var result = new AggregationItem();
            result.Value = itemDto.Value;
            result.IsApplied = itemDto.IsApplied ?? false;
            result.Count = itemDto.Count ?? 0;

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
            var retVal = new CatalogProperty();

            retVal.Id = propertyDto.Id;
            retVal.Name = propertyDto.Name;
            retVal.Type = propertyDto.Type;
            retVal.ValueType = propertyDto.ValueType;

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
            var result = new searchDto.ProductSearch
            {
                Locale = criteria.Language != null ? criteria.Language.CultureName : workContext.CurrentLanguage.CultureName,
                SearchPhrase = criteria.Keyword,
                Outline = criteria.Outline,
                Currency = criteria.Currency == null ? workContext.CurrentCurrency.Code : criteria.Currency.Code,
                Terms = criteria.Terms.ToStrings(),
                PriceLists = workContext.CurrentPricelists.Where(p => p.Currency.Equals(workContext.CurrentCurrency)).Select(p => p.Id).ToList(),
                Skip = criteria.Start,
                Take = criteria.PageSize,
                ResponseGroup = ((int)criteria.ResponseGroup).ToString()
            };

            // Add vendor id to terms
            if (!string.IsNullOrEmpty(criteria.VendorId))
            {
                if (result.Terms == null)
                {
                    result.Terms = new List<string>();
                }

                result.Terms.Add(string.Concat("vendor:", criteria.VendorId));
            }

            if (criteria.SortBy != null)
                result.Sort = new[] { criteria.SortBy };

            return result;
        }

        public virtual searchDto.CategorySearch ToCategorySearchDto(CategorySearchCriteria criteria, WorkContext workContext)
        {
            var result = new searchDto.CategorySearch
            {
                Skip = criteria.Start,
                Take = criteria.PageSize,
                Outline = criteria.Outline,
                ResponseGroup = ((int)criteria.ResponseGroup).ToString()
            };

            if (criteria.SortBy != null)
                result.Sort = new[] { criteria.SortBy };

            return result;
        }

        public virtual Association ToAssociation(catalogDto.ProductAssociation associationDto)
        {
            Association retVal = null;

            if (associationDto.AssociatedObjectType.EqualsInvariant("product"))
            {
                retVal = new ProductAssociation
                {
                    ProductId = associationDto.AssociatedObjectId
                };

            }
            else if (associationDto.AssociatedObjectType.EqualsInvariant("category"))
            {
                retVal = new CategoryAssociation
                {
                    CategoryId = associationDto.AssociatedObjectId
                };
            }

            if (retVal != null)
            {
                retVal.Type = associationDto.Type;
                retVal.Priority = associationDto.Priority ?? 0;
                retVal.Image = new Image { Url = associationDto.AssociatedObjectImg };
                retVal.Quantity = associationDto.Quantity;
            }

            return retVal;
        }

        public virtual Category ToCategory(searchDto.Category categoryDto, Language currentLanguage, Store store)
        {
            return ToCategory(categoryDto.JsonConvert<catalogDto.Category>(), currentLanguage, store);
        }

        public virtual Category ToCategory(catalogDto.Category categoryDto, Language currentLanguage, Store store)
        {
            var result = new Category();
            result.Id = categoryDto.Id;
            result.CatalogId = categoryDto.CatalogId;
            result.Code = categoryDto.Code;
            result.Name = categoryDto.Name;
            result.ParentId = categoryDto.ParentId;
            result.TaxType = categoryDto.TaxType;

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

            result.Outline = categoryDto.Outlines.GetOutlinePath(store.Catalog);
            result.SeoPath = categoryDto.Outlines.GetSeoPath(store, currentLanguage, null);
            result.Url = "~/" + (result.SeoPath ?? "category/" + categoryDto.Id);

            if (categoryDto.Images != null)
            {
                result.Images = categoryDto.Images.Select(ToImage).ToArray();
                result.PrimaryImage = result.Images.FirstOrDefault();
            }

            if (categoryDto.Properties != null)
            {
                result.Properties = categoryDto.Properties
                    .Where(x => string.Equals(x.Type, "Category", StringComparison.OrdinalIgnoreCase))
                    .Select(p => ToProperty(p, currentLanguage))
                    .ToList();
            }

            return result;
        }

        public virtual Image ToImage(catalogDto.Image imageDto)
        {
            var result = new Image();
            result.Url = imageDto.Url.RemoveLeadingUriScheme();
            return result;
        }

        public virtual Asset ToAsset(catalogDto.Asset assetDto)
        {
            var result = new Asset();
            result.Url = assetDto.Url.RemoveLeadingUriScheme();
            result.TypeId = assetDto.TypeId;
            result.Size = assetDto.Size;
            result.Name = assetDto.Name;
            result.MimeType = assetDto.MimeType;
            result.Group = assetDto.Group;
            return result;
        }

        public virtual Product ToProduct(searchDto.Product productDto, Language currentLanguage, Currency currentCurrency, Store store)
        {
            return ToProduct(productDto.JsonConvert<catalogDto.Product>(), currentLanguage, currentCurrency, store);
        }

        public virtual Product ToProduct(catalogDto.Product productDto, Language currentLanguage, Currency currentCurrency, Store store)
        {
            var retVal = new Product(currentCurrency, currentLanguage);
            retVal.Id = productDto.Id;
            retVal.CatalogId = productDto.CatalogId;
            retVal.CategoryId = productDto.CategoryId;
            retVal.DownloadExpiration = productDto.DownloadExpiration;
            retVal.DownloadType = productDto.DownloadType;
            retVal.EnableReview = productDto.EnableReview ?? false;
            retVal.Gtin = productDto.Gtin;
            retVal.HasUserAgreement = productDto.HasUserAgreement ?? false;
            retVal.IsActive = productDto.IsActive ?? false;
            retVal.IsBuyable = productDto.IsBuyable ?? false;
            retVal.ManufacturerPartNumber = productDto.ManufacturerPartNumber;
            retVal.MaxNumberOfDownload = productDto.MaxNumberOfDownload ?? 0;
            retVal.MaxQuantity = productDto.MaxQuantity ?? 0;
            retVal.MeasureUnit = productDto.MeasureUnit;
            retVal.MinQuantity = productDto.MinQuantity ?? 0;
            retVal.Name = productDto.Name;
            retVal.Outline = productDto.Outline;
            retVal.PackageType = productDto.PackageType;
            retVal.ProductType = productDto.ProductType;
            retVal.ShippingType = productDto.ShippingType;
            retVal.TaxType = productDto.TaxType;
            retVal.TrackInventory = productDto.TrackInventory ?? false;
            retVal.VendorId = productDto.Vendor;
            retVal.WeightUnit = productDto.WeightUnit;
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
                    .Select(p => ToProperty(p, currentLanguage))
                    .ToList();

                retVal.VariationProperties = productDto.Properties
                    .Where(x => string.Equals(x.Type, "Variation", StringComparison.InvariantCultureIgnoreCase))
                    .Select(p => ToProperty(p, currentLanguage))
                    .ToList();
            }

            if (productDto.Images != null)
            {
                retVal.Images = productDto.Images.Select(ToImage).ToArray();
                retVal.PrimaryImage = retVal.Images.FirstOrDefault();
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
                retVal.Descriptions = productDto.Reviews.Where(r => !string.IsNullOrEmpty(r.Content)).Select(r => new EditorialReview
                {
                    Language = new Language(r.LanguageCode),
                    ReviewType = r.ReviewType,
                    Value = Markdown.ToHtml(r.Content, _markdownPipeline)
                }).Where(x => x.Language.Equals(currentLanguage)).ToList();
                retVal.Description = retVal.Descriptions.FindWithLanguage(currentLanguage, x => x.Value, null);
            }

            return retVal;
        }


        public virtual marketingDto.ProductPromoEntry ToProductPromoEntryDto(Product product)
        {
            var retVal = new marketingDto.ProductPromoEntry();
            retVal.CatalogId = product.CatalogId;
            retVal.CategoryId = product.CategoryId;
            retVal.Outline = product.Outline;

            if (product.Price != null)
            {
                retVal.Discount = (double)product.Price.DiscountAmount.Amount;
                retVal.Price = (double)product.Price.SalePrice.Amount;
            }

            retVal.ProductId = product.Id;
            retVal.Quantity = 1;
            retVal.Variations = product.Variations?.Select(ToProductPromoEntryDto).ToList();

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
                    //Special case when product have 100% discount and need to calculate tax for old value
                    Amount =  product.Price.ActualPrice.Amount > 0 ? product.Price.ActualPrice : product.Price.SalePrice
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
