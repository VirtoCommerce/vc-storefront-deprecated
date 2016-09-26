using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart.Services;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Services;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model.Common.Exceptions;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.LiquidThemeEngine.Converters;

namespace VirtoCommerce.Storefront.Controllers
{
    public class ShopifyCompatibilityController : StorefrontControllerBase
    {
        private readonly WorkContext _workContext;
        private readonly IStorefrontUrlBuilder _urlBuilder;
        private readonly ICartBuilder _cartBuilder;
        private readonly ICatalogSearchService _catalogService;

        public ShopifyCompatibilityController(WorkContext workContext, IStorefrontUrlBuilder urlBuilder, ICartBuilder cartBuilder, ICatalogSearchService catalogService)
            : base(workContext, urlBuilder)
        {
            _workContext = workContext;
            _urlBuilder = urlBuilder;
            _cartBuilder = cartBuilder;
            _catalogService = catalogService;
        }

        // GET: /cart/change?line=...&quantity=...
        [HttpGet]
        public async Task<ActionResult> Change(int line, int quantity)
        {
            EnsureThatCartExist();
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
            EnsureThatCartExist();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                var product = (await _catalogService.GetProductsAsync(new string[] { id }, Model.Catalog.ItemResponseGroup.ItemLarge)).FirstOrDefault();
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
            
            EnsureThatCartExist();
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
            EnsureThatCartExist();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.Clear().SaveAsync();
            }
            return StoreFrontRedirect("~/cart");
        }

        // GET: /cart.js
        [HttpGet]
        [HandleJsonErrorAttribute]
        public ActionResult CartJs()
        {
            EnsureThatCartExist();

            return LiquidJson(_cartBuilder.Cart.ToShopifyModel(WorkContext));
        }

        // POST: /cart/add.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> AddJs(string id, int quantity = 1)
        {
            LineItem lineItem = null;

            EnsureThatCartExist();
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
            return LiquidJson(lineItem != null ? lineItem.ToShopifyModel(_workContext) : null);
        }

        // POST: /cart/change.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> ChangeJs(string id, int quantity)
        {
            EnsureThatCartExist();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.ChangeItemQuantityAsync(id, quantity);
                await _cartBuilder.SaveAsync();
            }
            return LiquidJson(_cartBuilder.Cart.ToShopifyModel(WorkContext));
        }

        // POST: /cart/update.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> UpdateJs(int[] updates)
        {
            EnsureThatCartExist();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.ChangeItemsQuantitiesAsync(updates);
                await _cartBuilder.SaveAsync();
            }
            return LiquidJson(_cartBuilder.Cart.ToShopifyModel(WorkContext));
        }

        // POST: /cart/clear.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> ClearJs()
        {
            EnsureThatCartExist();
            using (await AsyncLock.GetLockByKey(GetAsyncLockCartKey(_workContext.CurrentCart)).LockAsync())
            {
                await _cartBuilder.Clear().SaveAsync();
            }
            return LiquidJson(_cartBuilder.Cart.ToShopifyModel(WorkContext));
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

        private void EnsureThatCartExist()
        {
            if (WorkContext.CurrentCart == null)
            {
                throw new StorefrontException("Cart not found");
            }
            _cartBuilder.TakeCart(WorkContext.CurrentCart);
        }

        private static string GetAsyncLockCartKey(ShoppingCart cart)
        {
            return string.Join(":", "Cart", cart.Id, cart.Name, cart.CustomerId);
        }
    }
}