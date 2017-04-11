using System.Linq;
using PagedList;
using VirtoCommerce.Storefront.Model.Common;
using Xunit;

namespace VirtoCommerce.Storefront.Test
{
    [Trait("Category", "CI")]
    public class CommonTests
    {
        [Theory]
        [InlineData("http://test/file123.jpg", "_grande", "http://test/file123_grande.jpg")]
        [InlineData("http://test/file123.jpg.liquid", "_grande", "http://test/file123_grande.jpg.liquid")]
        [InlineData("http://test/file123", "_grande", "http://test/file123_grande")]
        public void StringExtensionsAddSuffixToFileUrl(string str, string suffix, string result)
        {
            Assert.Equal(str.AddSuffixToFileUrl(suffix), result);
        }

    }
}
