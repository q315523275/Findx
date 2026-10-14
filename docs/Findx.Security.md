# Findx.Security

认证授权 组件，支持 Jwt、Cookie 认证，授权支持角色、资源标记、角色及资源同时验证，可以实现 `IFunctionStore` 接口来实现端点资源的在线管理。

> Nuget 包引用

```
Findx.Security
```

目标框架：`net6.0` / `net8.0` / `net10.0`。

本包提供两个模块，均通过 `AddFindx().AddModules()` 自动装配：

| 模块 | Level | Order | 开关 |
|---|---|---|---|
| `AuthenticationModule` | Application | 20 | `Findx:Authentication:Enabled` |
| `AuthorizationModule` | Application | 30 | `Findx:Authorization:Enabled` |

> 重要

**不存在 `Findx:Security` 配置节**。本包读的是 `Findx:Authentication:*` 与 `Findx:Authorization:*` 两个平行节点。两个开关都必须**显式写 `true`**，否则对应模块不注册任何服务。

## Json 配置

```json
{
    "Findx": {
        "Authentication": {
            "Enabled": true,
            "DefaultScheme": "Bearer",
            "Jwt": {
                "Enabled": true,
                "Secret": "至少32位以上的随机密钥",
                "Issuer": "findx",
                "Audience": "findx",
                "AccessExpireMinutes": 120,
                "RefreshExpireMinutes": 10080,
                "IsRefreshAbsoluteExpired": true,
                "RenewalMinutes": 20
            },
            "Cookie": {
                "Enabled": false,
                "CookieName": "",
                "LoginPath": "",
                "LogoutPath": "",
                "AccessDeniedPath": "",
                "ReturnUrlParameter": "",
                "ExpireMinutes": 0,
                "SlidingExpiration": true
            }
        },
        "Authorization": {
            "Enabled": true,
            "SuperRoleName": "admin"
        }
    }
}
```

> 配置说明

**`Findx:Authentication`**

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `Enabled` | `false` | 是否启用认证，false 时整块不注册 |
| `DefaultScheme` | `Bearer` | 默认认证方案 |

**`Findx:Authentication:Jwt`**

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `Enabled` | `false` | 是否启用 JWT 方案 |
| `Secret` | 无 | **必填**，为 null 时启动抛异常 |
| `Issuer` | `findx` | 签发者 |
| `Audience` | `findx` | 受众 |
| `AccessExpireMinutes` | `5` | AccessToken 有效期（分钟） |
| `RefreshExpireMinutes` | `10080` | RefreshToken 有效期（分钟），当前代码路径已注释 |
| `IsRefreshAbsoluteExpired` | `true` | 刷新令牌是否绝对过期 |
| `RenewalMinutes` | `0` | 自动续期阈值（分钟），**默认关闭** |

**`Findx:Authentication:Cookie`**

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `Enabled` | `false` | 是否启用 Cookie 方案 |
| `CookieName` | 无 | 不覆盖 ASP.NET Core 默认 |
| `LoginPath` / `LogoutPath` / `AccessDeniedPath` | 无 | 不覆盖框架默认 |
| `ReturnUrlParameter` | 无 | 不覆盖框架默认 |
| `ExpireMinutes` | `0` | `0` 表示不设置，用框架默认 |
| `SlidingExpiration` | `true` | 滑动过期 |

**`Findx:Authorization`**

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `Enabled` | `false` | 是否启用功能权限授权 |
| `SuperRoleName` | 无 | 超级管理员角色名，命中直接放行 |

## 认证

> 签发 Token

