using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.SubscriptionModuleApi;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Cart.Services;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Common.Events;
using VirtoCommerce.Storefront.Model.Common.Exceptions;
using VirtoCommerce.Storefront.Model.Order.Events;
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

        // POST: storefrontapi/wishlist/items?id=...
        [HttpPost]
        public async Task<ActionResult> AddItemToWishlist(string id, int quantity = 1)
        {
            var wishlistBuilder = await LoadOrCreateWishlistAsync();
            var products = await _catalogSearchService.GetProductsAsync(new[] { id }, Model.Catalog.ItemResponseGroup.ItemLarge);
            if (products != null && products.Any())
            {
                await wishlistBuilder.AddItemAsync(products.First(), quantity);
                await wishlistBuilder.SaveAsync();
            }
            return Json(new { ItemsCount = wishlistBuilder.Cart.ItemsQuantity });
            
        }
        
        // DELETE: storefrontapi/wishlist/items?id=...
        [HttpDelete]
        public async Task<ActionResult> RemoveWishlistItem(string lineItemId)
        {
            var wishlistBuilder = await LoadOrCreateWishlistAsync();
            await wishlistBuilder.RemoveItemAsync(lineItemId);
            await wishlistBuilder.SaveAsync();
            return Json(new { ItemsCount = wishlistBuilder.Cart.ItemsQuantity });
        }
        private static string GetAsyncLockCartKey(ShoppingCart cart)
        {
            return string.Join(":", "wishlist", cart.Id, cart.Name, cart.CustomerId);
        }

        private async Task<ICartBuilder> LoadOrCreateWishlistAsync()
        {
            await _wishlistBuilder.LoadOrCreateNewTransientCartAsync("whishlist", WorkContext.CurrentStore, WorkContext.CurrentCustomer, WorkContext.CurrentLanguage, WorkContext.CurrentCurrency);
            return _wishlistBuilder;
        }
    }
}
