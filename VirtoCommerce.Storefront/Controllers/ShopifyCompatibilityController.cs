using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using VirtoCommerce.LiquidThemeEngine.Converters;
using VirtoCommerce.LiquidThemeEngine.Filters;
using VirtoCommerce.LiquidThemeEngine.Objects;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart.Services;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Services;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model.Common.Exceptions;

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

            await _cartBuilder.ChangeItemQuantityAsync(line - 1, quantity);
            await _cartBuilder.SaveAsync();

            return StoreFrontRedirect("~/cart");
        }

        // POST: /cart/add
        [HttpPost]
        public async Task<ActionResult> Add(string id, int quantity)
        {
            EnsureThatCartExist();

            var product = (await _catalogService.GetProductsAsync(new string[] { id }, Model.Catalog.ItemResponseGroup.ItemLarge)).FirstOrDefault();
            if (product != null)
            {
                await _cartBuilder.AddItem(product, quantity).SaveAsync();
            }

            return StoreFrontRedirect("~/cart");
        }

        // POST: /cart?updates=...&update=...
        [HttpPost]
        public async Task<ActionResult> Cart(int[] updates, string checkout)
        {
            EnsureThatCartExist();
            await _cartBuilder.ChangeItemsQuantitiesAsync(updates);
            await _cartBuilder.SaveAsync();

            string virtualRedirectUrl = "~/cart";

            if (Request.Form.Get("checkout") != null)
            {

                virtualRedirectUrl = "~/cart/checkout";

            }

            return StoreFrontRedirect(virtualRedirectUrl);
        }

        // GET: /cart/clear
        [HttpGet]
        public async Task<ActionResult> Clear()
        {
            EnsureThatCartExist();

            await _cartBuilder.Clear().SaveAsync();

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

            var product = (await _catalogService.GetProductsAsync(new[] { id }, Model.Catalog.ItemResponseGroup.ItemLarge)).FirstOrDefault();
            if (product != null)
            {
                await _cartBuilder.AddItem(product, quantity).SaveAsync();

                var addedItem = _cartBuilder.Cart.Items.FirstOrDefault(i => i.ProductId == id);
                if (addedItem != null)
                {
                    lineItem = addedItem.ToShopifyModel(WorkContext);
                }
            }

            return LiquidJson(lineItem);
        }

        // POST: /cart/change.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> ChangeJs(string id, int quantity)
        {
            EnsureThatCartExist();
            await _cartBuilder.ChangeItemQuantityAsync(id, quantity);
            await _cartBuilder.SaveAsync();

            return LiquidJson(_cartBuilder.Cart.ToShopifyModel(WorkContext));
        }

        // POST: /cart/update.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> UpdateJs(int[] updates)
        {
            EnsureThatCartExist();
            await _cartBuilder.ChangeItemsQuantitiesAsync(updates);
            await _cartBuilder.SaveAsync();

            return LiquidJson(_cartBuilder.Cart.ToShopifyModel(WorkContext));
        }

        // POST: /cart/clear.js
        [HttpPost]
        [HandleJsonErrorAttribute]
        public async Task<ActionResult> ClearJs()
        {
            EnsureThatCartExist();

            await _cartBuilder.Clear().SaveAsync();

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
                ContractResolver = new RubyContractResolver(),
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
    }
}