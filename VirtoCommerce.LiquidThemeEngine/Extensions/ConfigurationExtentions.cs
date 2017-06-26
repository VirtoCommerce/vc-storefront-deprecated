using System.Configuration;
using System.Web;
using System.Web.Configuration;

namespace VirtoCommerce.LiquidThemeEngine.Extensions
{
    public static class ConfigurationExtentions
    {
        public static bool CustomErrorsEnabled()
        {
            return GetCustomErrorsMode() == CustomErrorsMode.On || (GetCustomErrorsMode() == CustomErrorsMode.RemoteOnly && !HttpContext.Current.Request.IsLocal);
        }

        private static CustomErrorsMode GetCustomErrorsMode()
        {
            return ((CustomErrorsSection)ConfigurationManager.GetSection("system.web/customErrors")).Mode;
        }
    }
}
