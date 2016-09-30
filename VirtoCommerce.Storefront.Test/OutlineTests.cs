using System.Collections.Generic;
using VirtoCommerce.Storefront.Common;
using Xunit;
using catalogModel = VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi.Models;

namespace VirtoCommerce.Storefront.Test
{
    [Trait("Category", "CI")]
    public class OutlineTests
    {
        [Fact]
        public void When_HasMultipleOutlines_Expect_PathForGivenCatalog()
        {
            var outlines = new List<catalogModel.Outline>
            {
                new catalogModel.Outline
                {
                    Items = new List<catalogModel.OutlineItem>
                    {
                        new catalogModel.OutlineItem
                        {
                            SeoObjectType = "Catalog",
                            Id = "catalog1",
                        },
                        new catalogModel.OutlineItem
                        {
                            SeoObjectType = "Category",
                            Id = "parent1",
                        },
                        new catalogModel.OutlineItem
                        {
                            SeoObjectType = "Category",
                            Id = "category1",
                        },
                    },
                },
                new catalogModel.Outline
                {
                    Items = new List<catalogModel.OutlineItem>
                    {
                        new catalogModel.OutlineItem
                        {
                            SeoObjectType = "Catalog",
                            Id = "catalog2",
                        },
                        new catalogModel.OutlineItem
                        {
                            SeoObjectType = "Category",
                            Id = "parent2",
                        },
                        new catalogModel.OutlineItem
                        {
                            SeoObjectType = "Category",
                            Id = "category2",
                        },
                    },
                },
            };

            var result = outlines.GetOutlinePath("catalog2");
            Assert.Equal("parent2/category2", result);
        }
    }
}
