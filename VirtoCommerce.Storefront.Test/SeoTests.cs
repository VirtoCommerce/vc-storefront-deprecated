using System.Collections.Generic;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Stores;
using Xunit;
using catalogModel = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;

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
            SeoLinksType = SeoLinksType.Collapsed,
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

        [Fact]
        public void When_HasInactiveSeoRecords_Expect_OnlyActiveRecords()
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
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "inactive-parent1", IsActive = false },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "active-parent1", IsActive = true },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "inactive-parent2", IsActive = false },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "active-parent2", IsActive = true },
                                }
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "inactive-category1", IsActive = false },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "active-category1", IsActive = true },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "inactive-category2", IsActive = false },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "active-category2", IsActive = true },
                                },
                            },
                        },
                    },
                },
            };

            var result = category.Outlines.GetSeoPath(_store, new Language("ru-RU"), null);
            Assert.Equal("active-parent2/active-category2", result);
        }

        [Fact]
        public void When_HasVirtualParent_Expect_SkipLinkedPhysicalParent()
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
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "virtual-parent1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "virtual-parent2" },
                                }
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                HasVirtualParent = true,
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "physical-parent1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "physical-parent2" },
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
            Assert.Equal("virtual-parent2/category2", result);
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
            SeoLinksType = SeoLinksType.Collapsed,
        };

        [Fact]
        public void When_HasNoSeoRecords_Expect_Null()
        {
            var product = new catalogModel.Product
            {
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
        public void When_ProductHasParentCategoryWithVirtualParent_Expect_SkipLinkedPhysicalParent()
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
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "virtual-parent1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "virtual-parent2" },
                                }
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "Category",
                                HasVirtualParent = true,
                                SeoInfos = new List<catalogModel.SeoInfo>
                                {
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "parent1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "parent2" },
                                }
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

            var result = category.Outlines.GetSeoPath(_store, new Language("ru-RU"), null);
            Assert.Equal("virtual-parent2/product2", result);
        }

        [Fact]
        public void When_ProductHasVirtualParent_Expect_KeepProduct()
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
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "en-US", SemanticUrl = "virtual-parent1" },
                                    new catalogModel.SeoInfo { StoreId = "Store1", LanguageCode = "ru-RU", SemanticUrl = "virtual-parent2" },
                                }
                            },
                            new catalogModel.OutlineItem
                            {
                                SeoObjectType = "CatalogProduct",
                                HasVirtualParent = true,
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

            var result = category.Outlines.GetSeoPath(_store, new Language("ru-RU"), null);
            Assert.Equal("virtual-parent2/product2", result);
        }
    }
}
