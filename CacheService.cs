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
        private readonly TimeSpan _expiration;

        public CacheService(IMemoryCache memoryCache, IOptionsSnapshot<CacheOptions> cacheOptions)
        {
            _memoryCache = memoryCache;
            _cacheOptions = cacheOptions.Value;
            _expiration = _cacheOptions.Duration;

            ArgumentNullException.ThrowIfNull(_memoryCache);
            ArgumentNullException.ThrowIfNull(_cacheOptions);
        }

        public void Set(string groupKey, string key, object value)
        {
            if (_cacheOptions.IsEnabled)
            {
                var fullKey = GetFullKey(key, groupKey);
                _memoryCache.Set(fullKey, value, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _expiration
                });
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string groupKey, string key, Func<Task<T>> createFunc)
        {
            if (!_cacheOptions.IsEnabled)
                return await createFunc();

            var fullKey = GetFullKey(key, groupKey);

            return await _memoryCache.GetOrCreate(fullKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _expiration;
                return createFunc();
            });
        }

        public T GetOrCreate<T>(string groupKey, string key, Func<T> createFunc)
        {
            if (!_cacheOptions.IsEnabled)
                return createFunc();

            var fullKey = GetFullKey(key, groupKey);

            return _memoryCache.GetOrCreate(fullKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _expiration;
                return createFunc();
            });
        }

        public void Delete(string groupKey, string key)
        {
            if (_cacheOptions.IsEnabled)
                _memoryCache.Remove(GetFullKey(key, groupKey));
        }

        public void Set(string key, object value) => Set(string.Empty, key, value);

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> createFunc) => await GetOrCreateAsync(string.Empty, key, createFunc);

        public T GetOrCreate<T>(string key, Func<T> createFunc) => GetOrCreate(string.Empty, key, createFunc);

        public void Delete(string key) => Delete(string.Empty, key);

        private static string GetFullKey(string groupKey, string key) => $"{groupKey}:{key}";
    }
}
