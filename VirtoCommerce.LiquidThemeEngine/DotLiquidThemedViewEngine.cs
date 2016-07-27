using System;
using System.Web.Mvc;

namespace VirtoCommerce.LiquidThemeEngine
{
    public class DotLiquidThemedViewEngine : IViewEngine
    {
        private ShopifyLiquidThemeEngine _themeEngine;

        public DotLiquidThemedViewEngine(ShopifyLiquidThemeEngine themeEngine)
        {
            _themeEngine = themeEngine;
        }

        #region IViewEngine members
        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            string[] locations;
            if (_themeEngine.ResolveTemplatePath(partialViewName, out locations) != null)
            {
                return new ViewEngineResult(new DotLiquidThemedView(_themeEngine, partialViewName, null), this);
            }

            return new NullViewEngineResult(locations);
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            string[] locations;
            if (String.IsNullOrEmpty(masterName))
            {
                masterName = _themeEngine.MasterViewName;
            }

            if (_themeEngine.ResolveTemplatePath(viewName, out locations) != null)
            {
                return new ViewEngineResult(new DotLiquidThemedView(_themeEngine, viewName, masterName), this);
            }

            return new NullViewEngineResult(locations);
        }

        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
            //Nothing todo  
        }
        #endregion
    }

    /// <summary>
    /// Class used to tell ASp.NET View locator what view not exist (because no other way to construct ViewEngineResult with null View)
    /// </summary>
    public class NullViewEngineResult : ViewEngineResult
    {
        public NullViewEngineResult(string[] searchedLocations)
            : base(searchedLocations)
        {
            View = null;
        }
        public new IView View { get; private set; }
    }
}