```csharp
using Findx.Security;
using Findx.Security.Authentication.Jwt;
using Microsoft.Extensions.Options;

public class AuthController : ControllerBase
{
    private readonly IJwtTokenBuilder _tokenBuilder;
    private readonly IOptions<JwtOptions> _options;

    public AuthController(IJwtTokenBuilder tokenBuilder, IOptions<JwtOptions> options)
    {
        _tokenBuilder = tokenBuilder;
        _options = options;
    }

    [HttpPost("/api/login")]
    public async Task<IActionResult> Login(string userName)
    {
        var payload = new Dictionary<string, string>
        {
            { ClaimTypes.UserId,          "10001" },      // 必填，否则抛 ArgumentException
            { ClaimTypes.UserIdTypeName,  "System.Int64" },
            { ClaimTypes.UserName,        userName },
            { ClaimTypes.Nickname,        "张三" },
            { ClaimTypes.TenantId,        "000000000000000001" },
            { ClaimTypes.OrgId,           "20001" },
            { ClaimTypes.Role,            "admin,user" }  // 逗号分隔
        };

        var token = await _tokenBuilder.CreateAsync(payload, _options.Value);

        return Json(new
        {
            access_token = "Bearer " + token.AccessToken,
            expires = token.AccessTokenUtcExpires
        });
    }
}
```

> 相关类型

```csharp
public interface IJwtTokenBuilder
{
    Task<JwtToken> CreateAsync(IDictionary<string, string> payload, JwtOptions jwtOption);
}

public class JwtToken
{
    public string AccessToken { get; }
    public DateTime AccessTokenUtcExpires { get; }
    public bool IsExpired();
}
```

> 注意事项

- payload 中**必须**包含 `ClaimTypes.UserId`，否则抛 `ArgumentException("不存在用户标识")`。
- 签名算法为 `HmacSha256`，`ClockSkew` 为 1 分钟。
- `ClaimTypes` 全部是可写静态属性（`UserId = "user_id"`、`Role = "role"`、`TenantId = "tenant_id"` 等），可全局改写声明名。

## Token 自动续期

由 `JwtTokenRenewalMiddleware` 实现，挂载在 `UseAuthentication()` 之后。

> 工作流程

1. 已认证请求中判断 Token 剩余有效期 `<= RenewalMinutes` 时触发续期
2. 剔除 `nbf` / `exp` / `iat` / `iss` / `aud` 五个标准声明，其余业务声明原样搬进新 Token
3. 新 Token 通过**响应头** `Authorization: Bearer {新Token}` 回写

> 前端配合

```javascript
// 响应拦截器 —— 接收服务端续期后的新 Token
axios.interceptors.response.use(res => {
  const renew = res.headers['authorization'];
  if (renew && renew.startsWith('Bearer '))
    localStorage.setItem('access_token', renew.replace('Bearer ', ''));
  return res;
});
```

> 注意事项

`RenewalMinutes` 默认为 `0`，**必须显式配置才生效**。

## 强制下线与单点登录

`IAdvancedTokenValidator` 的 `LifetimeValidator` 扩展点，默认实现恒返回 `true`。

```csharp
public class MyTokenValidator : IAdvancedTokenValidator
{
    private readonly ICacheFactory _cacheFactory;
    public MyTokenValidator(ICacheFactory cacheFactory) => _cacheFactory = cacheFactory;

    public bool ValidateLifetime(DateTime? notBefore, DateTime? expires,
        SecurityToken securityToken, TokenValidationParameters validationParameters)
    {
        var cache = _cacheFactory.Create(CacheType.DefaultMemory);
        var userId = (securityToken as JwtSecurityToken)?
            .Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserId)?.Value;

        if (userId != null && cache.Get<bool>($"token:revoked:{userId}")) return false;
        return true;
    }
}
```

在自定义模块中替换注册：

```csharp
services.AddSingleton<IAdvancedTokenValidator, MyTokenValidator>();
```

## 授权

框架使用 ASP.NET Core 原生 `AuthorizeAttribute` 加框架自定义的 `PreAuthorizeAttribute`，**没有** `[AllowPermission]` / `[AllowRole]` 这类特性。

```csharp
public class PreAuthorizeAttribute : AuthorizeAttribute
{
    public string Authority { set; get; }   // 权限资源
}
```

策略名常量：`FunctionRequirement.Policy = "FindxPolicy"`。

> 四种访问级别

