# AspNetCoreCacheKit

[![NuGet](https://img.shields.io/nuget/v/AspNetCoreCacheKit.svg)](https://www.nuget.org/packages/AspNetCoreCacheKit) [![.NET](https://github.com/simoneM93/AspNetCoreCacheKit/actions/workflows/dotnet.yml/badge.svg)](https://github.com/simoneM93/AspNetCoreCacheKit/actions)

A modern caching library for ASP.NET Core featuring **group-based keys**, configuration validation, and simplified overloads.

## ✨ Features

- 🔑 **Group-based keys** (`"users:123"`)
- ⚡ **Optimized performance** with `GetOrCreate`
- ✅ **Configuration validation** with DataAnnotations
- 📐 **Nullable reference types** support
- 🎛️ **appsettings.json** configuration
- 🧪 **DI-ready** Scoped service

## 📋 Requirements

| Requirement | Minimum Version |
|-------------|-----------------|
| .NET | 8.0 or 9.0 |
| ASP.NET Core | 8.0+ |

## 🚀 Installation

```bash
dotnet add package AspNetCoreCacheKit
dotnet add package Microsoft.Extensions.Options.ConfigurationExtensions
dotnet add package Microsoft.Extensions.Options.DataAnnotations
```

## 🎯 Quick Start

### 1. Configure `appsettings.json`
```json
{
  "CacheOptions": {
    "IsEnabled": true,
    "Duration": "01:00:00"
  }
}
```

### 2. Register Services
```csharp
builder.Services.AddAspNetCoreCacheKit(builder.Configuration);
```

### 3. Use in Controller/Service
```csharp
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ICacheService _cache;

    public UsersController(ICacheService cache)
    {
        _cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        // With group
        var users = await _cache.GetOrCreateAsync("users", "all", GetUsersFromDb);
        
        // Without group (overload)
        var config = _cache.GetOrCreate("app:config", LoadConfig);
        
        return Ok(users);
    }
}
```