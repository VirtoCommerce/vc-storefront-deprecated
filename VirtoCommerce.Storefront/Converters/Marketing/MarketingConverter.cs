using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Omu.ValueInjecter;
using VirtoCommerce.Storefront.Common;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Cart;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Marketing;
using VirtoCommerce.Storefront.Model.Marketing.Factories;
using marketingDTO = VirtoCommerce.Storefront.AutoRestClients.MarketingModuleApi.Models;
using coreDTO = VirtoCommerce.Storefront.AutoRestClients.CoreModuleApi.Models;
namespace VirtoCommerce.Storefront.Converters
{
    
    public static class MarketingConverterExtension
    {
        public static MarketingConverter MarketingConverterInstance
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MarketingConverter>();
            }
        }

        public static PromotionEvaluationContext ToPromotionEvaluationContext(this WorkContext workContext, IEnumerable<Product> products = null)
        {           
            return MarketingConverterInstance.ToPromotionEvaluationContext(workContext, products);
        }

        public static marketingDTO.PromotionEvaluationContext ToPromotionEvaluationContextDTO(this PromotionEvaluationContext promoEvaluationContext)
        {
            return MarketingConverterInstance.ToPromotionEvaluationContextDTO(promoEvaluationContext);
        }

        public static Promotion ToWebModel(this marketingDTO.Promotion promotionDTO)
        {
            return MarketingConverterInstance.ToPromotion(promotionDTO);
        }

        public static marketingDTO.ProductPromoEntry ToProductPromoEntryDTO(this PromotionProductEntry promoProductEntry)
        {
            return MarketingConverterInstance.ToProductPromoEntryDTO(promoProductEntry);
        }

        public static PromotionReward ToPromotionReward(this marketingDTO.PromotionReward rewardDTO, Currency currency)
        {
            return MarketingConverterInstance.ToPromotionReward(rewardDTO, currency);
        }

        public static DynamicContentItem ToDynamicContentItem(this marketingDTO.DynamicContentItem contentItemDTO)
        {
            return MarketingConverterInstance.ToDynamicContentItem(contentItemDTO);
        }

        public static DynamicProperty ToDynamicProperty(this marketingDTO.DynamicObjectProperty propertyDTO)
        {
            return MarketingConverterInstance.ToDynamicProperty(propertyDTO);
        }

        public static marketingDTO.DynamicObjectProperty ToMarketingDynamicPropertyDTO(this DynamicProperty property)
        {
            return MarketingConverterInstance.ToMarketingDynamicPropertyDTO(property);
        }
    }

    public class MarketingConverter
    {
        public virtual DynamicProperty ToDynamicProperty(marketingDTO.DynamicObjectProperty propertyDTO)
        {
            return propertyDTO.JsonConvert<coreDTO.DynamicObjectProperty>().ToDynamicProperty();
        }

        public virtual marketingDTO.DynamicObjectProperty ToMarketingDynamicPropertyDTO(DynamicProperty property)
        {
            return property.ToDynamicPropertyDTO().JsonConvert<marketingDTO.DynamicObjectProperty>();
        }

        public virtual DynamicContentItem ToDynamicContentItem(marketingDTO.DynamicContentItem contentItemDTO)
        {
            var result = ServiceLocator.Current.GetInstance<MarketingFactory>().CreateDynamicContentItem();

            result.InjectFrom<NullableAndEnumValueInjecter>(contentItemDTO);

            if (contentItemDTO.DynamicProperties != null)
            {
                result.DynamicProperties = contentItemDTO.DynamicProperties.Select(ToDynamicProperty).ToList();
            }

            return result;
        }

        public virtual PromotionReward ToPromotionReward(marketingDTO.PromotionReward serviceModel, Currency currency)
        {
            var result = ServiceLocator.Current.GetInstance<MarketingFactory>().CreatePromotionReward();

            result.InjectFrom<NullableAndEnumValueInjecter>(serviceModel);

            result.Amount = (decimal)(serviceModel.Amount ?? 0);
            result.AmountType = EnumUtility.SafeParse(serviceModel.AmountType, AmountType.Absolute);
            result.CouponAmount = new Money(serviceModel.CouponAmount ?? 0, currency);
            result.CouponMinOrderAmount = new Money(serviceModel.CouponMinOrderAmount ?? 0, currency);
            result.Promotion = serviceModel.Promotion.ToWebModel();
            result.RewardType = EnumUtility.SafeParse(serviceModel.RewardType, PromotionRewardType.CatalogItemAmountReward);
            result.ShippingMethodCode = serviceModel.ShippingMethod;

            return result;
        }

        public marketingDTO.ProductPromoEntry ToProductPromoEntryDTO(PromotionProductEntry promoProductEntry)
        {
            var serviceModel = new marketingDTO.ProductPromoEntry();

            serviceModel.InjectFrom<NullableAndEnumValueInjecter>(promoProductEntry);

            serviceModel.Discount = promoProductEntry.Discount != null ? (double?)promoProductEntry.Discount.Amount : null;
            serviceModel.Price = promoProductEntry.Price != null ? (double?)promoProductEntry.Price.Amount : null;
            serviceModel.Variations = promoProductEntry.Variations != null ? promoProductEntry.Variations.Select(ToProductPromoEntryDTO).ToList() : null;

            return serviceModel;
        }

        public virtual Promotion ToPromotion(marketingDTO.Promotion promotionDTO)
        {
            var result = ServiceLocator.Current.GetInstance<MarketingFactory>().CreatePromotion();

            result.InjectFrom<NullableAndEnumValueInjecter>(promotionDTO);

            result.Coupons = promotionDTO.Coupons;

            return result;
        }

        public virtual PromotionEvaluationContext ToPromotionEvaluationContext(WorkContext workContext, IEnumerable<Product> products = null)
        {
            var result = ServiceLocator.Current.GetInstance<MarketingFactory>().CreatePromotionEvaluationContext();
            result.CartPromoEntries = workContext.CurrentCart.Items.Select(x => x.ToPromotionItem()).ToList();
            result.CartTotal = workContext.CurrentCart.Total;
            result.Coupon = workContext.CurrentCart.Coupon != null ? workContext.CurrentCart.Coupon.Code : null;
            result.Currency = workContext.CurrentCurrency;
            result.CustomerId = workContext.CurrentCustomer.Id;
            result.IsRegisteredUser = workContext.CurrentCustomer.IsRegisteredUser;
            result.Language = workContext.CurrentLanguage;
            result.StoreId = workContext.CurrentStore.Id;

            //Set cart lineitems as default promo items
            result.PromoEntries = result.CartPromoEntries;

            if (workContext.CurrentProduct != null)
            {
                result.PromoEntry = workContext.CurrentProduct.ToPromotionItem();
            }

            if (products != null)
            {
                result.PromoEntries = products.Select(x => x.ToPromotionItem()).ToList();
            }

            return result;
        }

        public virtual marketingDTO.PromotionEvaluationContext ToPromotionEvaluationContextDTO(PromotionEvaluationContext promoEvalContext)
        {
            var result = new marketingDTO.PromotionEvaluationContext();

            result.InjectFrom<NullableAndEnumValueInjecter>(promoEvalContext);

            result.CartPromoEntries = promoEvalContext.CartPromoEntries.Select(ToProductPromoEntryDTO).ToList();
            result.CartTotal = promoEvalContext.CartTotal != null ? (double?)promoEvalContext.CartTotal.Amount : null;
            result.Currency = promoEvalContext.Currency != null ? promoEvalContext.Currency.Code : null;
            result.Language = promoEvalContext.Language != null ? promoEvalContext.Language.CultureName : null;
            result.PromoEntries = promoEvalContext.PromoEntries.Select(ToProductPromoEntryDTO).ToList();
            result.PromoEntry = promoEvalContext.PromoEntry != null ? ToProductPromoEntryDTO(promoEvalContext.PromoEntry) : null;
            result.ShipmentMethodPrice = promoEvalContext.ShipmentMethodPrice != null ? (double?)promoEvalContext.ShipmentMethodPrice.Amount : null;

            return result;
        }
    }
}
