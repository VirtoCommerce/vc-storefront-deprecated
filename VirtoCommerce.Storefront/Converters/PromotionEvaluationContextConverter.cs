using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;
using marketingModel = VirtoCommerce.Storefront.AutoRestClients.MarketingModuleApi.Models;

namespace VirtoCommerce.Storefront.Converters
{
    public static class PromotionEvaluationContexStatictConverter
    {
        public static PromotionEvaluationContext ToPromotionEvaluationContext(this ShoppingCart cart)
        {
            var converter = AbstractTypeFactory<PromotionEvaluationContextConverter>.TryCreateInstance();
            return converter.ToPromotionEvaluationContext(cart);
        }

        public static PromotionEvaluationContext ToPromotionEvaluationContext(this WorkContext workContext, IEnumerable<Product> products = null)
        {
            var converter = AbstractTypeFactory<PromotionEvaluationContextConverter>.TryCreateInstance();
            return converter.ToPromotionEvaluationContext(workContext, products);
        }

        public static marketingModel.PromotionEvaluationContext ToServiceModel(this PromotionEvaluationContext webModel)
        {
            var converter = AbstractTypeFactory<PromotionEvaluationContextConverter>.TryCreateInstance();
            return converter.ToServiceModel(webModel);
        }
    }

    public class PromotionEvaluationContextConverter
    {
        public virtual PromotionEvaluationContext ToPromotionEvaluationContext(ShoppingCart cart)
        {
            var promotionItems = cart.Items.Select(i => i.ToPromotionItem()).ToList();

            var retVal = AbstractTypeFactory<PromotionEvaluationContext>.TryCreateInstance();
            retVal.CartPromoEntries = promotionItems;
            retVal.CartTotal = cart.Total;
            retVal.Coupon = cart.Coupon != null ? cart.Coupon.Code : null;
            retVal.Currency = cart.Currency;
            retVal.CustomerId = cart.Customer.Id;
            retVal.IsRegisteredUser = cart.Customer.IsRegisteredUser;
            retVal.Language = cart.Language;
            retVal.PromoEntries = promotionItems;
            retVal.StoreId = cart.StoreId;

            return retVal;
        }

        public virtual PromotionEvaluationContext ToPromotionEvaluationContext(WorkContext workContext, IEnumerable<Product> products = null)
        {
            var retVal = AbstractTypeFactory<PromotionEvaluationContext>.TryCreateInstance();
            retVal.CartPromoEntries = workContext.CurrentCart.Items.Select(x => x.ToPromotionItem()).ToList();
            retVal.CartTotal = workContext.CurrentCart.Total;
            retVal.Coupon = workContext.CurrentCart.Coupon != null ? workContext.CurrentCart.Coupon.Code : null;
            retVal.Currency = workContext.CurrentCurrency;
            retVal.CustomerId = workContext.CurrentCustomer.Id;
            retVal.IsRegisteredUser = workContext.CurrentCustomer.IsRegisteredUser;
            retVal.Language = workContext.CurrentLanguage;
            retVal.StoreId = workContext.CurrentStore.Id;

            //Set cart lineitems as default promo items
            retVal.PromoEntries = retVal.CartPromoEntries;

            if (workContext.CurrentProduct != null)
            {
                retVal.PromoEntry = workContext.CurrentProduct.ToPromotionItem();
            }

            if (products != null)
            {
                retVal.PromoEntries = products.Select(x => x.ToPromotionItem()).ToList();
            }

            return retVal;
        }

        public virtual marketingModel.PromotionEvaluationContext ToServiceModel(PromotionEvaluationContext webModel)
        {
            var serviceModel = AbstractTypeFactory<marketingModel.PromotionEvaluationContext>.TryCreateInstance();
            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(webModel);

            serviceModel.CartPromoEntries = webModel.CartPromoEntries.Select(pe => pe.ToServiceModel()).ToList();
            serviceModel.CartTotal = webModel.CartTotal != null ? (double?)webModel.CartTotal.Amount : null;
            serviceModel.Currency = webModel.Currency != null ? webModel.Currency.Code : null;
            serviceModel.Language = webModel.Language != null ? webModel.Language.CultureName : null;
            serviceModel.PromoEntries = webModel.PromoEntries.Select(pe => pe.ToServiceModel()).ToList();
            serviceModel.PromoEntry = webModel.PromoEntry != null ? webModel.PromoEntry.ToServiceModel() : null;
            serviceModel.ShipmentMethodPrice = webModel.ShipmentMethodPrice != null ? (double?)webModel.ShipmentMethodPrice.Amount : null;

            return serviceModel;
        }
    }
}
