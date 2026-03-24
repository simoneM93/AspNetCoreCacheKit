# AspNetCoreCacheKit

[![NuGet](https://img.shields.io/nuget/v/AspNetCoreCacheKit.svg)](https://www.nuget.org/packages/AspNetCoreCacheKit)
[![Publish to NuGet](https://github.com/simoneM93/AspNetCoreCacheKit/actions/workflows/publish.yml/badge.svg)](https://github.com/simoneM93/AspNetCoreCacheKit/actions/workflows/publish.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![GitHub Sponsors](https://img.shields.io/badge/Sponsor-%E2%9D%A4-ea4aaa?logo=github-sponsors)](https://github.com/sponsors/simoneM93)
[![Changelog](https://img.shields.io/badge/Changelog-view-blue)](CHANGELOG.md)

A lightweight caching library for ASP.NET Core featuring **group-based keys**, configurable expiration, and a DI-ready design that wraps `IMemoryCache` in a clean, testable abstraction.

> **Why AspNetCoreCacheKit?**
> `IMemoryCache` is powerful but low-level. AspNetCoreCacheKit adds group-based key management,
> `appsettings.json` configuration, and a consistent API that makes caching easy to use and easy to mock in tests.

---

## ✨ Features

- 🔑 **Group-based keys** — organise cache entries with prefixes like `"users:123"`
- ⚡ **`GetOrCreate` and `GetOrCreateAsync`** — read-through pattern out of the box
- 🗑️ **`Delete`** — remove a single entry by key or group + key
- ✅ **Configuration validation** with DataAnnotations
- 📐 **Nullable reference types** support
- 🎛️ **`appsettings.json`** configuration with sensible defaults

---

## 📋 Requirements

| Requirement | Minimum version |
|---|---|
| .NET | 9.0+ |
| ASP.NET Core | 9.0+ |

---

## 🚀 Installation

```bash
dotnet add package AspNetCoreCacheKit
```

---

## 🎯 Quick Start

### 1. Configure `appsettings.json`

```json
{
  "CacheOptions": {
    "IsEnabled": true,
    "DurationMinutes": 60,
    "GroupDurations": {
      "users": 30,
      "tokens": 5,
      "countries": 1440
    }
  }
}
```

> All duration values are expressed in **minutes**.
> `GroupDurations` is optional — groups without an entry use the global `DurationMinutes`.

### 2. Register services

```csharp
// With appsettings.json
builder.Services.AddAspNetCoreCacheKit(builder.Configuration);

// Without appsettings (uses defaults)
builder.Services.AddAspNetCoreCacheKit();
```

### 3. Inject and use

```csharp
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ICacheService _cache;

    public UsersController(ICacheService cache)
    {
        _cache = cache;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id, CancellationToken ct)
    {
        // With group key — full key will be "users:42"
        var user = await _cache.GetOrCreateAsync(
            "users",
            id.ToString(),
            _ => GetUserFromDb(id),
            ct);

        return user is null ? NotFound() : Ok(user);
    }
}
```

---

## 📚 API Reference

### Duration priority

Every method that writes to the cache resolves the expiration following this priority chain — first wins:

1. `duration` parameter passed explicitly to the method
2. `GroupDurations[groupKey]` configured in `appsettings.json`
3. Global `DurationMinutes` from `appsettings.json`

```csharp
// 1. Explicit duration wins — cached for 2 minutes regardless of everything else
await _cache.GetOrCreateAsync("tokens", userId, _ => GenerateTokenAsync(), TimeSpan.FromMinutes(2));

// 2. No explicit duration — uses GroupDurations["users"] = 30 min from appsettings
await _cache.GetOrCreateAsync("users", "42", _ => GetUserFromDb(42));

// 3. No explicit duration, no group config — uses global DurationMinutes = 60 min
await _cache.GetOrCreateAsync("misc", "key", _ => LoadSomethingAsync());
```

### `GetOrCreateAsync` — async read-through

```csharp
// With group — uses GroupDurations["users"] or global fallback
var user = await _cache.GetOrCreateAsync("users", "42", _ => GetUserFromDb(42), ct: ct);

// With group + explicit duration override
var token = await _cache.GetOrCreateAsync(
    "tokens", userId,
    _ => GenerateTokenAsync(userId),
    duration: TimeSpan.FromMinutes(5),
    ct);

// Without group key
var config = await _cache.GetOrCreateAsync("app:config", _ => LoadConfigAsync(), ct: ct);
```

### `GetOrCreate` — sync read-through

```csharp
// Uses GroupDurations["countries"] = 1440 min from appsettings
var countries = _cache.GetOrCreate("countries", () => LoadCountries());

// Explicit override — ignores group and global config
var result = _cache.GetOrCreate("countries", "IT", () => LoadItaly(), TimeSpan.FromHours(2));
```

### `Set` — explicit write

```csharp
_cache.Set("users", "42", user);                                  // group duration or global
_cache.Set("users", "42", user, TimeSpan.FromMinutes(10));        // explicit override
```

### `Delete` — remove an entry

```csharp
_cache.Delete("users", "42");
_cache.Delete("app:config");
```

---

## ⚙️ Configuration options

| Option | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | Enables or disables caching entirely. When `false`, factories are always invoked. |
| `DurationMinutes` | `int` | `60` | Global default cache duration in **minutes**. Used when no per-entry or per-group duration is set. |
| `GroupDurations` | `Dictionary<string, int>` | `{}` | Per-group durations in **minutes**. Key = group name, Value = duration in minutes. |

Setting `IsEnabled: false` is useful in development or testing environments where you want to bypass the cache without changing code.

---

## ❤️ Support

If you find AspNetCoreCacheKit useful, consider sponsoring its development.

[![Sponsor simoneM93](https://img.shields.io/badge/Sponsor-%E2%9D%A4-ea4aaa?logo=github-sponsors&style=for-the-badge)](https://github.com/sponsors/simoneM93)

---

## 📄 License

MIT — see [LICENSE](LICENSE) for details.
