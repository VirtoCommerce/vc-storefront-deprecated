using System.Web.Optimization;

namespace VirtoCommerce.Storefront
{
    public class BundleConfig
    {
        public bool Minify { get; set; }
        public IItemTransform[] CssItemTransforms { get; set; } = { new CssUrlTransform() };

        public virtual void RegisterBundles(BundleCollection bundles)
        {
            #region JS

            bundles.Add(
                CreateScriptBundle("~/default-theme/scripts")
                    .Include("~/App_Data/Themes/default/assets/modernizr.min.js")
                    .Include("~/App_Data/Themes/default/assets/interactor.js")
                    .Include("~/App_Data/Themes/default/assets/ideal-image-slider.min.js")
                    .Include("~/App_Data/Themes/default/assets/ideal-image-slider-bullet-nav.js")
                    .Include("~/App_Data/Themes/default/assets/ideal-image-slider-captions.js")
                    .IncludeDirectory("~/App_Data/Themes/default/assets/js/", "*.js"));

            bundles.Add(
                CreateScriptBundle("~/default-theme/checkout/scripts")
                    .Include("~/App_Data/Themes/default/assets/js/app.js")
                    .Include("~/App_Data/Themes/default/assets/js/services.js")
                    .Include("~/App_Data/Themes/default/assets/js/directives.js")
                    .Include("~/App_Data/Themes/default/assets/js/main.js")
                    .IncludeDirectory("~/App_Data/Themes/default/assets/js/common-components/", "*.js")
                    .IncludeDirectory("~/App_Data/Themes/default/assets/js/checkout/", "*.js"));

            bundles.Add(
                new ScriptBundle("~/default-theme/account/scripts")
                    .Include("~/App_Data/Themes/default/assets/modernizr.min.js")
                    .Include("~/App_Data/Themes/default/assets/js/app.js")
                    .Include("~/App_Data/Themes/default/assets/js/services.js")
                    .Include("~/App_Data/Themes/default/assets/js/main.js")
                    .Include("~/App_Data/Themes/default/assets/js/cart.js")
                    .Include("~/App_Data/Themes/default/assets/js/quote-request.js")
                    .Include("~/App_Data/Themes/default/assets/js/product-compare.js")
                    .IncludeDirectory("~/App_Data/Themes/default/assets/js/common-components/", "*.js")
                    .IncludeDirectory("~/App_Data/Themes/default/assets/js/account/", "*.js"));

            #endregion

            #region CSS

            bundles.Add(
                CreateStyleBundle("~/default-theme/css")
                    .Include("~/App_Data/Themes/default/assets/storefront.css", CssItemTransforms)
                    .Include("~/App_Data/Themes/default/assets/common-components.css", CssItemTransforms)
                    .Include("~/App_Data/Themes/default/assets/ideal-image-slider.css", CssItemTransforms)
                    .Include("~/App_Data/Themes/default/assets/ideal-image-slider-default-theme.css", CssItemTransforms));

            bundles.Add(
                new StyleBundle("~/default-theme/account/css")
                .Include("~/App_Data/Themes/default/assets/account-bootstrap.css", CssItemTransforms)
                .Include("~/App_Data/Themes/default/assets/common-components.css", CssItemTransforms));

            #endregion
        }


        protected virtual ScriptBundle CreateScriptBundle(string virtualPath)
        {
            var bundle = new ScriptBundle(virtualPath);

            if (!Minify)
            {
                bundle.Transforms.Clear();
            }

            return bundle;
        }

        protected virtual StyleBundle CreateStyleBundle(string virtualPath)
        {
            var bundle = new StyleBundle(virtualPath);

            if (!Minify)
            {
                bundle.Transforms.Clear();
            }

            return bundle;
        }
    }
}
