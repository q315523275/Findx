# Findx.IP

基于 IP2Region 的简单封装，实现纯离线的 IP 归属地查询。

> Nuget 包引用

```
Findx.IP
```

目标框架：`net6.0` / `net8.0` / `net10.0`，底层依赖 `IP2Region.Net 3.0.2`（仓库 https://github.com/lionsoul2014/ip2region）。

## 数据文件

`ip2region.xdb` 数据库文件**已内置在包内**并自动复制到输出目录，无需手工准备。

```xml
<None Include="ip2region.xdb">
    <Pack>true</Pack>
    <PackageCopyToOutput>true</PackageCopyToOutput>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

模块启动时从 `AppDomain.CurrentDomain.BaseDirectory` 读取，使用 `CachePolicy.Content` 把 xdb 全量读入内存（离线查询最快）。

## 装配方式

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 IpGeolocationModule
```

`IpGeolocationModule`（`ModuleLevel.Application`，`Order = 240`）注册：

```csharp
services.AddSingleton<ISearcher>(new Searcher(
    CachePolicy.Content,
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ip2region.xdb")));

services.Replace(new ServiceDescriptor(
    typeof(IIpGeolocation), typeof(IpSearchServiceDefault), ServiceLifetime.Singleton));
```

> 重要

核心包默认注册的是 `NullIpGeolocation`，其所有方法**都会抛 `NotImplementedException`**。必须引入本包才可使用。

## 接口

```csharp
public interface IIpGeolocation
{
    string GetLocationTextByIp(string ipAddress);
    IpGeolocationVo GetLocationByIp(string ipAddress);
    Dictionary<string, string> BatchGetLocationText(IEnumerable<string> ipAddresses);
    Dictionary<string, IpGeolocationVo> BatchGetLocation(IEnumerable<string> ipAddresses);
}
```

> IpGeolocationVo

| 属性 | 说明 |
|---|---|
| `IpAddress` | IP 地址 |
| `Country` / `CountryCode` | 国家 |
| `Province` | 省份 |
| `City` | 城市 |
| `District` | 区县 |
| `Longitude` / `Latitude` | 经纬度 |
| `Isp` | 运营商 |
| `IsInternalIp` | 是否内网地址 |

## 使用

> 简单用法

```csharp
[HttpGet("ipGeolocation")]
public string IpGeolocation([Required] string ip, [FromServices] IIpGeolocation ipGeolocation)
{
    return ipGeolocation.GetLocationTextByIp(ip);
}
```

> 结构化用法

```csharp
// 单个
var vo = _ipGeolocation.GetLocationByIp("8.8.8.8");
// vo.Country / vo.Province / vo.City / vo.Isp

// 批量
var map = _ipGeolocation.BatchGetLocation(new[] { "8.8.8.8", "114.114.114.114" });
```

> 内网地址处理

内网 IP（由 `NetUtility.IsInternalIp` 判定）会短路返回：

| 方法 | 内网 IP 返回值 |
|---|---|
| `GetLocationTextByIp` | `"局域网"` |
| `GetLocationByIp` | `IpGeolocationVo { IsInternalIp = true, IpAddress = ip }` |

## 注意事项

- 查询结果按管道分隔字符串按下标拆分：`0 → Country`、`2 → Province`、`3 → City`、`4 → Isp`。
- 两个 `BatchGet*` 方法均为**逐项循环**，大批量查询建议自行并发。
- IP2Region 数据库为**定期更新**的离线库，精度受数据库版本限制，超大网段可能定位到运营商而非具体地址。