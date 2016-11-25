using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.ContentModuleApi;
using VirtoCommerce.Storefront.Converters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.LinkList.Services;
using VirtoCommerce.Storefront.Model.Services;

namespace VirtoCommerce.Storefront.Services
{
    public class MenuLinkListServiceImpl : IMenuLinkListService
    {
        private readonly IContentModuleApiClient _cmsApi;
        private readonly ICatalogSearchService _catalogSearchService;

        public MenuLinkListServiceImpl(IContentModuleApiClient cmsApi, ICatalogSearchService catalogSearchService)
        {
            _cmsApi = cmsApi;
            _catalogSearchService = catalogSearchService;
        }

        public async Task<MenuLinkList[]> LoadAllStoreLinkListsAsync(string storeId)
        {
            var retVal = new List<MenuLinkList>();
            var linkLists = await _cmsApi.Menu.GetListsAsync(storeId);
            if (linkLists != null)
            {
                retVal.AddRange(linkLists.Select(x => x.ToMenuLinkList()));

                var allMenuLinks = retVal.SelectMany(x => x.MenuLinks).ToList();
                var productLinks = allMenuLinks.OfType<ProductMenuLink>().ToList();
                var categoryLinks = allMenuLinks.OfType<CategoryMenuLink>().ToList();

                Task<Product[]> productsLoadingTask = null;
                Task<Category[]> categoriesLoadingTask = null;

                //Parallel loading associated objects
                var productIds = productLinks.Select(x => x.AssociatedObjectId).ToArray();
                if (productIds.Any())
                {
                    productsLoadingTask = _catalogSearchService.GetProductsAsync(productIds, ItemResponseGroup.ItemSmall);
                }
                var categoriesIds = categoryLinks.Select(x => x.AssociatedObjectId).ToArray();
                if (categoriesIds.Any())
                {
                    categoriesLoadingTask = _catalogSearchService.GetCategoriesAsync(categoriesIds, CategoryResponseGroup.Info | CategoryResponseGroup.WithImages | CategoryResponseGroup.WithSeo | CategoryResponseGroup.WithOutlines);
                }
                //Populate link by associated product
                if (productsLoadingTask != null)
                {
                    var products = await productsLoadingTask;
                    foreach (var productLink in productLinks)
                    {
                        productLink.Product = products.FirstOrDefault(x => x.Id == productLink.AssociatedObjectId);
                    }
                }
                //Populate link by associated category
                if (categoriesLoadingTask != null)
                {
                    var categories = await categoriesLoadingTask;
                    foreach (var categoryLink in categoryLinks)
                    {
                        categoryLink.Category = categories.FirstOrDefault(x => x.Id == categoryLink.AssociatedObjectId);
                    }
                }

            }
            return retVal.ToArray();
        }
    }
}
