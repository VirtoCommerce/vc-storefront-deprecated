using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using VirtoCommerce.Storefront.AutoRestClients.ProductRecommendationsModuleApi;
using VirtoCommerce.Storefront.AutoRestClients.ProductRecommendationsModuleApi.Models;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Recommendations;
using VirtoCommerce.Storefront.Model.Interaction;

namespace VirtoCommerce.Storefront.Controllers.Api
{
    [HandleJsonError]
    public class ApiUserActionsController : StorefrontControllerBase
    {

        public ApiUserActionsController(WorkContext workContext, IStorefrontUrlBuilder urlBuilder) : base(workContext, urlBuilder)
        {
        }

        [HttpPost]
        public ActionResult SaveEventInfo(UserSession userSession)
        {
            throw new NotImplementedException();
        }
    }
}
