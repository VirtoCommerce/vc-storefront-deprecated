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
            #endregion

            #region CSS
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
