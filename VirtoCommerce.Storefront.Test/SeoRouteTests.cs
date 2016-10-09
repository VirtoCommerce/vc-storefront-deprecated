using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Moq;
using VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.StaticContent;
using VirtoCommerce.Storefront.Model.Stores;
using VirtoCommerce.Storefront.Routing;
using Xunit;
using catalogDto = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;
using coreDto = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;

namespace VirtoCommerce.Storefront.Test
{
    [Trait("Category", "CI")]
    public class SeoRouteTests
    {
        private readonly List<catalogDto.Category> _categories;
        private readonly List<catalogDto.Product> _products;
        private readonly List<catalogDto.SeoInfo> _catalogSeoRecords;
        private readonly List<ContentPage> _pages;
        private readonly coreDto.SeoInfo[] _coreSeoRecords;
        private readonly WorkContext _workContext;

        // Catalog structure
        //
        // Slug letters:
        //     a = active
        //     i = inactive
        //     c = category
        //     p = product
        //     v = vendor
        //     g = page
        //     d = duplicate
        //
        // c
        // L- c1            ac1, ic1
        //     |- c2        ac2
        //     L- c3        acd, ic3
        //         |- c4    acd, ic4
        //         |- p1    ap1, ipd, ip1
        //         L- p2    ap2, ipd

        public SeoRouteTests()
        {
            _categories = new List<catalogDto.Category>();
            _products = new List<catalogDto.Product>();
            _pages = new List<ContentPage>();

            _catalogSeoRecords = new List<catalogDto.SeoInfo>
            {
                new catalogDto.SeoInfo { ObjectType = "Vendor", ObjectId = "v1", SemanticUrl = "av1", IsActive = true },
                new catalogDto.SeoInfo { ObjectType = "Vendor", ObjectId = "v1", SemanticUrl = "iv1", IsActive = false },
            };

            var c = new catalogDto.Catalog { Id = "c" };

            var c1 = AddCategory(c, "c1", "ac1", "ic1");
            var c2 = AddCategory(c, "c2", "ac2");
            var c3 = AddCategory(c, "c3", "acd", "ic3");
            var c4 = AddCategory(c, "c4", "acd", "ic4");
            var p1 = AddProduct(c, "p1", "ap1", "ipd", "ip1");
            var p2 = AddProduct(c, "p2", "ap2", "ipd");

            c1.Outlines.Add(CreateOutline(c, new[] { c1 }));
            c2.Outlines.Add(CreateOutline(c, new[] { c1, c2 }));
            c3.Outlines.Add(CreateOutline(c, new[] { c1, c3 }));
            c4.Outlines.Add(CreateOutline(c, new[] { c1, c3, c4 }));
            p1.Outlines.Add(CreateOutline(c, new[] { c1, c3 }, p1));
            p2.Outlines.Add(CreateOutline(c, new[] { c1, c3 }, p2));

            AddPage("en-US", "ag1", "ig1");

            _coreSeoRecords = _catalogSeoRecords.Select(s => s.JsonConvert<coreDto.SeoInfo>()).ToArray();

            var store = CreateStore("s", c.Id, "en-US", "ru-RU");

            _workContext = new WorkContext
            {
                CurrentStore = store,
                CurrentLanguage = store.DefaultLanguage,
                Pages = new MutablePagedList<ContentItem>(_pages),
            };
        }

        [Theory]
        [InlineData("ac1", "Category", "c1", "ac1")]
        [InlineData("ic1", "Category", "c1", "ac1")]
        [InlineData("ac1/ac2", "Category", "c2", "ac1/ac2")]
        [InlineData("ic1/ac2", "Category", "c2", "ac1/ac2")]
        [InlineData("ac2", "Category", "c2", "ac1/ac2")]
        [InlineData("ac1/acd", "Category", "c3", "ac1/acd")]
        [InlineData("ac1/ic3", "Category", "c3", "ac1/acd")]
        [InlineData("acd", null, null, null)]
        [InlineData("ic3", "Category", "c3", "ac1/acd")]
        [InlineData("ac1/acd/acd", "Category", "c4", "ac1/acd/acd")]
        [InlineData("ac1/ic3/ic4", "Category", "c4", "ac1/acd/acd")]
        [InlineData("ac1/ic3/acd", null, null, null)]
        [InlineData("ac1/ac3/ip1", "CatalogProduct", "p1", "ac1/acd/ap1")]
        [InlineData("ap1", "CatalogProduct", "p1", "ac1/acd/ap1")]
        [InlineData("ip1", "CatalogProduct", "p1", "ac1/acd/ap1")]
        [InlineData("ipd", null, null, null)]
        [InlineData("ac1/ac3/ipd", null, null, null)]
        [InlineData("av1", "Vendor", "v1", "av1")]
        [InlineData("iv1", "Vendor", "v1", "av1")]
        [InlineData("ag1", "Page", null, "ag1")]
        [InlineData("ig1", "Page", null, "ag1")]
        public void FindAndValidateSeoEntity(string seoPath, string expectedObjectType, string expectedObjectId, string expectedSeoPath)
        {
            var service = GetSeoRouteService();
            var entity = service.FindEntityBySeoPath(seoPath, _workContext);

            if (expectedObjectType == null)
            {
                Assert.Null(entity);
            }
            else
            {
                Assert.NotNull(entity);
                Assert.Equal(expectedObjectType, entity.ObjectType);
                Assert.Equal(expectedObjectId, entity.ObjectId);
                Assert.Equal(expectedSeoPath, entity.SeoPath);

                if (expectedObjectType == "Page")
                {
                    Assert.NotNull(entity.ObjectInstance);
                }
            }
        }

