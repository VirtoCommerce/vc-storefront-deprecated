using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NLog;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Controllers;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront
{
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// Returns cache key
        /// </summary>
        /// <param name="context"></param>
        /// <param name="custom"></param>
        /// <returns></returns>
        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            var varyItems = new List<string>
            {
                base.GetVaryByCustomString(context, custom)
            };

            var customStrings = custom.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var customString in customStrings)
            {
                if (customString.EqualsInvariant("User"))
                {
                    string userId = null;
                    if (!context.User.Identity.IsAuthenticated)
                    {
                        var anonymousCookie = context.Request.Cookies.Get(StorefrontConstants.AnonymousCustomerIdCookie);
                        if (anonymousCookie != null)
                        {
                            userId = anonymousCookie.Value;
                        }
                    }
                    varyItems.Add(userId ?? context.User.Identity.Name);
                }
                else if (customString.EqualsInvariant("Currency"))
                {
                    var currencyCookie = context.Request.Cookies.Get(StorefrontConstants.CurrencyCookie);
                    if (currencyCookie != null)
                    {
                        varyItems.Add(currencyCookie.Value);
                    }
                }
            }

            var result = string.Join("-", varyItems.Where(s => !string.IsNullOrEmpty(s)));
            return result;
        }

        protected void Application_Start()
        {
        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            HttpException httpException = exception as HttpException;
            //ApiException apiException = exception as ApiException;

            var isNotFound = false;
            //if (apiException != null)
            //{
            //    isNotFound = apiException.ErrorCode == 404;
            //}
            if (httpException != null)
            {
                isNotFound = httpException.GetHttpCode() == 404;
            }

            if (isNotFound)
            {
                RouteData routeData = new RouteData();
                routeData.Values.Add("controller", "Error");
                routeData.Values.Add("action", "Http404");
                // Clear the error, otherwise, we will always get the default error page.
                Server.ClearError();
                // Call the controller with the route
                IController errorController = new ErrorController();
                errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
            }

            //Log exception
            var log = LogManager.GetCurrentClassLogger();
            log.Error(exception);

            //Response.Clear();
            //Server.ClearError();
            //Response.TrySkipIisCustomErrors = true;
        }

    }
}
