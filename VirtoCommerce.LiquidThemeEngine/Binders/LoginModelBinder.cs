using System.Web;
using VirtoCommerce.Storefront.Model;

namespace VirtoCommerce.LiquidThemeEngine.Binders
{
    public class LoginModelBinder : BaseModelBinder<Login>
    {
        protected override void ComplementModel(Login model, HttpRequestBase request)
        {
            model.Username = request["customer[user_name]"];
            model.Password = request["customer[password]"];
        }
    }
}
