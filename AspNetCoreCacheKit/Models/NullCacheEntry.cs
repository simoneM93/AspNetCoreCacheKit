using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace AspNetCoreCacheKit.Models
{
    internal sealed class NullCacheEntry : ICacheEntry
    {
        public object Key => string.Empty;
        public object? Value { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public IList<IChangeToken> ExpirationTokens { get; } = [];
        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = [];
        public CacheItemPriority Priority { get; set; }
        public long? Size { get; set; }
        public void Dispose() { }
    }
}
