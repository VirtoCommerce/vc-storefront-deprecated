using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using VirtoCommerce.LiquidThemeEngine.Converters;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Cart.Services;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Common.Exceptions;
using VirtoCommerce.Storefront.Model.Services;

namespace VirtoCommerce.Storefront.Controllers
{
    public class ShopifyCompatibilityController : StorefrontControllerBase
    {
        private readonly WorkContext _workContext;
        private readonly ICartBuilder _cartBuilder;
        private readonly ICatalogSearchService _catalogService;

        public ShopifyCompatibilityController(WorkContext workContext, IStorefrontUrlBuilder urlBuilder, ICartBuilder cartBuilder, ICatalogSearchService catalogService)
            : base(workContext, urlBuilder)
        {
            _workContext = workContext;
            _cartBuilder = cartBuilder;
            _catalogService = catalogService;
        }

        // GET: /cart/change?line=...&quantity=...
        [HttpGet]
        public async Task<ActionResult> Change(int line, int quantity)
        {
            await EnsureCartExistsAsync();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.ChangeItemQuantityAsync(line - 1, quantity);
                await _cartBuilder.SaveAsync();
            }
            return StoreFrontRedirect("~/cart");
        }

        // POST: /cart/add
        [HttpPost]
        public async Task<ActionResult> Add(string id, int quantity)
        {
            await EnsureCartExistsAsync();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                var product = (await _catalogService.GetProductsAsync(new[] { id }, Model.Catalog.ItemResponseGroup.ItemLarge)).FirstOrDefault();
                if (product != null)
                {
                    await _cartBuilder.AddItemAsync(product, quantity);
                    await _cartBuilder.SaveAsync();
                }
            }
            return StoreFrontRedirect("~/cart");
        }

        // POST: /cart?updates=...&update=...
        [HttpPost]
        public async Task<ActionResult> Cart(int[] updates, string checkout)
        {
            string virtualRedirectUrl = "~/cart";

            await EnsureCartExistsAsync();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.ChangeItemsQuantitiesAsync(updates);
                await _cartBuilder.SaveAsync();


                if (Request.Form.Get("checkout") != null)
                {

                    virtualRedirectUrl = "~/cart/checkout";

                }
            }
            return StoreFrontRedirect(virtualRedirectUrl);
        }

        // GET: /cart/clear
        [HttpGet]
        public async Task<ActionResult> Clear()
        {
            await EnsureCartExistsAsync();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.ClearAsync();
                await _cartBuilder.SaveAsync();
            }
            return StoreFrontRedirect("~/cart");
        }

        // GET: /cart.js
        [HttpGet]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> CartJs()
        {
            await EnsureCartExistsAsync();

            return LiquidJson(_cartBuilder.Cart.ToShopifyModel(WorkContext.CurrentLanguage, UrlBuilder));
        }

        // POST: /cart/add.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> AddJs(string id, int quantity = 1)
        {
            LineItem lineItem = null;

            await EnsureCartExistsAsync();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                var product = (await _catalogService.GetProductsAsync(new[] { id }, Model.Catalog.ItemResponseGroup.ItemLarge)).FirstOrDefault();
                if (product != null)
                {
                    await _cartBuilder.AddItemAsync(product, quantity);
                    await _cartBuilder.SaveAsync();

                    lineItem = _cartBuilder.Cart.Items.FirstOrDefault(i => i.ProductId == id);
                }
            }
            return LiquidJson(lineItem != null ? lineItem.ToShopifyModel(_workContext.CurrentLanguage, UrlBuilder) : null);
        }

        // POST: /cart/change.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> ChangeJs(string id, int quantity)
        {
            await EnsureCartExistsAsync();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.ChangeItemQuantityAsync(id, quantity);
                await _cartBuilder.SaveAsync();
            }
            return LiquidJson(_cartBuilder.Cart.ToShopifyModel(WorkContext.CurrentLanguage, UrlBuilder));
        }

        // POST: /cart/update.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> UpdateJs(int[] updates)
        {
            await EnsureCartExistsAsync();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.ChangeItemsQuantitiesAsync(updates);
                await _cartBuilder.SaveAsync();
            }
            return LiquidJson(_cartBuilder.Cart.ToShopifyModel(WorkContext.CurrentLanguage, UrlBuilder));
        }

        // POST: /cart/clear.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> ClearJs()
        {
            await EnsureCartExistsAsync();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.ClearAsync();
                await _cartBuilder.SaveAsync();
            }
            return LiquidJson(_cartBuilder.Cart.ToShopifyModel(WorkContext.CurrentLanguage, UrlBuilder));
        }

        /// GET collections
        /// This method used for display all categories
        /// <returns></returns>
        [OutputCache(CacheProfile = "CatalogSearchCachingProfile")]
        public ActionResult Collections()
        {
            return View("list-collections", WorkContext);
        }

        private JsonResult LiquidJson(object obj)
        {
            var serializedString = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ContractResolver = new LiquidThemeEngine.Filters.RubyContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return new JsonResult
            {
                ContentEncoding = Encoding.UTF8,
                ContentType = "application/json",
                Data = JsonConvert.DeserializeObject(serializedString),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private async Task EnsureCartExistsAsync()
        {
            if (WorkContext.CurrentCart == null)
            {
                throw new StorefrontException("Cart not found");
            }
            await _cartBuilder.TakeCartAsync(WorkContext.CurrentCart);
        }

        private static string GetAsyncLockCartKey(ShoppingCart cart)
        {
            return string.Join(":", "Cart", cart.Id, cart.Name, cart.CustomerId);
        }
    }
}
