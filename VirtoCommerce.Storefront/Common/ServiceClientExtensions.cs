using Microsoft.Rest;

namespace VirtoCommerce.Storefront.Common
{
    public static class ServiceClientExtensions
    {
        public static T DisableRetries<T>(this T client)
            where T : ServiceClient<T>
        {
            client.SetRetryPolicy(null);
            return client;
        }
    }
}
