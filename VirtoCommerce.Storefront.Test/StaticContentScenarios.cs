using System;
using System.IO;
using System.Linq;
using Moq;
using VirtoCommerce.LiquidThemeEngine;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.StaticContent;
using VirtoCommerce.Storefront.Model.StaticContent.Services;
using VirtoCommerce.Storefront.Model.Stores;
using VirtoCommerce.Storefront.Services;
using Xunit;

namespace VirtoCommerce.Storefront.Test
{
    [Trait("Category", "CI")]
    public class StaticContentScenarios
    {
        [Fact]
        public void LoadPageForLanguage_PageLoadedFromFolder()
        {
            var language = new Model.Language("en-US");
            var service = GetStaticContentService();
            var result = service.LoadStoreStaticContent(new Store { Id = "TestStore" });

            var page = result.Single(x => x.Url.Equals("folder1/about_us") && x.Language == language);
            Assert.IsType<ContentPage>(page);
            Assert.Equal(page.Language, language);
            Assert.NotEmpty(page.Content);
            Assert.Equal(page.Url, "folder1/about_us");
            Assert.Equal(Path.GetFileName(page.StoragePath), "about_us.en-US.md");
        }

        [Fact]
        public void LoadPageForMissedLanguage_PageLoadedWithDefaultLanguage()
        {
            var language = new Model.Language("es-ES");
            var service = GetStaticContentService();
            var result = service.LoadStoreStaticContent(new Store { Id = "TestStore" });

            var page = result.Single(x => x.Url.Equals("about_us") && (x.Language == language || x.Language.IsInvariant));
            Assert.IsType<ContentPage>(page);
            Assert.NotEmpty(page.Content);
            Assert.Equal(page.Url, "about_us");
            Assert.Equal(Path.GetFileName(page.StoragePath), "about_us.md");
        }

        [Fact]
        public void StaticContent_get_formatted_permalink()
        {
            var language = new Model.Language("en-US");
            var service = GetStaticContentService();

            var result = service.LoadStoreStaticContent(new Store { Id = "TestStore" });

            var page = result.Single(x => x.Url.Equals("blogs/news/about_us_permalink") && (x.Language == language || x.Language.IsInvariant));

            Assert.IsType<ContentPage>(page);
            Assert.NotEmpty(page.Content);
            Assert.Equal(page.Url, "blogs/news/about_us_permalink");
            Assert.Equal(Path.GetFileName(page.StoragePath), "about_us_permalink.md");
        }

        [Fact]
        public void StaticContent_get_blogs()
        {
            var language = new Model.Language("en-US");
            var service = GetStaticContentService();

            var result = service.LoadStoreStaticContent(new Store { Id = "TestStore" }).ToList();

            var blog = result.OfType<Blog>().FirstOrDefault(x => x.Name == "news");
            Assert.NotNull(blog);

            var page = result.Single(x => x.Url.Equals("blogs/news/post1") && (x.Language == language || x.Language.IsInvariant));
            Assert.IsType<BlogArticle>(page);
            Assert.NotEmpty(page.Content);
            Assert.Equal(page.Url, "blogs/news/post1");
            Assert.Equal(((BlogArticle)page).BlogName, "news");
        }

        [Fact]
        public void DontCrashOnInvalidYaml()
        {
            var service = GetStaticContentService();
            var result = service.LoadStoreStaticContent(new Store { Id = "StoreWithInvalidPages" }).ToList();
            Assert.Equal(2, result.Count);
        }


        private static IStaticContentService GetStaticContentService()
        {
            var cacheManager = new Mock<ILocalCacheManager>();
            cacheManager.Setup(cache => cache.Get<ContentItem[]>(It.IsAny<string>(), It.IsAny<string>())).Returns<ContentItem[]>(null);
            var urlBuilder = new Mock<IStorefrontUrlBuilder>();
            var liquidEngine = new Mock<ILiquidThemeEngine>();
            var path = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, "Pages");
            var blobProvider = new FileSystemContentBlobProvider(path);
            var retVal = new StaticContentServiceImpl(liquidEngine.Object, cacheManager.Object, () => null, () => urlBuilder.Object, StaticContentItemFactory.GetContentItemFromPath, blobProvider);
            return retVal;
        }
    }
}
