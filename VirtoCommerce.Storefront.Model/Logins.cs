using System;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.Model
{
    public partial class Logins : ValueObject<Logins>
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public override string ToString()
        {
            var retVal = string.Join(" ", ProviderKey, LoginProvider);
            return retVal;
        }
    }
}
