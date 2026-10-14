# Findx.Guids.NewId

NewId 组件，实现连续有序 Guid 的生成。对应 MassTransit 的 NewId 使用，生成连续有序 Guid，性能比 ABP 有序 Guid 快 4 倍多。

> Nuget 包引用

```
Findx.Guids.NewId
```

目标框架：`net6.0` / `net8.0` / `net10.0`，底层依赖 `NewId 4.0.1`（使用 Flake 算法，仓库 https://github.com/phatboyg/NewId）。

## 装配方式

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 NewIdModule
```

`NewIdModule`（`ModuleLevel.Framework`，`Order = 140`）通过 `Replace` 覆盖核心包的默认实现：

```csharp
services.Replace(new ServiceDescriptor(
    typeof(IGuidGenerator), typeof(MassTransitSequentialGuidGenerator), ServiceLifetime.Singleton));
```

## 实现

整个实现只有一行调用：

```csharp
public class MassTransitSequentialGuidGenerator : IGuidGenerator
{
    public Guid Create() => MassTransit.NewId.NextSequentialGuid();
}
```

## 使用

由于核心包的 `GuidKeyGenerator` 是委托到 `IGuidGenerator` 的薄封装，安装本包后**两个入口都会返回 NewId 的连续递增 Guid**。

```csharp
public class OrderService
{
    private readonly IGuidGenerator _guidGenerator;
    private readonly IKeyGenerator<Guid> _keyGenerator;

    public OrderService(IGuidGenerator guidGenerator, IKeyGenerator<Guid> keyGenerator)
    {
        _guidGenerator = guidGenerator;
        _keyGenerator = keyGenerator;
    }

    public Guid CreateId()
    {
        var id1 = _guidGenerator.Create();     // NewId 连续递增 Guid
        var id2 = _keyGenerator.Create();       // 同样走 NewId
        return id1;
    }
}
```

> 与核心包默认实现的对比

| 生成器 | 算法 | 特点 |
|---|---|---|
| `SequentialGuidGenerator`（核心包默认） | 6 字节毫秒时间戳 + 10 字节随机数 | 仅「有序」，同一时间窗内不连续 |
| `MassTransitSequentialGuidGenerator`（本包） | timestamp 前缀 + comb 序列（Flake 算法） | **连续递增**，写入密集场景友好 |

> Json配置

本包无独立配置节。若要指定底层有序 Guid 的字节序布局，仍使用核心包的配置：

```json
{
    "Findx": {
        "SequentialGuid": {
            "SequentialGuidType": "AsString"
        }
    }
}
```

> 注意事项

- 本包只替换 `IGuidGenerator`，`IKeyGenerator<long>` 仍走雪花 Id 生成器，不受影响。
- `Guid` 主键在 JSON 序列化时建议转字符串输出（配合 `LongStringJsonConverter`）。
- `GuidKeyGenerator` 与 `IGuidGenerator` 注册了两条路径但指向同一个生成器实例，行为一致。