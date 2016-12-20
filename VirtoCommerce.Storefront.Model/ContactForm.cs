using System.Collections.Generic;

namespace VirtoCommerce.Storefront.Model
{
    public partial class ContactUsForm
    {
        public ContactUsForm()
        {
            Contact = new Dictionary<string, string[]>();
        }

        public IDictionary<string, string[]> Contact { get; set; }
        public string FormType { get; set; }
    }
}
