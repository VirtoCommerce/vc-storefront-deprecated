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
            EnsureCartExists();

            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();
                await cartBuilder.ChangeItemQuantityAsync(line - 1, quantity);
                await cartBuilder.SaveAsync();
            }
            return StoreFrontRedirect("~/cart");
        }

        // POST: /cart/add
        [HttpPost]
        public async Task<ActionResult> Add(string id, int quantity)
        {
            EnsureCartExists();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();
                var product = (await _catalogService.GetProductsAsync(new[] { id }, Model.Catalog.ItemResponseGroup.ItemLarge)).FirstOrDefault();
                if (product != null)
                {
                    await cartBuilder.AddItemAsync(product, quantity);
                    await cartBuilder.SaveAsync();
                }
            }
            return StoreFrontRedirect("~/cart");
        }

        // POST: /cart?updates=...&update=...
        [HttpPost]
        public async Task<ActionResult> Cart(int[] updates, string checkout)
        {
            string virtualRedirectUrl = "~/cart";

            EnsureCartExists();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();
                await cartBuilder.ChangeItemsQuantitiesAsync(updates);
                await cartBuilder.SaveAsync();


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
            EnsureCartExists();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();
                await cartBuilder.ClearAsync();
                await cartBuilder.SaveAsync();
            }
            return StoreFrontRedirect("~/cart");
        }

        // GET: /cart.js
        [HttpGet]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> CartJs()
        {
            EnsureCartExists();
            var cartBuilder = await LoadOrCreateCartAsync();
            return LiquidJson(cartBuilder.Cart.ToShopifyModel(WorkContext.CurrentLanguage, UrlBuilder));
        }

        // POST: /cart/add.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> AddJs(string id, int quantity = 1)
        {
            LineItem lineItem = null;

            EnsureCartExists();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                var product = (await _catalogService.GetProductsAsync(new[] { id }, Model.Catalog.ItemResponseGroup.ItemLarge)).FirstOrDefault();
                if (product != null)
                {
                    var cartBuilder = await LoadOrCreateCartAsync();
                    await cartBuilder.AddItemAsync(product, quantity);
                    await cartBuilder.SaveAsync();

                    lineItem = cartBuilder.Cart.Items.FirstOrDefault(i => i.ProductId == id);
                }
            }
            return LiquidJson(lineItem != null ? lineItem.ToShopifyModel(_workContext.CurrentLanguage, UrlBuilder) : null);
        }

        // POST: /cart/change.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> ChangeJs(string id, int quantity)
        {
            EnsureCartExists();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();
                await cartBuilder.ChangeItemQuantityAsync(id, quantity);
                await cartBuilder.SaveAsync();
                return LiquidJson(cartBuilder.Cart.ToShopifyModel(WorkContext.CurrentLanguage, UrlBuilder));
            }           
        }

        // POST: /cart/update.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> UpdateJs(int[] updates)
        {
            EnsureCartExists();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();
                await cartBuilder.ChangeItemsQuantitiesAsync(updates);
                await cartBuilder.SaveAsync();
                return LiquidJson(cartBuilder.Cart.ToShopifyModel(WorkContext.CurrentLanguage, UrlBuilder));
            }           
        }

        // POST: /cart/clear.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> ClearJs()
        {
            EnsureCartExists();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                var cartBuilder = await LoadOrCreateCartAsync();
                await cartBuilder.ClearAsync();
                await cartBuilder.SaveAsync();
                return LiquidJson(_cartBuilder.Cart.ToShopifyModel(WorkContext.CurrentLanguage, UrlBuilder));
            }           
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

        private void EnsureCartExists()
        {
            if (WorkContext.CurrentCart == null)
            {
                throw new StorefrontException("Cart not found");
            }
        }
        private async Task<ICartBuilder> LoadOrCreateCartAsync()
        {
            var cart = WorkContext.CurrentCart;
            //Need to try load fresh cart from cache or service to prevent parallel update conflict
            //because WorkContext.CurrentCart may contains old cart
            await _cartBuilder.LoadOrCreateNewTransientCartAsync(cart.Name, WorkContext.CurrentStore, cart.Customer, cart.Language, cart.Currency);
            return _cartBuilder;
        }

        private static string GetAsyncLockCartKey(ShoppingCart cart)
        {
            return string.Join(":", "Cart", cart.Id, cart.Name, cart.CustomerId);
        }
    }
}
