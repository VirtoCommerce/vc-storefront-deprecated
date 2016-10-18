using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model.Catalog.Factories
{
    public class CatalogFactory
    {
        public virtual Product CreateProduct(Currency currency, Language language)
        {
            return new Product(currency, language);
        }

        public virtual Category CreateCategory()
        {
            return new Category();
        }

        public virtual Asset CreateAsset()
        {
            return new Asset();
        }

        public virtual Image CreateImage()
        {
            return new Image();
        }

        public virtual CatalogProperty CreateProperty()
        {
            return new CatalogProperty();
        }

        public virtual CategoryAssociation CreateCategoryAssociation(string categoryId)
        {
            return new CategoryAssociation()
            {
                CategoryId = categoryId
            };
        }

        public virtual ProductAssociation CreateProductAssociation(string productId)
        {
            return new ProductAssociation()
            {
                ProductId = productId
            };

        }

        public virtual Aggregation CreateAggregation()
        {
            return new Aggregation();
        }

        public virtual AggregationItem CreateAggregationItem()
        {
            return new AggregationItem();
        }
               
        public virtual CategorySearchCriteria CreateCategorySearchCriteria(Language language, NameValueCollection queryString = null)
        {
            return new CategorySearchCriteria(language, queryString ?? new NameValueCollection());
        }

        public virtual ProductSearchCriteria CreateProductSearchCriteria(Language language, Currency currency, NameValueCollection queryString = null)
        {
            return new ProductSearchCriteria(language, currency, queryString ?? new NameValueCollection());
        }

    }
}
