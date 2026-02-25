# AspNetCoreCacheKit

[![NuGet](https://img.shields.io/nuget/v/AspNetCoreCacheKit.svg)](https://www.nuget.org/packages/AspNetCoreCacheKit)

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
| .NET | 9.0 |
| ASP.NET Core | 9.0+ |

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
    "Duration": 60 //Expressed in minutes
  }
}
```

### 2. Register Services
```csharp
// With config
builder.Services.AddAspNetCoreCacheKit(builder.Configuration);

// Only defaults (Without appsettings)
builder.Services.AddAspNetCoreCacheKit();
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