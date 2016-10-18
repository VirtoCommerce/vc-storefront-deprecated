using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PagedList;
using VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.InventoryModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.SearchApiModuleApi;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Customer.Services;
using VirtoCommerce.Storefront.Model.Pricing.Services;
using VirtoCommerce.Storefront.Model.Services;

namespace VirtoCommerce.Storefront.Services
{
    public class CatalogSearchServiceImpl : ICatalogSearchService
    {
        private readonly Func<WorkContext> _workContextFactory;
        private readonly ICatalogModuleApiClient _catalogModuleApi;
        private readonly IInventoryModuleApiClient _inventoryModuleApi;
        private readonly ISearchApiModuleApiClient _searchApi;
        private readonly IPricingService _pricingService;
        private readonly ICustomerService _customerService;

        public CatalogSearchServiceImpl(
            Func<WorkContext> workContextFactory,
            ICatalogModuleApiClient catalogModuleApi,
            IInventoryModuleApiClient inventoryModuleApi,
            ISearchApiModuleApiClient searchApi,
            IPricingService pricingService,
            ICustomerService customerService)
        {
            _workContextFactory = workContextFactory;
            _catalogModuleApi = catalogModuleApi;
            _pricingService = pricingService;
            _inventoryModuleApi = inventoryModuleApi;
            _searchApi = searchApi;
            _customerService = customerService;
        }

        #region ICatalogSearchService Members

        public virtual async Task<Product[]> GetProductsAsync(string[] ids, ItemResponseGroup responseGroup = ItemResponseGroup.None)
        {
            var workContext = _workContextFactory();
            if (responseGroup == ItemResponseGroup.None)
            {
                responseGroup = workContext.CurrentProductResponseGroup;
            }

            var retVal = (await _catalogModuleApi.CatalogModuleProducts.GetProductByIdsAsync(ids.ToList(), ((int)responseGroup).ToString())).Select(x => x.ToProduct(workContext.CurrentLanguage, workContext.CurrentCurrency, workContext.CurrentStore)).ToArray();

            var allProducts = retVal.Concat(retVal.SelectMany(x => x.Variations)).ToList();

            if (!allProducts.IsNullOrEmpty())
            {
                var taskList = new List<Task>();

                if (responseGroup.HasFlag(ItemResponseGroup.ItemAssociations))
                {
                    taskList.Add(LoadProductAssociationsAsync(allProducts));
                }

                if (responseGroup.HasFlag(ItemResponseGroup.Inventory))
                {
                    taskList.Add(LoadProductInventoriesAsync(allProducts));
                }

                if (responseGroup.HasFlag(ItemResponseGroup.ItemWithPrices))
                {
                    await _pricingService.EvaluateProductPricesAsync(allProducts);
                }

                if (responseGroup.HasFlag(ItemResponseGroup.ItemWithVendor))
                {
                    await LoadProductVendorsAsync(allProducts);
                }

                await Task.WhenAll(taskList.ToArray());
            }

            return retVal;
        }

        public virtual async Task<Category[]> GetCategoriesAsync(string[] ids, CategoryResponseGroup responseGroup = CategoryResponseGroup.Info)
        {
            var workContext = _workContextFactory();

            var retVal = (await _catalogModuleApi.CatalogModuleCategories.GetCategoriesByIdsAsync(ids.ToList(), ((int)responseGroup).ToString())).Select(x => x.ToCategory(workContext.CurrentLanguage, workContext.CurrentStore)).ToArray();

            return retVal;
        }

        /// <summary>
        /// Async search categories by given criteria 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<Category>> SearchCategoriesAsync(CategorySearchCriteria criteria)
        {
            var workContext = _workContextFactory();
            criteria = criteria.Clone();
            var searchCriteria = criteria.ToCategorySearchDto(workContext);
            var result = await _searchApi.SearchApiModule.SearchCategoriesAsync(workContext.CurrentStore.Id, searchCriteria);

            //API temporary does not support paginating request to categories (that's uses PagedList with superset instead StaticPagedList)
            return new PagedList<Category>(result.Categories.Select(x => x.ToCategory(workContext.CurrentLanguage, workContext.CurrentStore)), criteria.PageNumber, criteria.PageSize);
        }

