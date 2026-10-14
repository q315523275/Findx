# Findx.Castle

Castle 组件，提供异步 Aop 功能。

使用基于 `Castle.DynamicProxy` 组件包扩展实现异步动态拦截的第三方组件包：`Castle.Core.AsyncInterceptor`

github：https://github.com/JSkimming/Castle.Core.AsyncInterceptor

> Nuget 包引用

```
Findx.Castle
```

目标框架：`net6.0` / `net8.0` / `net10.0`，底层依赖 `Castle.Core 5.2.1`、`Castle.Core.AsyncInterceptor 2.1.0`。

## 装配方式

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 CastleModule
```

`CastleModule`（`ModuleLevel.Application`，`Order = 120`）注册：

```csharp
services.AddTransient(typeof(CastleAsyncDeterminationInterceptor<>));
services.AddSingleton<IProxyGenerator, ProxyGenerator>();
```

`IProxyGenerator` 来自 `Castle.DynamicProxy` 命名空间。

## 定义拦截器

继承 `InterceptorBase`，重写 `InterceptAsync`。

> 使用示例

```csharp
using Findx.Aspect;

public class TestProxyInterceptor : InterceptorBase
{
    private readonly IKeyGenerator<Guid> _keyGenerator;

    public TestProxyInterceptor(IKeyGenerator<Guid> keyGenerator)
    {
        _keyGenerator = keyGenerator;
    }

    public override async Task InterceptAsync(IMethodInvocation invocation)
    {
        Console.WriteLine($"befor:{_keyGenerator.Create()},:{invocation.Arguments.ToJson()}");
        invocation.ReturnValue = true;
        Console.WriteLine($"after:{_keyGenerator.Create()}");

        await Task.CompletedTask;
    }
}
```

> IMethodInvocation 成员

| 成员 | 类型 | 说明 |
|---|---|---|
| `Arguments` | `object[]` | 方法参数数组 |
| `ArgumentsDictionary` | `IReadOnlyDictionary<string, object>` | 参数键值对（延迟构建） |
| `GenericArguments` | `Type[]` | 泛型参数 |
| `TargetObject` | `object` | 目标对象 |
| `Method` | `MethodInfo` | 目标方法 |
| `ReturnValue` | `object` | 返回值（可写） |
| `ProceedAsync()` | `Task` | 执行目标方法 |

> 拦截器注册

```csharp
var testProxyInterceptorType = typeof(TestProxyInterceptor);
var testInterceptorAdapterType = typeof(CastleAsyncDeterminationInterceptor<>).MakeGenericType(testProxyInterceptorType);
builder.Services.AddTransient(testProxyInterceptorType);
builder.Services.AddTransient(typeof(IMachine), provider =>
{
    var proxyGeneratorInstance = provider.GetRequiredService<IProxyGenerator>();
    return proxyGeneratorInstance.CreateInterfaceProxyWithoutTarget(typeof(IMachine), (IInterceptor)provider.GetRequiredService(testInterceptorAdapterType));
});
```

> 使用

```csharp
[HttpGet("aspect")]
public CommonResult Aspect([FromServices] IMachine machine)
{
    machine.Purchase(1999999);
    return CommonResult.Success();
}
```

```csharp
// 控制台打印
before:3a135b83-a6c1-32da-b7f3-66272cfbc8ad,:[1999999]
after:3a135b83-a6cf-b77d-5a1f-67bdb56ce9cc
```

## 实现结构

| 类 | 说明 |
|---|---|
| `CastleAsyncDeterminationInterceptor<TInterceptor>` | 适配壳，把 Findx 拦截器包成上游 `AsyncDeterminationInterceptor` |
| `CastleAsyncInterceptorAdapter<TInterceptor>` | 重写 `InterceptAsync` 两个重载，产出无返回值 / 有返回值适配器 |
| `CastleMethodInvocationAdapterBase` | 实现 `IMethodInvocation` 的抽象基类 |
| `CastleMethodInvocationAdapter` | `Task` 无返回值版本 |
| `CastleMethodInvocationAdapterWithReturnValue<TResult>` | 有返回值版本 |

## 辅助工具

```csharp
// 脱壳，取出代理对象内部的真实实例
var real = ProxyHelper.UnProxy(proxyObject);
var type = ProxyHelper.GetUnProxiedType(proxyObject);

// 全局忽略类型（需手动 Add 才生效）
DynamicProxyIgnoreTypes.Add<SomeType>();
DynamicProxyIgnoreTypes.Contains(typeof(SomeType));
```

## 注意事项

- `InterceptAsync` 采用**洋葱模型**，需要显式调用 `invocation.ProceedAsync()` 才会执行目标方法；不调用则相当于短路。
- 直接给 `invocation.ReturnValue` 赋值同样可以短路（此时无需调用 `ProceedAsync`）。
- 核心包只提供拦截器抽象，不含任何 AOP 实现，必须引入本包才能获得切面能力。