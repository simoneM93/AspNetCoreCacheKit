# AspNetCoreCacheKit

[![NuGet](https://img.shields.io/nuget/v/AspNetCoreCacheKit.svg)](https://www.nuget.org/packages/AspNetCoreCacheKit)
[![Publish to NuGet](https://github.com/simoneM93/AspNetCoreCacheKit/actions/workflows/publish.yml/badge.svg)](https://github.com/simoneM93/AspNetCoreCacheKit/actions/workflows/publish.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![GitHub Sponsors](https://img.shields.io/badge/Sponsor-%E2%9D%A4-ea4aaa?logo=github-sponsors)](https://github.com/sponsors/simoneM93)
[![Changelog](https://img.shields.io/badge/Changelog-view-blue)](CHANGELOG.md)

A lightweight caching library for ASP.NET Core featuring **group-based keys**, per-entry and per-group duration, configurable via `appsettings.json`, and a DI-ready design that wraps `IMemoryCache` in a clean, testable abstraction.

> **Why AspNetCoreCacheKit?**
> `IMemoryCache` is powerful but low-level. AspNetCoreCacheKit adds group-based key management,
> per-group and per-entry expiration, `appsettings.json` configuration, and a consistent
> type-safe API that makes caching easy to use and easy to mock in tests.

---

## ✨ Features

- 🔑 **Group-based keys** — organise cache entries with prefixes like `"users:123"`
- ⚡ **`GetOrCreate` and `GetOrCreateAsync`** — read-through pattern out of the box
- ⏱️ **Per-group and per-entry duration** — fine-grained expiration control
- 🗑️ **`Delete`** — remove a single entry by key or group + key
- ✅ **Configuration validation** with DataAnnotations
- 📐 **Nullable reference types** and **generic type-safe `Set<T>`** support
- 🎛️ **`appsettings.json`** configuration with sensible defaults
- 🧪 **DI-ready** — register with one line, mock `ICacheService` in tests

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
        // Uses GroupDurations["users"] = 30 min from appsettings
        var user = await _cache.GetOrCreateAsync(
            "users",
            id.ToString(),
            _ => GetUserFromDb(id),
            ct: ct);

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

// Explicit override
var result = _cache.GetOrCreate("countries", "IT", () => LoadItaly(), TimeSpan.FromHours(2));
```

### `Set` — explicit write

```csharp
_cache.Set("users", "42", user);                                   // group duration or global
_cache.Set("users", "42", user, TimeSpan.FromMinutes(10));         // explicit override
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
| `DurationMinutes` | `int` | `60` | Global default cache duration in **minutes**. |
| `GroupDurations` | `Dictionary<string, int>` | `{}` | Per-group durations in **minutes**. Key = group name, Value = duration in minutes. |

Setting `IsEnabled: false` is useful in development or testing environments where you want to bypass the cache without changing code.

---

## 🧪 Testing

`ICacheService` is a plain interface — mock it directly in unit tests:

```csharp
var cacheMock = new Mock<ICacheService>();

cacheMock
    .Setup(c => c.GetOrCreateAsync(
        "users", "1",
        It.IsAny<Func<ICacheEntry, Task<User>>>(),
        It.IsAny<TimeSpan?>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(new User { Id = 1, Name = "Simone" });
```

---

## 🧩 Part of the ASP.NET Core Kit ecosystem
 
AspNetCoreCacheKit is part of a cohesive set of lightweight, DI-ready toolkits for ASP.NET Core:
 
| Package | Description |
|---|---|
| [AspNetCoreAuthKit](https://www.nuget.org/packages/AspNetCoreAuthKit) | JWT, API Key, Refresh Tokens with rotation & dynamic claim policies |
| [AspNetCoreResponseKit](https://www.nuget.org/packages/AspNetCoreResponseKit) | Standardized `ApiResponse<T>`, global exception handling & smart factories |
| [AspNetCoreHttpKit](https://www.nuget.org/packages/AspNetCoreHttpKit) | Typed HTTP client with result pattern & StructLog integration |
| [AspNetCoreCacheKit](https://www.nuget.org/packages/AspNetCoreCacheKit) | Group-based caching with per-entry & per-group duration |
| [StructLog](https://www.nuget.org/packages/StructLog) | Structured logging with EventCode support & custom enrichers |
 
Each package works independently — use one or all five.

---

## ❤️ Support

If you find AspNetCoreCacheKit useful, consider sponsoring its development.

[![Sponsor simoneM93](https://img.shields.io/badge/Sponsor-%E2%9D%A4-ea4aaa?logo=github-sponsors&style=for-the-badge)](https://github.com/sponsors/simoneM93)

---

## 📄 License

MIT — see [LICENSE](LICENSE) for details.