        /// <summary>
        /// Search categories by given criteria 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public virtual IPagedList<Category> SearchCategories(CategorySearchCriteria criteria)
        {
            var workContext = _workContextFactory();
            criteria = criteria.Clone();
            var searchCriteria = criteria.ToCategorySearchDto(workContext);
            var categories = _searchApi.SearchApiModule.SearchCategories(workContext.CurrentStore.Id, searchCriteria).Categories.Select(x => x.ToCategory(workContext.CurrentLanguage, workContext.CurrentStore)).ToList();

            //API temporary does not support paginating request to categories (that's uses PagedList with superset)
            return new PagedList<Category>(categories, criteria.PageNumber, criteria.PageSize);
        }

        /// <summary>
        /// Async search products by given criteria 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public virtual async Task<CatalogSearchResult> SearchProductsAsync(ProductSearchCriteria criteria)
        {
            criteria = criteria.Clone();

            var workContext = _workContextFactory();
            var searchCriteria = criteria.ToProductSearchDto(workContext);
            var result = await _searchApi.SearchApiModule.SearchProductsAsync(workContext.CurrentStore.Id, searchCriteria);
            var products = result.Products.Select(x => x.ToProduct(workContext.CurrentLanguage, workContext.CurrentCurrency, workContext.CurrentStore)).ToList();

            if (products.Any())
            {
                var productsWithVariations = products.Concat(products.SelectMany(x => x.Variations)).ToList();
                var taskList = new List<Task>
                {
                    LoadProductInventoriesAsync(productsWithVariations),
                    LoadProductVendorsAsync(productsWithVariations),
                    _pricingService.EvaluateProductPricesAsync(productsWithVariations)
                };
                await Task.WhenAll(taskList.ToArray());
            }

            return new CatalogSearchResult
            {
                Products = new StaticPagedList<Product>(products, criteria.PageNumber, criteria.PageSize, (int?)result.TotalCount ?? 0),
                Aggregations = !result.Aggregations.IsNullOrEmpty() ? result.Aggregations.Select(x => x.ToAggregation(workContext.CurrentLanguage.CultureName)).ToArray() : new Aggregation[] { }
            };
        }


        /// <summary>
        /// Search products by given criteria 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public virtual CatalogSearchResult SearchProducts(ProductSearchCriteria criteria)
        {
            criteria = criteria.Clone();

            var workContext = _workContextFactory();
            var searchCriteria = criteria.ToProductSearchDto(workContext);
            var result = _searchApi.SearchApiModule.SearchProducts(workContext.CurrentStore.Id, searchCriteria);
            var products = result.Products.Select(x => x.ToProduct(workContext.CurrentLanguage, workContext.CurrentCurrency, workContext.CurrentStore)).ToList();

            if (products.Any())
            {
                var productsWithVariations = products.Concat(products.SelectMany(x => x.Variations)).ToList();
                //Unable to make parallel call because its synchronous method (in future this information pricing and inventory will be getting from search index) and this lines can be removed
                _pricingService.EvaluateProductPrices(productsWithVariations);
                LoadProductInventories(productsWithVariations);
                LoadProductVendors(productsWithVariations);
            }

            return new CatalogSearchResult
            {
                Products = new StaticPagedList<Product>(products, criteria.PageNumber, criteria.PageSize, (int?)result.TotalCount ?? 0),
                Aggregations = !result.Aggregations.IsNullOrEmpty() ? result.Aggregations.Select(x => x.ToAggregation(workContext.CurrentLanguage.CultureName)).ToArray() : new Aggregation[] { }
            };
        }

        #endregion

        protected virtual async Task LoadProductVendorsAsync(List<Product> products)
        {
            var vendorIds = products.Where(p => !string.IsNullOrEmpty(p.VendorId)).Select(p => p.VendorId).Distinct().ToArray();
            if (!vendorIds.IsNullOrEmpty())
            {
                var vendors = await _customerService.GetVendorsByIdsAsync(vendorIds);
                foreach (var product in products)
                {
                    product.Vendor = vendors.FirstOrDefault(v => v != null && v.Id == product.VendorId);
                }
            }
        }

