using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Practices.ServiceLocation;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Subscriptions;
using subscriptionDto = VirtoCommerce.Storefront.AutoRestClients.SubscriptionModuleApi.Models;
using orderDto = VirtoCommerce.Storefront.AutoRestClients.OrdersModuleApi.Models;
using VirtoCommerce.Storefront.Common;

namespace VirtoCommerce.Storefront.Converters.Subscriptions
{
    public static class SubscriptionConverterExtension
    {
        public static SubscriptionConverter SubscriptionConverterInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SubscriptionConverter>();
            }
        }

        public static Subscription ToSubscription(this subscriptionDto.Subscription subscriptionDto, ICollection<Currency> availCurrencies, Language language)
        {
            return SubscriptionConverterInstance.ToSubscription(subscriptionDto, availCurrencies, language);
        }

    }

    public class SubscriptionConverter
    {
        public virtual Subscription ToSubscription(subscriptionDto.Subscription subscriptionDto, ICollection<Currency> availCurrencies, Language language)
        {
            var currency = availCurrencies.FirstOrDefault(x => x.Equals(subscriptionDto.CustomerOrderPrototype.Currency)) ?? new Currency(language, subscriptionDto.CustomerOrderPrototype.Currency);
            var retVal = subscriptionDto.CustomerOrderPrototype.JsonConvert<orderDto.CustomerOrder>()
                                                                .ToCustomerOrder(availCurrencies, language)
                                                                .JsonConvert<Subscription>();
            retVal.Balance = new Money(subscriptionDto.Balance ?? 0, currency);
            retVal.Interval = EnumUtility.SafeParse<PaymentInterval>(subscriptionDto.Interval, PaymentInterval.Months);
            retVal.IntervalCount = subscriptionDto.IntervalCount ?? 0;
            retVal.TrialPeriodDays = subscriptionDto.TrialPeriodDays ?? 0;
            retVal.CustomerOrdersIds = subscriptionDto.CustomerOrdersIds;
            retVal.StartDate = subscriptionDto.StartDate;
            retVal.EndDate = subscriptionDto.EndDate;
            retVal.TrialSart = subscriptionDto.TrialSart;
            retVal.TrialEnd = subscriptionDto.TrialEnd;
            retVal.CurrentPeriodStart = subscriptionDto.CurrentPeriodStart;
            retVal.CurrentPeriodEnd = subscriptionDto.CurrentPeriodEnd;

            return retVal;
        }
     
    }
}