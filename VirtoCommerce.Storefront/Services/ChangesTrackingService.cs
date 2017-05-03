using System;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Storefront.AutoRestClients.CacheModuleApi;

namespace VirtoCommerce.Storefront.Services
{
    public class ChangesTrackingService : IChangesTrackingService
    {
        private readonly bool _trackChanges;
        private readonly TimeSpan _interval;
        private readonly ICacheModuleApiClient _cacheApi;

        private readonly object _changesTrackingLockObject = new object();

        private DateTime _lastRequestDateTime;
        private long _lastModifiedDateTicks;

        public ChangesTrackingService(bool trackChanges, TimeSpan interval, ICacheModuleApiClient cacheApi)
        {
            _trackChanges = trackChanges;
            _interval = interval;
            _cacheApi = cacheApi;
        }

        public virtual async Task<bool> HasChanges()
        {
            return _trackChanges && IsItTimeToCheckForChanges() && await HasChangesInternal();
        }


        protected virtual bool IsItTimeToCheckForChanges()
        {
            var result = false;

            lock (_changesTrackingLockObject)
            {
                var now = DateTime.UtcNow;
                var timePassed = now - _lastRequestDateTime;

                if (timePassed > _interval)
                {
                    result = true;
                    _lastRequestDateTime = now;
                }
            }

            return result;
        }

        protected virtual async Task<bool> HasChangesInternal()
        {
            var response = await _cacheApi.ChangesTracking.GetLastModifiedDateAsync();

            var newTicks = response.LastModifiedDate?.Ticks ?? 0;
            var oldTicks = Interlocked.Exchange(ref _lastModifiedDateTicks, newTicks);

            return newTicks > oldTicks;
        }
    }
}
