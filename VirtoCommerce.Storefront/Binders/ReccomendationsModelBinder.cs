using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using VirtoCommerce.Storefront.JsonConverters;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Recommendations;

namespace VirtoCommerce.Storefront.Binders
{
    public class ReccomendationsModelBinder<T> : DefaultModelBinder
    {
   
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (controllerContext == null)
                throw new ArgumentNullException("controllerContext");

            if (!controllerContext.HttpContext.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                return null;

            var retVal = new RecommendationEvalContext();
            var provider = bindingContext.ValueProvider.GetValue("provider");
            if (provider != null && provider.RawValue != null && provider.RawValue.ToString().EqualsInvariant("Cognitive"))
            {
                retVal = new CognitiveRecommendationEvalContext();
            }
            controllerContext.HttpContext.Request.InputStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(controllerContext.HttpContext.Request.InputStream);
            var bodyText = reader.ReadToEnd();
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            JsonConvert.PopulateObject(bodyText, retVal, settings);
            return retVal;
        }
    }
}