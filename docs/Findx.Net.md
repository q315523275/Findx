# Findx.Net

RPC 组件包（预留）。

> 当前状态

该包目录**只有 `.csproj` 项目文件，没有任何 `.cs` 源文件**，是一个尚未填充的空壳包。

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFrameworks>net6.0;net8.0;net9.0</TargetFrameworks>
        <LangVersion>default</LangVersion>
        <RootNamespace>Findx.RPC</RootNamespace>
    </PropertyGroup>
</Project>
```

| 项 | 值 |
|---|---|
| 根命名空间 | `Findx.RPC` |
| 目标框架 | `net6.0` / `net8.0` / `net9.0`（与其余包的 `net10.0` 不一致） |
| 源文件数 | 0 |
| 项目引用 | 无 |

> 相关能力

RPC 相关的**契约定义**目前放在核心包 `Findx` 中，但尚未有实际消费方：

```csharp
namespace Findx.Net.RPC;

// 空标记接口
public interface IRpcService;

[AttributeUsage(AttributeTargets.Method)]
public class RpcServiceRouteAttribute : Attribute
{
    public RpcServiceRouteAttribute(string template);
    public string Template { set; get; }
}
```

> 现状说明

- `IRpcService` 与 `RpcServiceRouteAttribute` 在全仓库中**没有任何实现方与使用方**，属于预留契约。
- RPC 方向的现有能力分散在其他包：`Findx.WebApiClient`（声明式 HTTP 调用）、`Findx.Discovery`（服务发现与负载均衡）。
- 目标框架缺少 `net10.0`，与其他包不一致，引用前需注意。

如需使用 RPC 相关能力，建议先用 `Findx.WebApiClient` + `Findx.Discovery` 组合实现；待本包补充实现后再迁移。