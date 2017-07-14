using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart.Services;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Common.Exceptions;
using VirtoCommerce.Storefront.Model.Services;


namespace VirtoCommerce.Storefront.Controllers.Api
{
    [HandleJsonError]
    public class ApiWishlistController : StorefrontControllerBase
    {
        private readonly ICartBuilder _wishlistBuilder;
        private readonly ICatalogSearchService _catalogSearchService;

        public ApiWishlistController(WorkContext workContext, ICatalogSearchService catalogSearchService, ICartBuilder cartBuilder,
                                     IStorefrontUrlBuilder urlBuilder)
            : base(workContext, urlBuilder)
        {
            _wishlistBuilder = cartBuilder;
            _catalogSearchService = catalogSearchService;
        }

        // GET: ApiWishlist
        // GET: storefrontapi/wishlist
        [HttpGet]
        public async Task<ActionResult> GetWishlist()
        {
            var wishlistBuilder = await LoadOrCreateWishlistAsync();
            await wishlistBuilder.ValidateAsync();
            return Json(wishlistBuilder.Cart, JsonRequestBehavior.AllowGet);
        }

        // GET: storefrontapi/whishlst/contains
        [HttpGet]
        public async Task<ActionResult> Contains(string productId)
        {
            var wishlistBuilder = await LoadOrCreateWishlistAsync();
            await wishlistBuilder.ValidateAsync();
            var hasProduct = wishlistBuilder.Cart.Items.Any(x => x.ProductId == productId);
            return Json(new { Contains = hasProduct }, JsonRequestBehavior.AllowGet);
        }

        // POST: storefrontapi/wishlist/items?id=...
        [HttpPost]
        public async Task<ActionResult> AddItemToWishlist(string productId)
        {
            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext)).LockAsync())
            {
                var wishlistBuilder = await LoadOrCreateWishlistAsync();

                var products = await _catalogSearchService.GetProductsAsync(new[] { productId }, Model.Catalog.ItemResponseGroup.ItemLarge);
                if (products != null && products.Any())
                {
                    await wishlistBuilder.AddItemAsync(products.First(), 1);
                    await wishlistBuilder.SaveAsync();
                }
                return Json(new { ItemsCount = wishlistBuilder.Cart.ItemsQuantity });
            }
        }

        // DELETE: storefrontapi/wishlist/items?id=...
        [HttpDelete]
        public async Task<ActionResult> RemoveWishlistItem(string lineItemId)
        {
            //Need lock to prevent concurrent access to same cart
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(WorkContext)).LockAsync())
            {
                var wishlistBuilder = await LoadOrCreateWishlistAsync();
                await wishlistBuilder.RemoveItemAsync(lineItemId);
                await wishlistBuilder.SaveAsync();
                return Json(new { ItemsCount = wishlistBuilder.Cart.ItemsQuantity });
            }

        }
        private static string GetAsyncLockCartKey(WorkContext context)
        {
            return string.Join(":", "wishlist", context.CurrentCustomer.Id, context.CurrentStore.Id);
        }

        private void EnsureCartExists()
        {
            if (WorkContext.CurrentCart == null)
            {
                throw new StorefrontException("Whishlist not found");
            }
        }

        private async Task<ICartBuilder> LoadOrCreateWishlistAsync()
        {
            await _wishlistBuilder.LoadOrCreateNewTransientCartAsync("whishlist", WorkContext.CurrentStore, WorkContext.CurrentCustomer, WorkContext.CurrentLanguage, WorkContext.CurrentCurrency);
            return _wishlistBuilder;
        }
    }
}