```csharp
// 匿名
[HttpGet("health")]
public IActionResult Health() => Ok();

// 仅要求登录
[Authorize]
[HttpGet("profile")]
public IActionResult Profile() => Ok();

// 角色限制 —— AccessType = RoleLimit
[Authorize(Policy = FunctionRequirement.Policy, Roles = "admin")]
[HttpGet("list")]
public IActionResult List() => Ok();

// 权限资源限制 —— AccessType = AuthorityLimit
[PreAuthorize(Policy = FunctionRequirement.Policy, Authority = "Sys:User")]
[HttpPost("create")]
public IActionResult Create() => Ok();

// 角色 + 权限资源 —— AccessType = RoleAuthorityLimit
[PreAuthorize(Policy = FunctionRequirement.Policy, Roles = "admin", Authority = "Sys:User:Delete")]
[HttpPost("delete")]
public IActionResult Delete() => Ok();
```

> `FunctionAccessType` 映射规则

| 代码标注 | `FunctionAccessType` |
|---|---|
| 无标注 / `[AllowAnonymous]` | `Anonymous` |
| `[Authorize]` 无角色无资源 | `Login` |
| `[Authorize(Roles = "...")]` | `RoleLimit` |
| `[PreAuthorize(Authority = "...")]` | `AuthorityLimit` |
| `[PreAuthorize(Roles = "...", Authority = "...")]` | `RoleAuthorityLimit` |

Action 未标注时**继承 Controller 上的标注**。

> 判定顺序

```
function == null  → NoFound (404)
IsLocked         → Locked (423)
Anonymous         → Ok
未登录            → Unauthorized (401)
Login            → Ok
RoleLimit        → 角色校验
AuthorityLimit   → 资源校验
RoleAuthorityLimit → 先角色后资源
```

> 注意事项

**`AuthorizeAuthorityLimit` 当前是空实现，直接返回 `Ok`**（源码注释为「Todo 自行实现资源的验证，如 Vue 按钮权限」）。要真正启用「权限资源」维度校验，必须继承 `FunctionAuthorizationBase` 重写该方法：

```csharp
public class MyFunctionAuthorization : FunctionAuthorizationBase
{
    public MyFunctionAuthorization(IConfiguration settingProvider) : base(settingProvider) { }

    protected override AuthorizationStatus AuthorizeAuthorityLimit(IFunction function, IPrincipal principal)
    {
        // 从用户权限集合中查找 function.Authority
        return AuthorizationStatus.Ok;
    }
}
```

`SuperRoleName` 命中的角色在角色校验时直接返回 `Ok`（超级管理员后门）。

## 端点资源在线管理

`IFunctionStore<TFunction>` 抽象负责功能信息的持久化，默认实现 `MvcFunctionStore` **仅存内存**，进程重启丢失。

```csharp
public interface IFunctionStore<TFunction>
{
    Task SyncToDatabaseAsync(IEnumerable<TFunction> functions, CancellationToken cancellationToken = default);
    Task<List<TFunction>> QueryFromDatabaseAsync(CancellationToken cancellationToken = default);
}
```

> 查询功能清单

```csharp
[HttpGet("functions")]
[DisableAuditing]
public async Task<CommonResult> Functions([FromServices] IFunctionStore<MvcFunction> store)
    => CommonResult.Success(await store.QueryFromDatabaseAsync());
```

> 实现数据库存储

```csharp
public class MyFunctionStore : IFunctionStore<MvcFunction>
{
    private readonly IRepository<MvcFunction, long> _repo;
    public MyFunctionStore(IRepository<MvcFunction, long> repo) => _repo = repo;

    public async Task SyncToDatabaseAsync(IEnumerable<MvcFunction> functions, CancellationToken ct = default)
        => await _repo.InsertOrUpdateAsync(functions, ct);

    public async Task<List<MvcFunction>> QueryFromDatabaseAsync(CancellationToken ct = default)
        => await _repo.SelectAsync(a => true, ct);
}
```

```csharp
services.AddSingleton<IFunctionStore<MvcFunction>, MyFunctionStore>();
```

替换后 `Roles` / `Authority` / `IsLocked` 变成数据库可编辑数据，调用 `IFunctionHandler.RefreshCacheAsync()` 即可热更新。

## 启动示例

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFindx().AddModules();   // 自动装载 AuthenticationModule + AuthorizationModule
builder.Services.AddControllers();

var app = builder.Build();

app.UseFindx();                             // 内部执行 UseAuthentication → JwtTokenRenewalMiddleware → UseAuthorization
app.UseEndpointsWithAreaRoute();
app.UseFindxHosting();
```