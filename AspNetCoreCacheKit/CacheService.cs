using AspNetCoreCacheKit.Interfaces;
using AspNetCoreCacheKit.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AspNetCoreCacheKit
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheOptions _cacheOptions;

        public CacheService(IMemoryCache memoryCache, IOptions<CacheOptions> cacheOptions)
        {
            ArgumentNullException.ThrowIfNull(memoryCache);
            ArgumentNullException.ThrowIfNull(cacheOptions);

            _memoryCache = memoryCache;
            _cacheOptions = cacheOptions.Value;
        }

        public void Set<T>(string groupKey, string key, T value, TimeSpan? duration = null)
        {
            if (_cacheOptions.IsEnabled)
            {
                var fullKey = GetFullKey(groupKey, key);
                _memoryCache.Set(fullKey, value, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ResolveExpiration(groupKey, duration)
                });
            }
        }

        public async Task<T?> GetOrCreateAsync<T>(
            string groupKey,
            string key,
            Func<ICacheEntry, Task<T>> createFunc,
            TimeSpan? duration = null,
            CancellationToken cancellationToken = default)
        {
            if (!_cacheOptions.IsEnabled)
                return await createFunc(new NullCacheEntry());

            cancellationToken.ThrowIfCancellationRequested();

            var expiration = ResolveExpiration(groupKey, duration);

            return await _memoryCache.GetOrCreateAsync(
                GetFullKey(groupKey, key),
                entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = expiration;
                    return createFunc(entry);
                });
        }

        public T? GetOrCreate<T>(
            string groupKey,
            string key,
            Func<T> createFunc,
            TimeSpan? duration = null)
        {
            if (!_cacheOptions.IsEnabled)
                return createFunc();

            var expiration = ResolveExpiration(groupKey, duration);

            return _memoryCache.GetOrCreate(
                GetFullKey(groupKey, key),
                entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = expiration;
                    return createFunc();
                });
        }

        public void Delete(string groupKey, string key)
        {
            if (_cacheOptions.IsEnabled)
                _memoryCache.Remove(GetFullKey(groupKey, key));
        }

        public void Set<T>(string key, T value, TimeSpan? duration = null)
            => Set(string.Empty, key, value, duration);

        public async Task<T?> GetOrCreateAsync<T>(
            string key,
            Func<ICacheEntry, Task<T>> createFunc,
            TimeSpan? duration = null,
            CancellationToken cancellationToken = default)
            => await GetOrCreateAsync(string.Empty, key, createFunc, duration, cancellationToken);

        public T? GetOrCreate<T>(string key, Func<T> createFunc, TimeSpan? duration = null)
            => GetOrCreate(string.Empty, key, createFunc, duration);

        public void Delete(string key)
            => Delete(string.Empty, key);

        private static string GetFullKey(string groupKey, string key)
            => string.IsNullOrEmpty(groupKey) ? key : $"{groupKey}:{key}";

        private TimeSpan ResolveExpiration(string groupKey, TimeSpan? duration)
            => duration ?? _cacheOptions.GetGroupDuration(groupKey) ?? _cacheOptions.Duration;

    }
}
