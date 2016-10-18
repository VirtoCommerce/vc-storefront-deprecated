using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.Storefront.Model.Stores.Factories
{
    public class StoreFactory
    {
        public Store CreateStore()
        {
            return new Store();
        }
    }
}
