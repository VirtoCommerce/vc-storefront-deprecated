using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using VirtoCommerce.Storefront.JsonConverters;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Order;

namespace VirtoCommerce.Storefront.Binders
{
    public class OrderModelBinder<T> : DefaultModelBinder
    {
        private readonly Func<WorkContext> _workContextFactory;
        public OrderModelBinder(Func<WorkContext> workContextFactory)
        {
            _workContextFactory = workContextFactory;
        }

        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (controllerContext == null)
                throw new ArgumentNullException("controllerContext");

            if (!controllerContext.HttpContext.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                return null;

            var workContext = _workContextFactory();
            var currencyCode = bindingContext.ValueProvider.GetValue("currency.code");
            if (currencyCode == null)
            {
                currencyCode = bindingContext.ValueProvider.GetValue("currency");
            }
            if (currencyCode == null)
            {
                throw new ArgumentNullException("PaymentIn.Currency");
            }

            var currency = workContext.AllCurrencies.FirstOrDefault(x => x.Equals(currencyCode.RawValue));
            object retVal = null;
            var type = typeof(T);
            if (type == typeof(Shipment))
            {
                retVal = new Shipment(currency);
            }
            else if (type == typeof(PaymentIn))
            {
                retVal = new PaymentIn(currency);
            }
            controllerContext.HttpContext.Request.InputStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(controllerContext.HttpContext.Request.InputStream);
            var bodyText = reader.ReadToEnd();
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new MoneyJsonConverter(workContext.AllCurrencies));
            settings.Converters.Add(new CurrencyJsonConverter(workContext.AllCurrencies));
            JsonConvert.PopulateObject(bodyText, retVal, settings);
            return retVal;
        }
    }
}