using AspNetCoreCacheKit.Interfaces;
using AspNetCoreCacheKit.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreCacheKit.Extensions
{
    public static class CacheServiceExtensions
    {
        public static IServiceCollection AddAspNetCoreCacheKit(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.AddMemoryCache();

            var cacheSection = configuration.GetSection("CacheOptions");
            services.Configure<CacheOptions>(cacheSection);

            services.AddOptions<CacheOptions>()
                .Bind(cacheSection)
                .ValidateDataAnnotations()
                .Validate(options => options.Duration > TimeSpan.Zero, "Duration must be greater than zero")
                .PostConfigure(options =>
                {
                    options.Duration = options.Duration == TimeSpan.Zero ? TimeSpan.FromMinutes(60) : options.Duration;
                })
                .ValidateOnStart();


            services.AddScoped<ICacheService, CacheService>();

            return services;
        }

        public static IServiceCollection AddAspNetCoreCacheKit(this IServiceCollection services) =>
            services.AddAspNetCoreCacheKit(new ConfigurationBuilder().Build());
    }
}
