using VirtoCommerce.LiquidThemeEngine.Objects;
using StorefrontModel = VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.LiquidThemeEngine.Converters
{
    public static class RegisterFormConverter
    {
        public static Form ToShopifyModel(this StorefrontModel.Register storefrontModel)
        {
            var shopifyModel = new Form();
            
            shopifyModel.FirstName = storefrontModel.FirstName;
            shopifyModel.LastName = storefrontModel.LastName;
            shopifyModel.Email = storefrontModel.Email;

            shopifyModel.PasswordNeeded = false;

            return shopifyModel;
        }
    }
}
