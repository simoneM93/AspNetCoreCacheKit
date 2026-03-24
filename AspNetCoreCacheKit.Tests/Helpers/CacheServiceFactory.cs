using AspNetCoreCacheKit.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AspNetCoreCacheKit.Tests.Helpers
{
    internal static class CacheServiceFactory
    {
        public static CacheService Create(
            bool isEnabled = true,
            int durationMinutes = 60,
            Dictionary<string, int>? groupDurations = null)
        {
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var options = Options.Create(new CacheOptions
            {
                IsEnabled = isEnabled,
                DurationMinutes = durationMinutes,
                GroupDurations = groupDurations ?? []
            });

            return new CacheService(memoryCache, options);
        }
    }
}
