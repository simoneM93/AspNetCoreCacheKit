using Microsoft.Extensions.Caching.Memory;

namespace AspNetCoreCacheKit.Interfaces
{
    /// <summary>
    /// Group-based caching abstraction over <see cref="IMemoryCache"/> with per-entry and
    /// per-group expiration.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>Writes <paramref name="value"/> to the cache under <paramref name="groupKey"/> + <paramref name="key"/>.</summary>
        /// <param name="groupKey">Logical group the entry belongs to; used to resolve the group's configured duration.</param>
        /// <param name="key">Entry key, unique within <paramref name="groupKey"/>.</param>
        /// <param name="value">Value to store.</param>
        /// <param name="duration">Explicit expiration override; falls back to the group duration, then the global duration.</param>
        void Set<T>(string groupKey, string key, T value, TimeSpan? duration = null);

        /// <summary>Writes <paramref name="value"/> to the cache under <paramref name="key"/>, with no group.</summary>
        /// <param name="key">Entry key.</param>
        /// <param name="value">Value to store.</param>
        /// <param name="duration">Explicit expiration override; falls back to the global duration.</param>
        void Set<T>(string key, T value, TimeSpan? duration = null);

        /// <summary>
        /// Returns the cached value for <paramref name="groupKey"/> + <paramref name="key"/>, or
        /// invokes <paramref name="createFunc"/> and caches its result on a miss.
        /// </summary>
        /// <param name="groupKey">Logical group the entry belongs to; used to resolve the group's configured duration.</param>
        /// <param name="key">Entry key, unique within <paramref name="groupKey"/>.</param>
        /// <param name="createFunc">Factory invoked on a cache miss.</param>
        /// <param name="duration">Explicit expiration override; falls back to the group duration, then the global duration.</param>
        /// <param name="cancellationToken">See remarks.</param>
        /// <remarks>
        /// <paramref name="cancellationToken"/> is only checked before the cache lookup starts.
        /// It does not cancel an in-flight <paramref name="createFunc"/> call or an in-flight
        /// wait for a concurrent caller already populating the same key — cancel that behavior
        /// inside <paramref name="createFunc"/> itself if needed.
        /// </remarks>
        Task<T?> GetOrCreateAsync<T>(
            string groupKey,
            string key,
            Func<ICacheEntry, Task<T>> createFunc,
            TimeSpan? duration = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the cached value for <paramref name="key"/>, or invokes
        /// <paramref name="createFunc"/> and caches its result on a miss. No group.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <param name="createFunc">Factory invoked on a cache miss.</param>
        /// <param name="duration">Explicit expiration override; falls back to the global duration.</param>
        /// <param name="cancellationToken">See remarks.</param>
        /// <remarks>
        /// <paramref name="cancellationToken"/> is only checked before the cache lookup starts.
        /// It does not cancel an in-flight <paramref name="createFunc"/> call or an in-flight
        /// wait for a concurrent caller already populating the same key — cancel that behavior
        /// inside <paramref name="createFunc"/> itself if needed.
        /// </remarks>
        Task<T?> GetOrCreateAsync<T>(
           string key,
           Func<ICacheEntry, Task<T>> createFunc,
           TimeSpan? duration = null,
           CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the cached value for <paramref name="key"/>, or invokes
        /// <paramref name="createFunc"/> and caches its result on a miss. No group.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <param name="createFunc">Factory invoked on a cache miss.</param>
        /// <param name="duration">Explicit expiration override; falls back to the global duration.</param>
        T? GetOrCreate<T>(string key, Func<T> createFunc, TimeSpan? duration = null);

        /// <summary>
        /// Returns the cached value for <paramref name="groupKey"/> + <paramref name="key"/>, or
        /// invokes <paramref name="createFunc"/> and caches its result on a miss.
        /// </summary>
        /// <param name="groupKey">Logical group the entry belongs to; used to resolve the group's configured duration.</param>
        /// <param name="key">Entry key, unique within <paramref name="groupKey"/>.</param>
        /// <param name="createFunc">Factory invoked on a cache miss.</param>
        /// <param name="duration">Explicit expiration override; falls back to the group duration, then the global duration.</param>
        T? GetOrCreate<T>(
            string groupKey,
            string key,
            Func<T> createFunc,
            TimeSpan? duration = null);

        /// <summary>Removes the entry at <paramref name="groupKey"/> + <paramref name="key"/>, if present.</summary>
        void Delete(string groupKey, string key);

        /// <summary>Removes the entry at <paramref name="key"/>, if present. No group.</summary>
        void Delete(string key);
    }
}
