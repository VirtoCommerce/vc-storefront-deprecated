using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model.Pricing.Factories
{
    public class PricingFactory
    {
        public Pricelist CreatePricelist(Currency currency)
        {
            return new Pricelist(currency);
        }

        public ProductPrice CreateProductPrice(Currency currency)
        {
            return new ProductPrice(currency);
        }
    }
}
