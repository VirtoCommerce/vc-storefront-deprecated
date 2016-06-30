using System.Collections.Generic;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Stores;
using Xunit;
using catalogModel = VirtoCommerce.CatalogModule.Client.Model;

namespace VirtoCommerce.Storefront.Test
{

    [Trait("Category", "CI")]
    public class CategorySeoTests
    {
        private readonly Store _store = new Store
        {
            Id = "Store1",
            DefaultLanguage = new Language("en-US"),
            Languages = new List<Language>(new[]
            {
                new Language("en-US"),
            }),
            SeoLinksType = SeoLinksType.Long,
        };

        [Fact]
        public void When_HasNoSeoRecords_Expect_Null()
        {
            var category = new catalogModel.Category();

            var result = category.Outlines.GetSeoPath(_store, new Language("en-US"), null);
            Assert.Null(result);
        }

        [Fact]
        public void When_HasSeoRecords_Expect_ShortPath()
        {
            var category = new catalogModel.Category
            {
                Outlines = new List<catalogModel.Outline>
                {
                    new catalogModel.Outline
                    {
                        Items = new List<catalogModel.OutlineItem>
                        {
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Catalog",
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2" },
                                },
                            }
                        },
                    },
                },
            };

            var result = category.Outlines.GetSeoPath(_store, new Language("ru-RU"), null);
            Assert.Equal("category2", result);
        }

        [Fact]
        public void When_HasParentSeoRecords_Expect_LongPath()
        {
            var category = new catalogModel.Category
            {
                Outlines = new List<catalogModel.Outline>
                {
                    new catalogModel.Outline
                    {
                        Items = new List<catalogModel.OutlineItem>
                        {
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Catalog",
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "grandparent1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "grandparent2" },
                                }
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "parent1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "parent2" },
                                }
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2" },
                                },
                            },
                        },
                    },
                },
            };

            var result = category.Outlines.GetSeoPath(_store, new Language("ru-RU"), null);
            Assert.Equal("grandparent2/parent2/category2", result);
        }

        [Fact]
        public void When_MissingAnyParentSeoRecord_Expect_Null()
        {
            var category = new catalogModel.Category
            {
                Outlines = new List<catalogModel.Outline>
                {
                    new catalogModel.Outline
                    {
                        Items = new List<catalogModel.OutlineItem>
                        {
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Catalog",
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>(),
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "parent1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "parent2" },
                                }
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2" },
                                },
                            },
                        },
                    },
                },
            };

            var result = category.Outlines.GetSeoPath(_store, new Language("ru-RU"), null);
            Assert.Null(result);
        }
    }

    [Trait("Category", "CI")]
    public class ProductSeoTests
    {
        private readonly Store _store = new Store
        {
            Id = "Store1",
            DefaultLanguage = new Language("en-US"),
            Languages = new List<Language>(new[]
            {
                new Language("en-US"),
            }),
            SeoLinksType = SeoLinksType.Long,
        };

        [Fact]
        public void When_HasNoSeoRecords_Expect_Null()
        {
            var product = new catalogModel.Product
            {
                Category = new catalogModel.Category(),
            };

            var result = product.Outlines.GetSeoPath(_store, new Language("en-US"), null);
            Assert.Null(result);
        }

        [Fact]
        public void When_HasSeoRecordsAndNoCategorySeoRecords_Expect_Null()
        {
            var product = new catalogModel.Product
            {
                Outlines = new List<catalogModel.Outline>
                {
                    new catalogModel.Outline
                    {
                        Items = new List<catalogModel.OutlineItem>
                        {
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Catalog",
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "CatalogProduct",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "product2" },
                                },
                            },
                        },
                    },
                },
            };

            var result = product.Outlines.GetSeoPath(_store, new Language("ru-RU"), null);
            Assert.Null(result);
        }

        [Fact]
        public void When_HasCategorySeoRecords_Expect_LongPath()
        {
            var product = new catalogModel.Product
            {
                Outlines = new List<catalogModel.Outline>
                {
                    new catalogModel.Outline
                    {
                        Items = new List<catalogModel.OutlineItem>
                        {
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Catalog",
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2" },
                                },
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "CatalogProduct",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "product2" },
                                },
                            },
                        },
                    },
                },
            };

            var result = product.Outlines.GetSeoPath(_store, new Language("ru-RU"), null);
            Assert.Equal("category2/product2", result);
        }

        [Fact]
        public void When_HasParentCategorySeoRecords_Expect_LongPath()
        {
            var product = new catalogModel.Product
            {
                Outlines = new List<catalogModel.Outline>
                {
                    new catalogModel.Outline
                    {
                        Items = new List<catalogModel.OutlineItem>
                        {
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Catalog",
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "grandparent1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "grandparent2" },
                                }
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "parent1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "parent2" },
                                }
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2" },
                                },
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "CatalogProduct",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "product2" },
                                },
                            },
                        },
                    },
                },
            };

            var result = product.Outlines.GetSeoPath(_store, new Language("ru-RU"), null);
            Assert.Equal("grandparent2/parent2/category2/product2", result);
        }

        [Fact]
        public void When_MissingAnyParentSeoRecord_Expect_Null()
        {
            var product = new catalogModel.Product
            {
                Outlines = new List<catalogModel.Outline>
                {
                    new catalogModel.Outline
                    {
                        Items = new List<catalogModel.OutlineItem>
                        {
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Catalog",
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>(),
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "parent1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "parent2" },
                                }
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "category1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "category2" },
                                },
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "product1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "product2" },
                                },
                            },
                        },
                    },
                },
            };

            var result = product.Outlines.GetSeoPath(_store, new Language("ru-RU"), null);
            Assert.Null(result);
        }
    }
}
