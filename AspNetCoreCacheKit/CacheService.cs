using System.Collections.Concurrent;
using AspNetCoreCacheKit.Interfaces;
using AspNetCoreCacheKit.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AspNetCoreCacheKit
{
    /// <inheritdoc cref="ICacheService"/>
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheOptions _cacheOptions;

        // ponytail: per-key locks prevent cache stampede on concurrent misses; entries are
        // never pruned, so an unbounded set of distinct keys leaks SemaphoreSlim/object instances.
        // Add refcounted eviction if key cardinality becomes large enough to matter.
        private readonly ConcurrentDictionary<CacheKey, SemaphoreSlim> _asyncLocks = new();
        private readonly ConcurrentDictionary<CacheKey, object> _syncLocks = new();

        // Structural key instead of "groupKey:key" string concatenation — a bare key containing
        // ':' (e.g. "app:config", a documented convention) can never collide with a real
        // groupKey+key pair, since the two components are compared independently.
        private readonly record struct CacheKey(string GroupKey, string Key);

        /// <summary>Creates a <see cref="CacheService"/> wrapping <paramref name="memoryCache"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="memoryCache"/> or <paramref name="cacheOptions"/> is <c>null</c>.</exception>
        public CacheService(IMemoryCache memoryCache, IOptions<CacheOptions> cacheOptions)
        {
            ArgumentNullException.ThrowIfNull(memoryCache);
            ArgumentNullException.ThrowIfNull(cacheOptions);

            _memoryCache = memoryCache;
            _cacheOptions = cacheOptions.Value;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

            var fullKey = GetFullKey(groupKey, key);

            if (_memoryCache.TryGetValue(fullKey, out T? cached))
                return cached;

            var keyLock = _asyncLocks.GetOrAdd(fullKey, static _ => new SemaphoreSlim(1, 1));
            await keyLock.WaitAsync(cancellationToken);
            try
            {
                if (_memoryCache.TryGetValue(fullKey, out cached))
                    return cached;

                var expiration = ResolveExpiration(groupKey, duration);

                return await _memoryCache.GetOrCreateAsync(
                    fullKey,
                    entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = expiration;
                        return createFunc(entry);
                    });
            }
            finally
            {
                keyLock.Release();
            }
        }

        /// <inheritdoc/>
        public T? GetOrCreate<T>(
            string groupKey,
            string key,
            Func<T> createFunc,
            TimeSpan? duration = null)
        {
            if (!_cacheOptions.IsEnabled)
                return createFunc();

            var fullKey = GetFullKey(groupKey, key);

            if (_memoryCache.TryGetValue(fullKey, out T? cached))
                return cached;

            var keyLock = _syncLocks.GetOrAdd(fullKey, static _ => new object());
            lock (keyLock)
            {
                if (_memoryCache.TryGetValue(fullKey, out cached))
                    return cached;

                var expiration = ResolveExpiration(groupKey, duration);

                return _memoryCache.GetOrCreate(
                    fullKey,
                    entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = expiration;
                        return createFunc();
                    });
            }
        }

        /// <inheritdoc/>
        public void Delete(string groupKey, string key)
        {
            if (_cacheOptions.IsEnabled)
                _memoryCache.Remove(GetFullKey(groupKey, key));
        }

        /// <inheritdoc/>
        public void Set<T>(string key, T value, TimeSpan? duration = null)
            => Set(string.Empty, key, value, duration);

        /// <inheritdoc/>
        public async Task<T?> GetOrCreateAsync<T>(
            string key,
            Func<ICacheEntry, Task<T>> createFunc,
            TimeSpan? duration = null,
            CancellationToken cancellationToken = default)
            => await GetOrCreateAsync(string.Empty, key, createFunc, duration, cancellationToken);

        /// <inheritdoc/>
        public T? GetOrCreate<T>(string key, Func<T> createFunc, TimeSpan? duration = null)
            => GetOrCreate(string.Empty, key, createFunc, duration);

        /// <inheritdoc/>
        public void Delete(string key)
            => Delete(string.Empty, key);

        private static CacheKey GetFullKey(string groupKey, string key)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            return new CacheKey(groupKey ?? string.Empty, key);
        }

        private TimeSpan ResolveExpiration(string groupKey, TimeSpan? duration)
            => duration ?? _cacheOptions.GetGroupDuration(groupKey) ?? _cacheOptions.Duration;

    }
}
