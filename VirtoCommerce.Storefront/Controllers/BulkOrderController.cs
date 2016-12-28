using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.BulkOrder;
using VirtoCommerce.Storefront.Model.Cart.Services;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Common.Exceptions;

namespace VirtoCommerce.Storefront.Controllers
{
    public class BulkOrderController : StorefrontControllerBase
    {
        private readonly ICartBuilder _cartBuilder;

        public BulkOrderController(
            WorkContext workContext,
            IStorefrontUrlBuilder urlBuilder,
            ICartBuilder cartBuilder)
            : base(workContext, urlBuilder)
        {
            _cartBuilder = cartBuilder;
        }

        // GET: /bulkorder
        [HttpGet]
        public ActionResult Index()
        {
            return View("bulk-order", WorkContext);
        }

        // POST: /bulkorder/addfielditems
        [HttpPost]
        public async Task<ActionResult> AddFieldItems(BulkOrderItem[] items)
        {
            await EnsureThatCartExistsAsync();

            items = items.Where(i => !string.IsNullOrEmpty(i.Sku)).ToArray();
            if (items.Length == 0)
            {
                return StoreFrontRedirect("~/bulkorder");
            }

            await _cartBuilder.FillFromBulkOrderItemsAsync(items);
            await _cartBuilder.SaveAsync();

            return StoreFrontRedirect("~/cart");
        }

        // POST: /bulkorder/addcsvitems
        [HttpPost]
        public async Task<ActionResult> AddCsvItems(string csv)
        {
            if (string.IsNullOrEmpty(csv))
            {
                return StoreFrontRedirect("~/bulkorder");
            }

            await EnsureThatCartExistsAsync();

            var items = csv.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                           .Select(csvRecord => GetBulkOrderItemFromCsvRecord(csvRecord)).ToArray();
            if (items.Length == 0)
            {
                return StoreFrontRedirect("~/bulkorder");
            }

            await _cartBuilder.FillFromBulkOrderItemsAsync(items);

            return StoreFrontRedirect("~/cart");
        }

        private async Task EnsureThatCartExistsAsync()
        {
            if (WorkContext.CurrentCart == null)
            {
                throw new StorefrontException("Cart not found");
            }

            await _cartBuilder.TakeCartAsync(WorkContext.CurrentCart);
        }

        private BulkOrderItem GetBulkOrderItemFromCsvRecord(string csvRecord)
        {
            BulkOrderItem bulkOrderItem = null;

            var splitted = csvRecord.Split(',');
            if (splitted.Length == 2)
            {
                int quantity = 0;
                int.TryParse(splitted[1], out quantity);
                if (quantity > 0)
                {
                    bulkOrderItem = new BulkOrderItem
                    {
                        Quantity = quantity,
                        Sku = splitted[0]
                    };
                }
            }

            return bulkOrderItem;
        }
    }
}