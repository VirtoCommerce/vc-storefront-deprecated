using Microsoft.Owin;
using Microsoft.Practices.Unity;
using System;
using System.Threading.Tasks;

namespace VirtoCommerce.Storefront.Owin
{
    internal class OriginalHostRewriterOwinMiddleware : OwinMiddleware
    {
        private readonly UnityContainer _container;

        public OriginalHostRewriterOwinMiddleware(OwinMiddleware next, UnityContainer container)
            : base(next)
        {
            _container = container;
        }

        public override async Task Invoke(IOwinContext context)
        {
            string originalHost = context.Request.Headers["X-Original-Host"];
            if (originalHost != null && !String.Equals(context.Request.Host.Value, originalHost, StringComparison.OrdinalIgnoreCase))
                context.Request.Host = new Microsoft.Owin.HostString(originalHost);

            await Next.Invoke(context);
        }
    }
}