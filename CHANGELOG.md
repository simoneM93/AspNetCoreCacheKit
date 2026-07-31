# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

---

## [4.0.0] - 2026-07-31

### Fixed
- Cache stampede: `GetOrCreateAsync` and `GetOrCreate` now serialize concurrent misses on the same key via a per-key lock, so the factory runs once instead of once per concurrent caller
- Internal cache key collision: a bare key containing `:` (e.g. `"app:config"`) could collide with an unrelated group + key pair that concatenated to the same string
- `Set`/`GetOrCreate`/`GetOrCreateAsync`/`Delete` now throw `ArgumentException` on a null or empty `key` instead of failing inside `IMemoryCache` with an unclear error
- README example for `GetOrCreateAsync` used an invalid named argument (`ct:` instead of `cancellationToken:`)

### Added
- `AddAspNetCoreCacheKit(Action<CacheOptions>)` overload — configure options programmatically, no `IConfiguration`/`appsettings.json` required
- XML doc comments on all public API members (`ICacheService`, `CacheOptions`, `CacheServiceExtensions`); package now ships an XML doc file
- Test coverage for `AddAspNetCoreCacheKit` (DI registration, config binding, `ValidateOnStart` failures)

### Changed
- **Breaking:** target framework is now `net10.0` only — `net8.0` and `net9.0` are no longer supported

### Breaking changes
- Target framework `net8.0;net9.0;net10.0` → `net10.0`: projects on .NET 8 or 9 must upgrade to .NET 10, or stay on `AspNetCoreCacheKit` 3.x

---

## [3.0.0] - 2026-03-25
 
### Added
- Unit test suite (`AspNetCoreCacheKit.Tests`) using xUnit 2.x and real `IMemoryCache`
- Repository restructured to support multiple projects: main project moved to `AspNetCoreCacheKit/` subfolder, tests in `AspNetCoreCacheKit.Tests/`
 
### Changed
- `ICacheService.Set` is now generic: `void Set<T>(string groupKey, string key, T value, TimeSpan? duration = null)` — **breaking change**
- `ICacheService.Set` overload without group is now generic: `void Set<T>(string key, T value, TimeSpan? duration = null)` — **breaking change**
 
### Fixed
- `Set` followed by `GetOrCreate<T>` / `GetOrCreateAsync<T>` now works correctly — previously `Set(object)` was type-incompatible with generic read methods causing cache misses
 
### Breaking changes
- `Set(string groupKey, string key, object value)` → `Set<T>(string groupKey, string key, T value)`: update all call sites to use the generic overload
  ```csharp
  // before
  _cache.Set("users", "1", user);
 
  // after — type is usually inferred automatically
  _cache.Set("users", "1", user);
  ```
 
---

## [2.0.0] - 2026-03-24

### Added
- Per-group cache duration via `GroupDurations` in `appsettings.json` — each group key can now have its own expiration independent of the global default
- Per-entry duration override — all `Set`, `GetOrCreate` and `GetOrCreateAsync` methods now accept an optional `TimeSpan? duration` parameter
- Duration resolution priority chain: per-entry → per-group → global default
- `CancellationToken` support on all async methods
- Validation on `GroupDurations` values — all entries must be greater than zero, enforced at startup via `ValidateOnStart`
- `RepositoryType`, `PackageProjectUrl` and `PackageReleaseNotes` metadata in `.csproj`

### Changed
- `CacheOptions.Duration` renamed to `CacheOptions.DurationMinutes` for clarity — **breaking change**
- `GetOrCreateAsync` now uses `IMemoryCache.GetOrCreateAsync` natively instead of wrapping the synchronous `GetOrCreate`
- `ICacheService` registration changed from `Scoped` to `Singleton` in `AddAspNetCoreCacheKit` — aligns with `IOptions<T>` which is Singleton
- Return types of `GetOrCreate<T>` and `GetOrCreateAsync<T>` corrected from `T` to `T?` — reflects the nullable contract of `IMemoryCache`
- `GetFullKey` now returns the bare key when `groupKey` is empty, avoiding keys like `:mykey`
- `PackageTags` expanded with `group-cache`, `cache-expiration`, `dotnet`, `dependency-injection`
- `Description` updated to reflect new features

### Fixed
- Null checks in constructor moved before field assignments and applied to parameters, not fields
- `IOptionsSnapshot<CacheOptions>` replaced with `IOptions<CacheOptions>` — `IOptionsSnapshot` was being recreated on every HTTP request, causing unnecessary allocations for a static configuration
- Redundant `services.Configure<CacheOptions>()` call removed from `AddAspNetCoreCacheKit` — `AddOptions().Bind()` already covers this
- Redundant `.PostConfigure()` removed — default value is now declared directly on `CacheOptions.DurationMinutes`

### Breaking changes
- `CacheOptions.Duration` (TimeSpan) → `CacheOptions.DurationMinutes` (int): update your `appsettings.json` accordingly
  ```json
  // before
  "CacheOptions": { "Duration": "00:60:00" }

  // after
  "CacheOptions": { "DurationMinutes": 60 }
  ```
- `ICacheService` methods now return `T?` instead of `T` — callers may need to handle null returns
- `GetOrCreateAsync` and `GetOrCreate` signatures now include `TimeSpan? duration = null` — source-compatible (optional parameter), but binary-incompatible if you were using the interface via reflection

---

## [1.0.0] - 2025-01-01

### Added
- Initial release
- `ICacheService` interface with `Set`, `GetOrCreate`, `GetOrCreateAsync` and `Delete` methods
- Group-based cache keys (`"group:key"` format)
- `CacheOptions` with `IsEnabled` and `Duration` flags
- `AddAspNetCoreCacheKit()` and `AddAspNetCoreCacheKit(IConfiguration)` DI extension methods
- Configuration validation with DataAnnotations and `ValidateOnStart`
- MIT license

---
[Unreleased]: https://github.com/simoneM93/AspNetCoreCacheKit/compare/v4.0.0...HEAD
[4.0.0]: https://github.com/simoneM93/AspNetCoreCacheKit/compare/v3.0.0...v4.0.0
[3.0.0]: https://github.com/simoneM93/AspNetCoreCacheKit/compare/v2.0.0...v3.0.0
[2.0.0]: https://github.com/simoneM93/AspNetCoreCacheKit/compare/v1.0.0...v2.0.0
[1.0.0]: https://github.com/simoneM93/AspNetCoreCacheKit/releases/tag/v1.0.0
