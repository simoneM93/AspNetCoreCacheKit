using AspNetCoreCacheKit.Interfaces;
using AspNetCoreCacheKit.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreCacheKit.Extensions
{
    public static class CacheServiceExtensions
    {
        public static IServiceCollection AddAspNetCoreCacheKit(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddMemoryCache();

            var cacheSection = configuration.GetSection("CacheOptions");

            services.AddOptions<CacheOptions>()
                .Bind(cacheSection)
                .ValidateDataAnnotations()
                .Validate(
                    options => options.DurationMinutes > 0,
                    "DurationMinutes must be greater than zero.")
                .Validate(
                    options => options.GroupDurations.Values.All(v => v > 0),
                    "All GroupDurations values must be greater than zero.")
                .ValidateOnStart();

            services.AddSingleton<ICacheService, CacheService>();

            return services;
        }

        public static IServiceCollection AddAspNetCoreCacheKit(this IServiceCollection services) =>
            services.AddAspNetCoreCacheKit(new ConfigurationBuilder().Build());
    }
}
