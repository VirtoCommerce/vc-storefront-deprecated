using System.Web.Optimization;
using Xunit;

namespace VirtoCommerce.Storefront.Test
{
    [Trait("Category", "CI")]
    public class CssUrlTransformTests
    {
        private readonly IItemTransform _transformer;

        public CssUrlTransformTests()
        {
            _transformer = new CssUrlTransform();
        }

        [Theory]
        [InlineData(".test { background: url(/logo.png) }", ".test { background: url(/logo.png) }")]
        [InlineData(".test { background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAZdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuMTJDBGvsAAAADUlEQVQYV2P4//8/AwAI/AL+iF8G4AAAAABJRU5ErkJggg==) }", ".test { background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAZdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuMTJDBGvsAAAADUlEQVQYV2P4//8/AwAI/AL+iF8G4AAAAABJRU5ErkJggg==) }")]
        [InlineData(".test { background: url(//cdn.some.com/infographs/whatisacdnmini.jpg) }", ".test { background: url(//cdn.some.com/infographs/whatisacdnmini.jpg) }")]
        public void ValidateCustomCssRewriteUrl(string originalCss, string expectedCss)
        {
            var result = _transformer.Process(null, originalCss);

            Assert.Equal(expectedCss, result);
        }
    }
}
