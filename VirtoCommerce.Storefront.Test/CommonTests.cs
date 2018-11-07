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
        [InlineData("http://domain.subdomain/file123.jpg", "_grande", "http://domain.subdomain/file123_grande.jpg")]
        [InlineData("http://domain.subdomain/file123/file123.jpg", "_grande", "http://domain.subdomain/file123/file123_grande.jpg")]
        [InlineData("http://domain.subdomain/file123", "_grande", "http://domain.subdomain/file123_grande")]
        public void StringExtensionsAddSuffixToFileUrl(string str, string suffix, string result)
        {
            Assert.Equal(str.AddSuffixToFileUrl(suffix), result);
        }

    }
}
