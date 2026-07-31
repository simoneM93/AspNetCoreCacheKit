using AspNetCoreCacheKit.Extensions;
using AspNetCoreCacheKit.Interfaces;
using AspNetCoreCacheKit.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AspNetCoreCacheKit.Tests
{
    public class CacheServiceExtensions_Registration
    {
        [Fact]
        public void AddAspNetCoreCacheKit_NullServices_ThrowsArgumentNullException()
        {
            IServiceCollection services = null!;
            var configuration = new ConfigurationBuilder().Build();

            Assert.Throws<ArgumentNullException>(() => services.AddAspNetCoreCacheKit(configuration));
        }

        [Fact]
        public void AddAspNetCoreCacheKit_NullConfiguration_ThrowsArgumentNullException()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentNullException>(() => services.AddAspNetCoreCacheKit((IConfiguration)null!));
        }

        [Fact]
        public void AddAspNetCoreCacheKit_RegistersICacheServiceAsSingleton()
        {
            var services = new ServiceCollection();
            services.AddAspNetCoreCacheKit();

            var descriptor = services.Single(d => d.ServiceType == typeof(ICacheService));

            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Fact]
        public void AddAspNetCoreCacheKit_NoConfiguration_UsesDefaultOptions()
        {
            var services = new ServiceCollection();
            services.AddAspNetCoreCacheKit();

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<CacheOptions>>().Value;

            Assert.True(options.IsEnabled);
            Assert.Equal(60, options.DurationMinutes);
            Assert.Empty(options.GroupDurations);
        }

        [Fact]
        public void AddAspNetCoreCacheKit_BindsCacheOptionsFromConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["CacheOptions:IsEnabled"] = "false",
                    ["CacheOptions:DurationMinutes"] = "15",
                    ["CacheOptions:GroupDurations:users"] = "5",
                })
                .Build();

            var services = new ServiceCollection();
            services.AddAspNetCoreCacheKit(configuration);

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<CacheOptions>>().Value;

            Assert.False(options.IsEnabled);
            Assert.Equal(15, options.DurationMinutes);
            Assert.Equal(5, options.GroupDurations["users"]);
        }

        [Fact]
        public void AddAspNetCoreCacheKit_InvalidDurationMinutes_ThrowsOnResolve()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["CacheOptions:DurationMinutes"] = "0",
                })
                .Build();

            var services = new ServiceCollection();
            services.AddAspNetCoreCacheKit(configuration);

            var provider = services.BuildServiceProvider();

            Assert.Throws<OptionsValidationException>(
                () => { _ = provider.GetRequiredService<IOptions<CacheOptions>>().Value; });
        }

        [Fact]
        public void AddAspNetCoreCacheKit_ActionOverload_NullServices_ThrowsArgumentNullException()
        {
            IServiceCollection services = null!;

            Assert.Throws<ArgumentNullException>(() => services.AddAspNetCoreCacheKit(_ => { }));
        }

        [Fact]
        public void AddAspNetCoreCacheKit_ActionOverload_NullConfigure_ThrowsArgumentNullException()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentNullException>(() => services.AddAspNetCoreCacheKit((Action<CacheOptions>)null!));
        }

        [Fact]
        public void AddAspNetCoreCacheKit_ActionOverload_ConfiguresCacheOptions()
        {
            var services = new ServiceCollection();
            services.AddAspNetCoreCacheKit(options =>
            {
                options.IsEnabled = false;
                options.DurationMinutes = 15;
                options.GroupDurations["users"] = 5;
            });

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<CacheOptions>>().Value;

            Assert.False(options.IsEnabled);
            Assert.Equal(15, options.DurationMinutes);
            Assert.Equal(5, options.GroupDurations["users"]);
        }

        [Fact]
        public void AddAspNetCoreCacheKit_ActionOverload_InvalidDurationMinutes_ThrowsOnResolve()
        {
            var services = new ServiceCollection();
            services.AddAspNetCoreCacheKit(options => options.DurationMinutes = 0);

            var provider = services.BuildServiceProvider();

            Assert.Throws<OptionsValidationException>(
                () => { _ = provider.GetRequiredService<IOptions<CacheOptions>>().Value; });
        }

        [Fact]
        public void AddAspNetCoreCacheKit_InvalidGroupDurations_ThrowsOnResolve()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["CacheOptions:GroupDurations:users"] = "0",
                })
                .Build();

            var services = new ServiceCollection();
            services.AddAspNetCoreCacheKit(configuration);

            var provider = services.BuildServiceProvider();

            Assert.Throws<OptionsValidationException>(
                () => { _ = provider.GetRequiredService<IOptions<CacheOptions>>().Value; });
        }
    }
}
