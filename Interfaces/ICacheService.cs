using Microsoft.Extensions.Caching.Memory;

namespace AspNetCoreCacheKit.Interfaces
{
    public interface ICacheService
    {
        void Set(string groupKey, string key, object value, TimeSpan? duration = null);

        void Set(string key, object value, TimeSpan? duration = null);

        Task<T?> GetOrCreateAsync<T>(
            string groupKey,
            string key,
            Func<ICacheEntry, Task<T>> createFunc,
            TimeSpan? duration = null,
            CancellationToken cancellationToken = default);

        Task<T?> GetOrCreateAsync<T>(
           string key,
           Func<ICacheEntry, Task<T>> createFunc,
           TimeSpan? duration = null,
           CancellationToken cancellationToken = default);

        T? GetOrCreate<T>(string key, Func<T> createFunc, TimeSpan? duration = null);

        T? GetOrCreate<T>(
            string groupKey,
            string key,
            Func<T> createFunc,
            TimeSpan? duration = null);

        void Delete(string groupKey, string key);

        void Delete(string key);
    }
}
