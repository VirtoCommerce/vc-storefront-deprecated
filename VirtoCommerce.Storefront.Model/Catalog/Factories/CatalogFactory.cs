using System;
using System.Collections.Generic;
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
    }
}
