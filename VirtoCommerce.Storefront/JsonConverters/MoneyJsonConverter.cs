using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Storefront.Model.Common;

namespace VirtoCommerce.Storefront.JsonConverters
{
    public class MoneyJsonConverter : JsonConverter
    {
        private readonly IEnumerable<Currency> _availCurrencies;
        public MoneyJsonConverter(IEnumerable<Currency> availCurrencies)
        {
            _availCurrencies = availCurrencies;
        }


        public override bool CanWrite { get { return false; } }
        public override bool CanRead { get { return true; } }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Money).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Money retVal = null;
            var obj = JObject.Load(reader);

            var currencyCode = obj["currency"]["code"].Value<string>();
            var amount = obj["amount"].Value<decimal>();
            var currency = _availCurrencies.FirstOrDefault(x => x.Equals(currencyCode));
            if (currency == null)
            {
                throw new NotSupportedException("Unknown currency code: " + currencyCode);
            }
            retVal = new Money(amount, currency);

            return retVal;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}