using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.Storefront.Model.LinkList.Factories
{
    public class LinkListFactory
    {
        public virtual MenuLinkList CreateMenuLinkList()
        {
            return new MenuLinkList();
        }

        public virtual MenuLink CreateMenuLink(string type)
        {
            var result = new MenuLink();
            if (type != null)
            {
                if ("product" == type.ToLowerInvariant())
                {
                    result = new ProductMenuLink();
                }
                else if ("category" == type.ToLowerInvariant())
                {
                    result = new CategoryMenuLink();
                }
            }
            return result;
        }
    }
}
