using System;
using System.Collections.Generic;
using System.Text;

namespace VirtoCommerce.Storefront.Model.Security
{
    public class ExternalUserLoginInfo
    {
        public string LoginProvider { get; set; }  
        // Summary:
        //     Gets or sets the unique identifier for the user identity user provided by the
        //     login provider.
        public string ProviderKey { get; set; }
        //
        // Summary:
        //     Gets or sets the display name for the provider.
        public string ProviderDisplayName { get; set; }
    }
}
