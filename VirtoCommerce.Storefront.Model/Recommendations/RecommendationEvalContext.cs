using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.Storefront.Model.Recommendations
{
    public partial class RecommendationEvalContext
    {
        public RecommendationEvalContext()
        {
            ProductIds = new List<string>();
            Take = 20;
        }
        
        public string Provider { get; set;}

        public string Type { get; set; }

        public string StoreId { get; set; }

        public string UserId { get; set; }

        public ICollection<string> ProductIds { get; set; }

        public int Take { get; set; }    
       
        public string ModelId { get; set; }

        public string BuildId { get; set; }

    }
}
