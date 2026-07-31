using AspNetCoreCacheKit.Interfaces;
using AspNetCoreCacheKit.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspNetCoreCacheKit.Extensions
{
    /// <summary>DI registration for <see cref="AspNetCoreCacheKit.Interfaces.ICacheService"/>.</summary>
    public static class CacheServiceExtensions
    {
        /// <summary>
        /// Registers <see cref="Interfaces.ICacheService"/> as a singleton, binding <see cref="CacheOptions"/>
        /// from the "CacheOptions" section of <paramref name="configuration"/> and validating it on startup.
        /// </summary>
        public static IServiceCollection AddAspNetCoreCacheKit(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            return services.AddAspNetCoreCacheKitCore(
                builder => builder.Bind(configuration.GetSection("CacheOptions")));
        }

        /// <summary>
        /// Registers <see cref="Interfaces.ICacheService"/> as a singleton, configuring <see cref="CacheOptions"/>
        /// programmatically via <paramref name="configure"/> instead of from an <see cref="IConfiguration"/> source.
        /// </summary>
        public static IServiceCollection AddAspNetCoreCacheKit(
            this IServiceCollection services,
            Action<CacheOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            return services.AddAspNetCoreCacheKitCore(builder => builder.Configure(configure));
        }

        /// <summary>
        /// Registers <see cref="Interfaces.ICacheService"/> as a singleton with default <see cref="CacheOptions"/>
        /// (no configuration source).
        /// </summary>
        public static IServiceCollection AddAspNetCoreCacheKit(this IServiceCollection services) =>
            services.AddAspNetCoreCacheKit(new ConfigurationBuilder().Build());

        private static IServiceCollection AddAspNetCoreCacheKitCore(
            this IServiceCollection services,
            Action<OptionsBuilder<CacheOptions>> configureOptions)
        {
            services.AddMemoryCache();

            var optionsBuilder = services.AddOptions<CacheOptions>();
            configureOptions(optionsBuilder);
            optionsBuilder
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
    }
}