        private catalogDto.Category AddCategory(catalogDto.Catalog catalog, string categoryId, params string[] slugs)
        {
            var result = new catalogDto.Category
            {
                Id = categoryId,
                Catalog = catalog,
                CatalogId = catalog.Id,
                Outlines = new List<catalogDto.Outline>()
            };

            _categories.Add(result);
            _catalogSeoRecords.AddRange(slugs.Select(s => CreateSeoInfo("Category", categoryId, s)));

            return result;
        }

        private catalogDto.Product AddProduct(catalogDto.Catalog catalog, string productId, params string[] slugs)
        {
            var result = new catalogDto.Product
            {
                Id = productId,
                Catalog = catalog,
                CatalogId = catalog.Id,
                Outlines = new List<catalogDto.Outline>()
            };

            _products.Add(result);
            _catalogSeoRecords.AddRange(slugs.Select(s => CreateSeoInfo("CatalogProduct", productId, s)));

            return result;
        }

        private void AddPage(string cultureName, params string[] urls)
        {
            _pages.Add(new ContentPage
            {
                Language = new Language(cultureName),
                Url = urls[0],
                AliasesUrls = urls.Skip(1).ToList(),
            });
        }

        private catalogDto.Outline CreateOutline(catalogDto.Catalog catalog, catalogDto.Category[] categories, catalogDto.Product product = null)
        {
            var result = new catalogDto.Outline
            {
                Items = new List<catalogDto.OutlineItem> { CreateOutlineItem("Catalog", catalog.Id) }
            };

            result.Items.AddRange(categories.Select(c => CreateOutlineItem("Category", c.Id)));

            if (product != null)
            {
                result.Items.Add(CreateOutlineItem("CatalogProduct", product.Id));
            }

            return result;
        }

        private catalogDto.OutlineItem CreateOutlineItem(string objectType, string objectId)
        {
            return new catalogDto.OutlineItem
            {
                SeoObjectType = objectType,
                Id = objectId,
                SeoInfos = _catalogSeoRecords.Where(s => s.ObjectType == objectType && s.ObjectId == objectId).ToList(),
            };
        }

        private static catalogDto.SeoInfo CreateSeoInfo(string objectType, string objectId, string slug)
        {
            return new catalogDto.SeoInfo
            {
                ObjectType = objectType,
                ObjectId = objectId,
                SemanticUrl = slug,
                IsActive = slug.StartsWith("a"),
            };
        }

        private static Store CreateStore(string storeId, string catalogId, params string[] cultureNames)
        {
            var result = new Store
            {
                Id = storeId,
                Catalog = catalogId,
                SeoLinksType = SeoLinksType.Collapsed,
                Languages = cultureNames.Select(n => new Language(n)).ToList(),
            };

            result.DefaultLanguage = result.Languages.FirstOrDefault();

            return result;
        }

        #region Mocks

        private ISeoRouteService GetSeoRouteService()
        {
            var cacheManager = new Mock<ILocalCacheManager>();
            //cacheManager.Setup(cache => cache.Get<List<catalogDto.SeoInfo>>(It.IsAny<string>(), It.IsAny<string>())).Returns<List<catalogDto.SeoInfo>>(null);

            var coreApi = new Mock<ICoreModuleApiClient>();
            coreApi
                .Setup(x => x.Commerce.GetSeoInfoBySlugWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<string, Dictionary<string, List<string>>, CancellationToken>(GetSeoInfoBySlugWithHttpMessagesAsync);

            var catalogApi = new Mock<ICatalogModuleApiClient>();
            catalogApi
                .Setup(x => x.CatalogModuleCategories.GetCategoriesByIdsWithHttpMessagesAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<IList<string>, string, Dictionary<string, List<string>>, CancellationToken>(GetCategoriesByIdsWithHttpMessagesAsync);
            catalogApi
                .Setup(x => x.CatalogModuleProducts.GetProductByIdsWithHttpMessagesAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<IList<string>, string, Dictionary<string, List<string>>, CancellationToken>(GetProductByIdsWithHttpMessagesAsync);

            var result = new SeoRouteService(() => coreApi.Object, () => catalogApi.Object, cacheManager.Object);
            return result;
        }

        private Task<HttpOperationResponse<IList<coreDto.SeoInfo>>> GetSeoInfoBySlugWithHttpMessagesAsync(string slug, Dictionary<string, List<string>> customHeaders, CancellationToken cancellationToken)
        {
            var result = new HttpOperationResponse<IList<coreDto.SeoInfo>>();
            result.Body = _coreSeoRecords
                .Where(s => s.SemanticUrl.EqualsInvariant(slug))
                .Join(_coreSeoRecords, x => x.ObjectId, y => y.ObjectId, (x, y) => y)
                .ToList();
            return Task.FromResult(result);
        }

        private Task<HttpOperationResponse<IList<catalogDto.Category>>> GetCategoriesByIdsWithHttpMessagesAsync(IList<string> ids, string respGroup, Dictionary<string, List<string>> customHeaders, CancellationToken cancellationToken)
        {
            var result = new HttpOperationResponse<IList<catalogDto.Category>>();
            result.Body = _categories.Where(c => ids.Contains(c.Id, StringComparer.OrdinalIgnoreCase)).ToList();
            return Task.FromResult(result);
        }

        private Task<HttpOperationResponse<IList<catalogDto.Product>>> GetProductByIdsWithHttpMessagesAsync(IList<string> ids, string respGroup, Dictionary<string, List<string>> customHeaders, CancellationToken cancellationToken)
        {
            var result = new HttpOperationResponse<IList<catalogDto.Product>>();
            result.Body = _products.Where(p => ids.Contains(p.Id, StringComparer.OrdinalIgnoreCase)).ToList();
            return Task.FromResult(result);
        }

        #endregion
    }
}
