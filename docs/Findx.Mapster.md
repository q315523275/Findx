# Findx.Mapster

Mapster 组件，实现对象的映射功能，高性能。

> Nuget 包引用

```
Findx.Mapster
```

目标框架：`net6.0` / `net8.0` / `net10.0`，底层依赖 `Mapster`（net6.0 为 7.4.0，net8.0/net10.0 为 10.0.12）。

## 装配方式

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 MapsterMapperModule
```

`MapsterMapperModule`（`ModuleLevel.Framework`，`Order = 80`）注册：

```csharp
services.AddSingleton<IObjectMapper, MapsterMapper>();
```

## 接口实现

```csharp
public interface IObjectMapper
{
    TTarget MapTo<TTarget>(object source);
    TTarget MapTo<TSource, TTarget>(TSource source, TTarget target);
}
```

```csharp
public class MapsterMapper : IObjectMapper
{
    public TTarget MapTo<TTarget>(object source) => source.Adapt<TTarget>();

    public TTarget MapTo<TSource, TTarget>(TSource source, TTarget target)
    {
        source.Adapt(target);
        return target;
    }
}
```

## 启动期全局配置

模块的 `UseModule` 阶段会应用全局映射规则：

```csharp
public override void UseModule(IServiceProvider provider)
{
    var option = provider.GetService<IOptions<MappingOptions>>();

    if (option is { Value.IgnoreCase: true })
        TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);

    if (option is { Value.IgnoreNullValues: true })
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);

    TypeAdapterConfig.GlobalSettings.Default.IgnoreAttribute(typeof(IgnoreMappingAttribute));

    MapperExtensions.SetMapper(provider.GetRequiredService<IObjectMapper>());
    base.UseModule(provider);
}
```

> 内置规则

| 规则 | 说明 |
|---|---|
| `IgnoreCase` | 属性名匹配忽略大小写 |
| `IgnoreNullValues` | 源为 null 的属性不覆盖目标 |
| `IgnoreAttribute` | 全局识别 `[IgnoreMapping]` 特性 |

## Json 配置

配置节为 **`Findx:Mapping`**。

```json
{
    "Findx": {
        "Mapping": {
            "IgnoreCase": true,
            "IgnoreNullValues": false
        }
    }
}
```

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `IgnoreCase` | `true` | 属性名匹配是否忽略大小写 |
| `IgnoreNullValues` | `false` | 源为 null 时是否跳过赋值 |

## 使用

> 扩展方法

```csharp
using Findx.Mapping;

var dto = entity.MapTo<UserDto>();          // 静态门面
var user = req.MapTo(existingUser);         // 映射到已有对象
```

> 注入使用

```csharp
public class OrderService
{
    private readonly IObjectMapper _mapper;
    public OrderService(IObjectMapper mapper) => _mapper = mapper;

    public UserDto ToDto(User user) => _mapper.MapTo<UserDto>(user);
}
```

> 忽略指定属性

```csharp
public class UserDto
{
    public string Name { get; set; }

    [IgnoreMapping]                          // 不参与映射
    public string Password { get; set; }
}
```

> 自定义映射规则

```csharp
TypeAdapterConfig<Source, Target>.NewConfig()
    .Map(dest => dest.Name, src => src.UserName)
    .Ignore(dest => dest.Secret);
```

框架已全局注册 `.IgnoreAttribute(typeof(IgnoreMappingAttribute))`，因此 `[IgnoreMapping]` 在任意映射方向都生效。

## 注意事项

- 核心包**不注册** `IObjectMapper`，未引入本包时 `MapTo` 会抛异常（`MapperExtensions` 的静态实例为 null）。
- `MapperExtensions` 使用**静态字段**保存 mapper 实例（全局单例语义），不便于单元测试隔离。
- `TypeAdapterConfig.GlobalSettings` 是 Mapster 全局配置，多模块共享同一实例。