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
        private readonly List<coreDto.SeoInfo> _coreSeoRecords;

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
        //
        // v1
        // |- v11 -> c2      ac2
        // L- v12 -> c3      acd
        //
        // v2
        // |- v21 -> c2      ac2
        // |- v22 -> c3      acd

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
            var v1 = new catalogDto.Catalog { Id = "v1", IsVirtual = true };
            var v2 = new catalogDto.Catalog { Id = "v2", IsVirtual = true };

            var c1 = AddCategory(c, "c1", "ac1", "ic1");
            var c2 = AddCategory(c, "c2", "ac2");
            var c3 = AddCategory(c, "c3", "acd", "ic3");
            var c4 = AddCategory(c, "c4", "acd", "ic4");

            var v11 = AddCategory(v1, "v11", "ac2");
            var v12 = AddCategory(v1, "v12", "acd");

            var v21 = AddCategory(v2, "v21", "ac2");
            var v22 = AddCategory(v2, "v22", "acd");

            var p1 = AddProduct(c, "p1", "ap1", "ipd", "ip1");
            var p2 = AddProduct(c, "p2", "ap2", "ipd");

            c1.Outlines.Add(CreateOutline(c, new[] { c1 }));
            c2.Outlines.Add(CreateOutline(c, new[] { c1, c2 }));
            c3.Outlines.Add(CreateOutline(c, new[] { c1, c3 }));
            c4.Outlines.Add(CreateOutline(c, new[] { c1, c3, c4 }));

            v11.Outlines.Add(CreateOutline(v1, new[] { v11 }));
            v12.Outlines.Add(CreateOutline(v1, new[] { v12 }));
            c2.Outlines.Add(CreateOutline(v1, new[] { v11, c2 }));
            c3.Outlines.Add(CreateOutline(v1, new[] { v12, c3 }));

            v21.Outlines.Add(CreateOutline(v2, new[] { v21 }));
            v22.Outlines.Add(CreateOutline(v2, new[] { v22 }));
            c2.Outlines.Add(CreateOutline(v2, new[] { v21, c2 }));
            c3.Outlines.Add(CreateOutline(v2, new[] { v22, c3 }));

            p1.Outlines.Add(CreateOutline(c, new[] { c1, c3 }, p1));
            p2.Outlines.Add(CreateOutline(c, new[] { c1, c3 }, p2));

            AddPage("en-US", "ag1", "ig1");

            _coreSeoRecords = _catalogSeoRecords.Select(s => s.JsonConvert<coreDto.SeoInfo>()).ToList();

            // Create duplicates
            _coreSeoRecords.AddRange(_coreSeoRecords);
        }

        [Theory]
        [InlineData("c", SeoLinksType.Short, "ac1", null, "CatalogSearch", "CategoryBrowsing", "categoryId", "c1")]
        [InlineData("c", SeoLinksType.Short, "ic1", "ac1", null, null, null, null)]
        [InlineData("c", SeoLinksType.Short, "ac1/ac2", null, "Asset", "GetThemeAssets", null, null)]
        [InlineData("c", SeoLinksType.Short, "ic1/ac2", null, "Asset", "GetThemeAssets", null, null)]
        [InlineData("c", SeoLinksType.Short, "ac2", null, "Asset", "GetThemeAssets", null, null)]
        [InlineData("c", SeoLinksType.Short, "ac1/acd", null, "Asset", "GetThemeAssets", null, null)]
        [InlineData("c", SeoLinksType.Short, "ac1/ic3", "acd", null, null, null, null)]
        [InlineData("c", SeoLinksType.Short, "acd", null, "Asset", "GetThemeAssets", null, null)]
        [InlineData("c", SeoLinksType.Short, "ic3", "acd", null, null, null, null)]
        [InlineData("c", SeoLinksType.Short, "ac1/acd/acd", null, "Asset", "GetThemeAssets", null, null)]
        [InlineData("c", SeoLinksType.Short, "ac1/ic3/ic4", "acd", null, null, null, null)]
        [InlineData("c", SeoLinksType.Short, "ac1/ic3/acd", null, "Asset", "GetThemeAssets", null, null)]
        [InlineData("c", SeoLinksType.Short, "ac1/acd/ap1", "ap1", null, null, null, null)]
        [InlineData("c", SeoLinksType.Short, "ac1/acd/ip1", "ap1", null, null, null, null)]
        [InlineData("c", SeoLinksType.Short, "ap1", null, "Product", "ProductDetails", "productId", "p1")]
        [InlineData("c", SeoLinksType.Short, "ip1", "ap1", null, null, null, null)]
        [InlineData("c", SeoLinksType.Short, "ipd", null, "Asset", "GetThemeAssets", null, null)]
        [InlineData("c", SeoLinksType.Short, "ac1/acd/ipd", null, "Asset", "GetThemeAssets", null, null)]
        [InlineData("c", SeoLinksType.Short, "av1", null, "Vendor", "VendorDetails", "vendorId", "v1")]
        [InlineData("c", SeoLinksType.Short, "iv1", "av1", null, null, null, null)]
        [InlineData("c", SeoLinksType.Short, "ag1", null, "StaticContent", "GetContentPage", null, null)]
        [InlineData("c", SeoLinksType.Short, "ig1", "ag1", null, null, null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "ac1", null, "CatalogSearch", "CategoryBrowsing", "categoryId", "c1")]
        [InlineData("c", SeoLinksType.Collapsed, "ic1", "ac1", null, null, null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "ac1/ac2", null, "CatalogSearch", "CategoryBrowsing", "categoryId", "c2")]
        [InlineData("c", SeoLinksType.Collapsed, "ic1/ac2", "ac1/ac2", null, null, null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "ac2", "ac1/ac2", null, null, null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "ac1/acd", null, "CatalogSearch", "CategoryBrowsing", "categoryId", "c3")]
        [InlineData("c", SeoLinksType.Collapsed, "ac1/ic3", "ac1/acd", null, null, null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "acd", null, "Asset", "GetThemeAssets", null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "ic3", "ac1/acd", null, null, null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "ac1/acd/acd", null, "CatalogSearch", "CategoryBrowsing", "categoryId", "c4")]
        [InlineData("c", SeoLinksType.Collapsed, "ac1/ic3/ic4", "ac1/acd/acd", null, null, null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "ac1/ic3/acd", null, "Asset", "GetThemeAssets", null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "ac1/acd/ap1", null, "Product", "ProductDetails", "productId", "p1")]
        [InlineData("c", SeoLinksType.Collapsed, "ac1/acd/ip1", "ac1/acd/ap1", null, null, null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "ap1", "ac1/acd/ap1", null, null, null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "ip1", "ac1/acd/ap1", null, null, null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "ipd", null, "Asset", "GetThemeAssets", null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "ac1/acd/ipd", null, "Asset", "GetThemeAssets", null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "av1", null, "Vendor", "VendorDetails", "vendorId", "v1")]
        [InlineData("c", SeoLinksType.Collapsed, "iv1", "av1", null, null, null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "ag1", null, "StaticContent", "GetContentPage", null, null)]
        [InlineData("c", SeoLinksType.Collapsed, "ig1", "ag1", null, null, null, null)]
        [InlineData("v1", SeoLinksType.Collapsed, "ac1", null, "Asset", "GetThemeAssets", null, null)]
        [InlineData("v1", SeoLinksType.Collapsed, "acd", null, "CatalogSearch", "CategoryBrowsing", "categoryId", "v12")]
        [InlineData("v1", SeoLinksType.Collapsed, "acd/acd", "acd", null, null, null, null)]
        [InlineData("v2", SeoLinksType.Collapsed, "acd", null, "CatalogSearch", "CategoryBrowsing", "categoryId", "v22")]
        [InlineData("v2", SeoLinksType.Collapsed, "acd/acd", "acd", null, null, null, null)]
        public void ValidateSeoRouteResponse(string catalogId, SeoLinksType linksType, string seoPath, string expectedRedirectLocation, string expectedController, string expectedAction, string expectedObjectIdName, string expectedObjectId)
        {
            // Don't use catalog API client for short SEO links
            var shortLinks = linksType == SeoLinksType.None || linksType == SeoLinksType.Short;
            var catalogApi = shortLinks ? null : CreateCatalogApiClient();
            var service = CreateSeoRouteService(catalogApi);

            var workContext = CreateWorkContext(catalogId, linksType);
            var response = service.HandleSeoRequest(seoPath, workContext);

            Assert.NotNull(response);
            Assert.Equal(expectedRedirectLocation, response.RedirectLocation);

            if (expectedRedirectLocation != null)
            {
                Assert.True(response.Redirect);
            }
            else
            {
                Assert.False(response.Redirect);
                Assert.NotNull(response.RouteData);
                Assert.Equal(expectedController, response.RouteData["controller"]);
                Assert.Equal(expectedAction, response.RouteData["action"]);

                if (expectedObjectIdName != null)
                {
                    Assert.Equal(expectedObjectId, response.RouteData[expectedObjectIdName]);
                }

                if (expectedController == "StaticContent")
                {
                    Assert.NotNull(response.RouteData["page"]);
                }
            }
        }


        private WorkContext CreateWorkContext(string catalogId, SeoLinksType linksType)
        {
            var store = CreateStore("s", catalogId, linksType, "en-US", "ru-RU");

            return new WorkContext
            {
                CurrentStore = store,
                CurrentLanguage = store.DefaultLanguage,
                Pages = new MutablePagedList<ContentItem>(_pages),
            };
        }

        private static Store CreateStore(string storeId, string catalogId, SeoLinksType linksType, params string[] cultureNames)
        {
            var result = new Store
            {
                Id = storeId,
                Catalog = catalogId,
                SeoLinksType = linksType,
                Languages = cultureNames.Select(n => new Language(n)).ToList(),
            };

            result.DefaultLanguage = result.Languages.FirstOrDefault();

            return result;
        }

        private catalogDto.Category AddCategory(catalogDto.Catalog catalog, string categoryId, params string[] slugs)
        {
            var result = new catalogDto.Category
            {
                Id = categoryId,
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

            result.Items.AddRange(categories.Select(c => CreateOutlineItem("Category", c.Id, c.CatalogId != catalog.Id)));

            if (product != null)
            {
                result.Items.Add(CreateOutlineItem("CatalogProduct", product.Id));
            }

            return result;
        }

        private catalogDto.OutlineItem CreateOutlineItem(string objectType, string objectId, bool? hasVirtualParent = null)
        {
            return new catalogDto.OutlineItem
            {
                SeoObjectType = objectType,
                Id = objectId,
                HasVirtualParent = hasVirtualParent,
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

        #region Mocks

        private ICatalogModuleApiClient CreateCatalogApiClient()
        {
            var catalogApi = new Mock<ICatalogModuleApiClient>();
            catalogApi
                .Setup(x => x.CatalogModuleCategories.GetCategoriesByIdsWithHttpMessagesAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<IList<string>, string, Dictionary<string, List<string>>, CancellationToken>(GetCategoriesByIdsWithHttpMessagesAsync);
            catalogApi
                .Setup(x => x.CatalogModuleProducts.GetProductByIdsWithHttpMessagesAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<IList<string>, string, Dictionary<string, List<string>>, CancellationToken>(GetProductByIdsWithHttpMessagesAsync);

            return catalogApi.Object;
        }

        private ISeoRouteService CreateSeoRouteService(ICatalogModuleApiClient catalogApiClient)
        {
            var cacheManager = new Mock<ILocalCacheManager>();
            //cacheManager.Setup(cache => cache.Get<List<catalogDto.SeoInfo>>(It.IsAny<string>(), It.IsAny<string>())).Returns<List<catalogDto.SeoInfo>>(null);

            var coreApi = new Mock<ICoreModuleApiClient>();
            coreApi
                .Setup(x => x.Commerce.GetSeoInfoBySlugWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<string, Dictionary<string, List<string>>, CancellationToken>(GetSeoInfoBySlugWithHttpMessagesAsync);


            var result = new SeoRouteService(() => coreApi.Object, () => catalogApiClient, cacheManager.Object);
            return result;
        }

        private Task<HttpOperationResponse<IList<coreDto.SeoInfo>>> GetSeoInfoBySlugWithHttpMessagesAsync(string slug, Dictionary<string, List<string>> customHeaders, CancellationToken cancellationToken)
        {
            var result = new HttpOperationResponse<IList<coreDto.SeoInfo>>
            {
                Body = _coreSeoRecords
                    .Where(s => s.SemanticUrl.EqualsInvariant(slug))
                    .Join(_coreSeoRecords, x => string.Join("_", x.ObjectType, x.ObjectId), y => string.Join("_", y.ObjectType, y.ObjectId), (x, y) => y)
                    //.Reverse()
                    .ToList()
            };
            return Task.FromResult(result);
        }

        private Task<HttpOperationResponse<IList<catalogDto.Category>>> GetCategoriesByIdsWithHttpMessagesAsync(IList<string> ids, string respGroup, Dictionary<string, List<string>> customHeaders, CancellationToken cancellationToken)
        {
            var result = new HttpOperationResponse<IList<catalogDto.Category>>
            {
                Body = _categories.Where(c => ids.Contains(c.Id, StringComparer.OrdinalIgnoreCase)).ToList()
            };
            return Task.FromResult(result);
        }

        private Task<HttpOperationResponse<IList<catalogDto.Product>>> GetProductByIdsWithHttpMessagesAsync(IList<string> ids, string respGroup, Dictionary<string, List<string>> customHeaders, CancellationToken cancellationToken)
        {
            var result = new HttpOperationResponse<IList<catalogDto.Product>>
            {
                Body = _products.Where(p => ids.Contains(p.Id, StringComparer.OrdinalIgnoreCase)).ToList()
            };
            return Task.FromResult(result);
        }

        #endregion
    }
}
