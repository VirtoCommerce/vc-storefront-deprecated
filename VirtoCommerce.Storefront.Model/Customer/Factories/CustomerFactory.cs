using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.Storefront.Model.Customer.Factories
{
    public class CustomerFactory
    {
        public CustomerInfo CreateCustomerInfo()
        {
            return new CustomerInfo();
        }

        public Vendor CreateVendor()
        {
            return new Vendor();
        }
    }
}
