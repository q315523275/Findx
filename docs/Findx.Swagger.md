# Findx.Swagger

Swagger 组件，使用 Swagger 生成应用的在线接口文档信息，并针对中文场景做了展示优化。

> Nuget 包引用

```
Findx.Swagger
```

目标框架：`net6.0` / `net8.0` / `net10.0`，底层依赖 `Swashbuckle.AspNetCore`（net6.0 为 7.3.2，net8.0/net10.0 为 10.2.3）。

> 本包无独立的 DI 扩展方法

通过 `StartupModule` 机制自动装配：

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 SwaggerModule
```

`SwaggerModule`（`ModuleLevel.Application`，`Order = 40`）在 `ConfigureServices` 中调用 `AddSwaggerGen`，在 `UseModule` 中调用 `UseSwagger()` 与 `UseSwaggerUI(...)`。

## Json 配置

```json
{
    "Findx": {
        "Swagger": {
            "Enabled": true,
            "AllParametersInCamelCase": true,
            "HideSchemas": true,
            "DocExpansion": "None",
            "Endpoints": [
                {
                    "Title": "EleAdmin 后台接口",
                    "Version": "eleAdmin",
                    "Url": "/swagger/eleAdmin/swagger.json"
                }
            ]
        }
    }
}
```

> 配置说明

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `Enabled` | `true` | 是否启用，false 时不注册任何东西 |
| `Endpoints` | 空集合 | 多文档组配置 |
| `AllParametersInCamelCase` | `false` | true 时全部参数名小驼峰 |
| `HideSchemas` | `true` | true 时折叠模型（`DefaultModelsExpandDepth(-1)`） |
| `DocExpansion` | `None` | 文档展开级别（`List` / `Full` / `None`） |

`Endpoints` 每项字段：

| 字段 | 说明 |
|---|---|
| `Title` | 文档标题 |
| `Version` | 文档分组名，**同时作为 SwaggerDoc 的注册名** |
| `Url` | json 文档地址 |

> 注意事项

`Version` 既作为分组名也作为文档注册名，需与 Controller 上的 `[ApiExplorerSettings(GroupName = "...")]` 保持一致。

## 中文优化

| 优化点 | 说明 |
|---|---|
| 枚举显示中文 | 候选项重写为 `名称(中文描述)=数值` |
| 标题栏可编辑 | 劫持 Swagger UI 渲染，使标题栏文本可复制 |
| 模型默认折叠 | `HideSchemas` 控制 |
| 文档默认全部折叠 | `DocExpansion = None` |
| 参数小驼峰 | `AllParametersInCamelCase` 控制 |
| XML 注释自动加载 | 自动扫描输出目录下所有 `.xml` |

> 枚举中文示例

给枚举字段加 `[Description]` 即可生效：

```csharp
public enum FunctionAccessType
{
    [Description("匿名")] Anonymous = 0,
    [Description("登录")] Login = 1,
    [Description("角色")] RoleLimit = 2,
    [Description("权限资源")] AuthorityLimit = 3,
    [Description("角色及权限资源")] RoleAuthorityLimit = 4
}
```

Swagger 中显示为：

```
Anonymous(匿名)=0
Login(登录)=1
RoleLimit(角色)=2
AuthorityLimit(权限资源)=3
RoleAuthorityLimit(角色及权限资源)=4
```

> XML 注释

需在项目 csproj 中开启：

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

```csharp
/// <summary>
///     查询用户详情
/// </summary>
[HttpGet("{id:long}")]
public async Task<CommonResult<UserDto>> GetAsync(long id) => Ok();
```

`SwaggerModule` 会自动加载输出目录中的所有 `.xml`，**无需任何额外配置**。

## Bearer 授权

自动注册 `Bearer` 安全定义，Swagger UI 右上角出现 **Authorize** 按钮，填入 `Bearer xxx` 即可对所有接口免去手填 Token。

> 配合鉴权使用

```csharp
[HttpGet("list")]
[Authorize(Policy = FunctionRequirement.Policy, Roles = "admin")]
public async Task<CommonResult> ListAsync() => CommonResult.Success();
```

## 端点分组

分组需要三处对应配置。

> 第1步 Controller 上标分组名

```csharp
using Microsoft.AspNetCore.Mvc;

[Area("system")]
[Route("api/[area]/[controller]")]
[ApiExplorerSettings(GroupName = "eleAdmin")]        // 需与配置里的 Version 一致
[Tags("系统-用户"), Description("系统-用户")]          // 中文分组标题
public class SysUserController : ControllerBase { }
```

> 第2步 配置中每组一份文档

```json
"Endpoints": [
    { "Title": "EleAdmin 后台接口", "Version": "eleAdmin", "Url": "/swagger/eleAdmin/swagger.json" },
    { "Title": "配置中心服务端接口", "Version": "config",   "Url": "/swagger/config/swagger.json" },
    { "Title": "工作流接口",       "Version": "Workflow", "Url": "/swagger/Workflow/swagger.json" }
]
```

> 第3步 访问

```
/swagger/index.html
```

Swagger UI 右上角出现文档下拉框可切换多份文档。

## 使用示例

> 基础用法

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddFindx().AddModules();   // 自动装载 SwaggerModule
builder.Services.AddControllers();

var app = builder.Build();
app.UseFindx();                             // 内部执行 UseSwagger + UseSwaggerUI
app.UseEndpointsWithAreaRoute();
app.UseFindxHosting();

// 浏览: /swagger/index.html
```

> 启用被注释的功能

标签重排与标签分组两个功能在源码中默认未启用，可在自定义模块中追加：

```csharp
public class MySwaggerExtraModule : WebApplicationModuleBase
{
    public override ModuleLevel Level => ModuleLevel.Application;
    public override int Order => 41;   // 必须 > 40，紧随 SwaggerModule

    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.Configure<SwaggerGenOptions>(options =>
        {
            options.DocumentFilter<TagOrderDocumentFilter>();   // 标签按接口顺序重排
            options.TagActionsBy(d =>
            {
                var tagAttr = d.ActionDescriptor.EndpointMetadata.OfType<TagsAttribute>().FirstOrDefault();
                return tagAttr != null
                    ? tagAttr.Tags.ToList()
                    : [d.ActionDescriptor.As<ControllerActionDescriptor>().ControllerName];
            });
        });
        return services;
    }
}
```

`Configure<SwaggerGenOptions>` 是**追加**而非替换，两者可共存。

> 注意事项

- `SwaggerEndpoint.Servers` 字段当前**未被消费**，属预留配置。
- 包内 `SwaggerModule.bak.cs` 是整份注释掉的 net10.0 `AddOpenApi` 方案备份，不参与编译。