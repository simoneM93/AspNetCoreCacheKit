using AspNetCoreCacheKit.Models;
using AspNetCoreCacheKit.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AspNetCoreCacheKit.Tests
{
    public class CacheService_Constructor
    {
        [Fact]
        public void Constructor_NullMemoryCache_ThrowsArgumentNullException()
        {
            var options = Options.Create(new CacheOptions());
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new CacheService(null!, options));
            Assert.Equal("memoryCache", ex.ParamName);
        }

        [Fact]
        public void Constructor_NullOptions_ThrowsArgumentNullException()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new CacheService(cache, null!));
            Assert.Equal("cacheOptions", ex.ParamName);
        }

        [Fact]
        public void Constructor_ValidArguments_DoesNotThrow()
        {
            var sut = CacheServiceFactory.Create();
            Assert.NotNull(sut);
        }
    }

    public class CacheService_SetAndGetOrCreate
    {
        [Fact]
        public void Set_ThenGetOrCreate_ReturnsCachedValue()
        {
            var sut = CacheServiceFactory.Create();
            sut.Set("users", "1", "Simone");

            var result = sut.GetOrCreate<string>("users", "1", () => "Other");

            Assert.Equal("Simone", result);
        }

        [Fact]
        public void GetOrCreate_CacheMiss_InvokesFactory()
        {
            var sut = CacheServiceFactory.Create();
            var factoryInvoked = false;

            sut.GetOrCreate<string>("users", "99", () =>
            {
                factoryInvoked = true;
                return "FromFactory";
            });

            Assert.True(factoryInvoked);
        }

        [Fact]
        public void GetOrCreate_CacheHit_DoesNotInvokeFactory()
        {
            var sut = CacheServiceFactory.Create();
            sut.Set("users", "1", "Cached");
            var factoryInvoked = false;

            sut.GetOrCreate<string>("users", "1", () =>
            {
                factoryInvoked = true;
                return "FromFactory";
            });

            Assert.False(factoryInvoked);
        }

        [Fact]
        public void GetOrCreate_WithoutGroup_WorksWithBareKey()
        {
            var sut = CacheServiceFactory.Create();

            var result = sut.GetOrCreate("app:config", () => "config-value");

            Assert.Equal("config-value", result);
        }

        [Fact]
        public void Set_WithoutGroup_WorksWithBareKey()
        {
            var sut = CacheServiceFactory.Create();
            sut.Set("app:config", "config-value");

            var result = sut.GetOrCreate<string>("app:config", () => "other");

            Assert.Equal("config-value", result);
        }
    }

    public class CacheService_GetOrCreateAsync
    {
        [Fact]
        public async Task GetOrCreateAsync_CacheMiss_InvokesFactory()
        {
            var sut = CacheServiceFactory.Create();
            var factoryInvoked = false;

            await sut.GetOrCreateAsync<string>("users", "1", _ =>
            {
                factoryInvoked = true;
                return Task.FromResult("value");
            });

            Assert.True(factoryInvoked);
        }

        [Fact]
        public async Task GetOrCreateAsync_CacheHit_DoesNotInvokeFactory()
        {
            var sut = CacheServiceFactory.Create();
            sut.Set("users", "1", "Cached");
            var factoryInvoked = false;

            await sut.GetOrCreateAsync<string>("users", "1", _ =>
            {
                factoryInvoked = true;
                return Task.FromResult("FromFactory");
            });

            Assert.False(factoryInvoked);
        }

        [Fact]
        public async Task GetOrCreateAsync_ReturnsCachedValue()
        {
            var sut = CacheServiceFactory.Create();

            var first = await sut.GetOrCreateAsync("users", "1", _ => Task.FromResult("Simone"));
            var second = await sut.GetOrCreateAsync("users", "1", _ => Task.FromResult("Other"));

            Assert.Equal("Simone", first);
            Assert.Equal("Simone", second);
        }

        [Fact]
        public async Task GetOrCreateAsync_WithoutGroup_WorksWithBareKey()
        {
            var sut = CacheServiceFactory.Create();

            var result = await sut.GetOrCreateAsync("app:config", _ => Task.FromResult("config-value"));

            Assert.Equal("config-value", result);
        }

        [Fact]
        public async Task GetOrCreateAsync_CancelledToken_ThrowsOperationCanceledException()
        {
            var sut = CacheServiceFactory.Create();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                sut.GetOrCreateAsync<string>("users", "1", _ => Task.FromResult("v"), cancellationToken: cts.Token));
        }
    }

    public class CacheService_Delete
    {
        [Fact]
        public void Delete_ExistingEntry_RemovesFromCache()
        {
            var sut = CacheServiceFactory.Create();
            sut.Set("users", "1", "Simone");

            sut.Delete("users", "1");

            var factoryInvoked = false;
            sut.GetOrCreate<string>("users", "1", () =>
            {
                factoryInvoked = true;
                return "FromFactory";
            });

            Assert.True(factoryInvoked);
        }

        [Fact]
        public void Delete_NonExistingEntry_DoesNotThrow()
        {
            var sut = CacheServiceFactory.Create();
            sut.Delete("users", "999");
        }

        [Fact]
        public void Delete_WithoutGroup_RemovesBareKey()
        {
            var sut = CacheServiceFactory.Create();
            sut.Set("app:config", "value");

            sut.Delete("app:config");

            var factoryInvoked = false;
            sut.GetOrCreate<string>("app:config", () =>
            {
                factoryInvoked = true;
                return "new";
            });

            Assert.True(factoryInvoked);
        }
    }

    public class CacheService_IsDisabled
    {
        [Fact]
        public void Set_WhenDisabled_DoesNotStoreValue()
        {
            var sut = CacheServiceFactory.Create(isEnabled: false);
            sut.Set("users", "1", "Simone");

            var factoryInvoked = false;
            sut.GetOrCreate<string>("users", "1", () =>
            {
                factoryInvoked = true;
                return "FromFactory";
            });

            Assert.True(factoryInvoked);
        }

        [Fact]
        public void GetOrCreate_WhenDisabled_AlwaysInvokesFactory()
        {
            var sut = CacheServiceFactory.Create(isEnabled: false);
            var callCount = 0;

            sut.GetOrCreate<string>("users", "1", () => { callCount++; return "v"; });
            sut.GetOrCreate<string>("users", "1", () => { callCount++; return "v"; });

            Assert.Equal(2, callCount);
        }

        [Fact]
        public async Task GetOrCreateAsync_WhenDisabled_AlwaysInvokesFactory()
        {
            var sut = CacheServiceFactory.Create(isEnabled: false);
            var callCount = 0;

            await sut.GetOrCreateAsync<string>("users", "1", _ => { callCount++; return Task.FromResult("v"); });
            await sut.GetOrCreateAsync<string>("users", "1", _ => { callCount++; return Task.FromResult("v"); });

            Assert.Equal(2, callCount);
        }

        [Fact]
        public void Delete_WhenDisabled_DoesNotThrow()
        {
            var sut = CacheServiceFactory.Create(isEnabled: false);
            sut.Delete("users", "1");
        }
    }

    public class CacheService_DurationResolution
    {
        [Fact]
        public void GetOrCreate_NoOverride_UsesGlobalDuration()
        {
            var sut = CacheServiceFactory.Create(durationMinutes: 60);

            var result = sut.GetOrCreate("users", "1", () => "value");

            Assert.Equal("value", result);
        }

        [Fact]
        public void GetOrCreate_WithGroupDuration_UsesGroupDuration()
        {
            var sut = CacheServiceFactory.Create(
                durationMinutes: 60,
                groupDurations: new Dictionary<string, int> { ["users"] = 5 });

            var result = sut.GetOrCreate("users", "1", () => "value");

            Assert.Equal("value", result);
        }

        [Fact]
        public void GetOrCreate_WithExplicitDuration_UsesExplicitDuration()
        {
            var sut = CacheServiceFactory.Create(
                durationMinutes: 60,
                groupDurations: new Dictionary<string, int> { ["users"] = 5 });

            var result = sut.GetOrCreate("users", "1", () => "value", TimeSpan.FromMinutes(1));

            Assert.Equal("value", result);
        }

        [Fact]
        public void Set_WithExplicitDuration_StoresValue()
        {
            var sut = CacheServiceFactory.Create(durationMinutes: 60);

            sut.Set("users", "1", "Simone", TimeSpan.FromMinutes(1));
            var result = sut.GetOrCreate("users", "1", () => "Other");

            Assert.Equal("Simone", result);
        }

        [Fact]
        public void GroupWithoutDurationConfig_FallsBackToGlobal()
        {
            var sut = CacheServiceFactory.Create(
                durationMinutes: 60,
                groupDurations: new Dictionary<string, int> { ["users"] = 5 });

            var result = sut.GetOrCreate("orders", "1", () => "order-value");

            Assert.Equal("order-value", result);
        }
    }

    public class CacheService_KeyComposition
    {
        [Fact]
        public void SameKey_DifferentGroups_AreIndependent()
        {
            var sut = CacheServiceFactory.Create();
            sut.Set("users", "1", "user-value");
            sut.Set("orders", "1", "order-value");

            var user = sut.GetOrCreate<string>("users", "1", () => "x");
            var order = sut.GetOrCreate<string>("orders", "1", () => "x");

            Assert.Equal("user-value", user);
            Assert.Equal("order-value", order);
        }

        [Fact]
        public void EmptyGroupKey_UsesBareKey()
        {
            var sut = CacheServiceFactory.Create();
            sut.Set(string.Empty, "mykey", "value");

            var result = sut.GetOrCreate<string>("mykey", () => "other");

            Assert.Equal("value", result);
        }

        [Fact]
        public void Delete_WithGroup_DoesNotAffectOtherGroups()
        {
            var sut = CacheServiceFactory.Create();
            sut.Set("users", "1", "user-value");
            sut.Set("orders", "1", "order-value");

            sut.Delete("users", "1");

            var order = sut.GetOrCreate<string>("orders", "1", () => "x");
            Assert.Equal("order-value", order);
        }
    }

    public class CacheService_MultipleCallsSameInstance
    {
        [Fact]
        public void MultipleGroups_AllStoredCorrectly()
        {
            var sut = CacheServiceFactory.Create();

            sut.Set("users", "1", "Simone");
            sut.Set("orders", "1", "Order-1");
            sut.Set("countries", "IT", "Italy");

            Assert.Equal("Simone", sut.GetOrCreate<string>("users", "1", () => "x"));
            Assert.Equal("Order-1", sut.GetOrCreate<string>("orders", "1", () => "x"));
            Assert.Equal("Italy", sut.GetOrCreate<string>("countries", "IT", () => "x"));
        }

        [Fact]
        public async Task AsyncAndSyncOverloads_ShareSameCache()
        {
            var sut = CacheServiceFactory.Create();

            sut.Set("users", "1", "Simone");

            var result = await sut.GetOrCreateAsync<string>("users", "1", _ => Task.FromResult("Other"));

            Assert.Equal("Simone", result);
        }
    }
}