        protected virtual void LoadProductVendors(List<Product> products)
        {
            var vendorIds = products.Where(p => !string.IsNullOrEmpty(p.VendorId)).Select(p => p.VendorId).Distinct().ToArray();
            if (!vendorIds.IsNullOrEmpty())
            {
                var vendors = _customerService.GetVendorsByIds(vendorIds);

                foreach (var product in products)
                {
                    product.Vendor = vendors.FirstOrDefault(v => v != null && v.Id == product.VendorId);
                    if (product.Vendor != null)
                    {
                        product.Vendor.Products = new MutablePagedList<Product>((pageNumber, pageSize, sortInfos) =>
                        {
                            var criteria = new ProductSearchCriteria
                            {
                                VendorId = product.VendorId,
                                PageNumber = pageNumber,
                                PageSize = pageSize,
                                ResponseGroup = ItemResponseGroup.ItemSmall,
                                SortBy = SortInfo.ToString(sortInfos),
                            };

                            var searchResult = SearchProducts(criteria);
                            return searchResult.Products;
                        });
                    }
                }
            }
        }

        protected virtual async Task LoadProductAssociationsAsync(IEnumerable<Product> products)
        {
            var allAssociations = products.SelectMany(x => x.Associations).ToList();

            var allProductAssociations = allAssociations.OfType<ProductAssociation>().ToList();
            var allCategoriesAssociations = allAssociations.OfType<CategoryAssociation>().ToList();

            if (allProductAssociations.Any())
            {
                var allAssociatedProducts = await GetProductsAsync(allProductAssociations.Select(x => x.ProductId).ToArray(), ItemResponseGroup.ItemInfo | ItemResponseGroup.ItemWithPrices | ItemResponseGroup.Seo);
                foreach (var productAssociation in allProductAssociations)
                {
                    productAssociation.Product = allAssociatedProducts.FirstOrDefault(x => x.Id == productAssociation.ProductId);
                }
            }

            if (allCategoriesAssociations.Any())
            {
                var allAssociatedCategories = await GetCategoriesAsync(allCategoriesAssociations.Select(x => x.CategoryId).ToArray(), CategoryResponseGroup.Info | CategoryResponseGroup.WithSeo | CategoryResponseGroup.WithOutlines | CategoryResponseGroup.WithImages);

                foreach (var categoryAssociation in allCategoriesAssociations)
                {
                    categoryAssociation.Category = allAssociatedCategories.FirstOrDefault(x => x.Id == categoryAssociation.CategoryId);
                    if (categoryAssociation.Category != null && categoryAssociation.Category.Products == null)
                    {
                        categoryAssociation.Category.Products = new MutablePagedList<Product>((pageNumber, pageSize, sortInfos) =>
                       {
                           var criteria = new ProductSearchCriteria
                           {
                               PageNumber = pageNumber,
                               PageSize = pageSize,
                               Outline = categoryAssociation.Category.Outline,
                               ResponseGroup = ItemResponseGroup.ItemSmall
                           };
                           if (!sortInfos.IsNullOrEmpty())
                           {
                               criteria.SortBy = SortInfo.ToString(sortInfos);
                           }
                           var searchResult = SearchProducts(criteria);
                           return searchResult.Products;
                       });
                    }
                }
            }
        }

        protected virtual async Task LoadProductInventoriesAsync(List<Product> products)
        {
            var inventories = await _inventoryModuleApi.InventoryModule.GetProductsInventoriesAsync(products.Select(x => x.Id).ToList());
            foreach (var item in products)
            {
                item.Inventory = inventories.Where(x => x.ProductId == item.Id).Select(x => x.ToInventory()).FirstOrDefault();
            }
        }

        protected virtual void LoadProductInventories(List<Product> products)
        {
            var inventories = _inventoryModuleApi.InventoryModule.GetProductsInventories(products.Select(x => x.Id).ToList());
            foreach (var item in products)
            {
                item.Inventory = inventories.Where(x => x.ProductId == item.Id).Select(x => x.ToInventory()).FirstOrDefault();
            }
        }
    }
}
