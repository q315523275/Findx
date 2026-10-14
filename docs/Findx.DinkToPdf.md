# Findx.DinkToPdf

Pdf 组件，实现 Html 生成 Pdf 功能，内嵌 `libwkhtmltox.so` 文件，Linux 部署无需额外安装其他组件。

> Nuget 包引用

```
Findx.DinkToPdf
```

目标框架：`net6.0` / `net8.0` / `net10.0`，基于 `wkhtmltopdf` 原生库通过 P/Invoke 封装。

## 装配方式

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 DinkToPdfModule
```

`DinkToPdfModule`（`ModuleLevel.Framework`，`Order = 130`）注册：

```csharp
services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
services.AddSingleton<IPdfConverter, DinkToPdfConverter>();
```

> 平台支持

包内**只打包了 Linux x86-64 版本**的原生库（约 43 MB）。

| 平台 | 支持情况 |
|---|---|
| Linux x86-64 | 支持，包内已带 `libwkhtmltox.so` 并自动复制到输出目录 |
| Windows | **不支持**，需自行提供 `libwkhtmltox.dll` |
| macOS | **不支持**，需自行提供 `libwkhtmltox.dylib` |
| ARM64（Apple Silicon、ARM 服务器） | **不支持**，需替换为对应架构的原生库 |

包内也没有任何 `DllImportResolver` 自定义解析逻辑，非 Linux 平台需自行解决动态库加载路径问题。缺少动态库时会在 `PdfTools.Load()` 抛 `DllNotFoundException`。

## 基础用法

`IPdfConverter` 是核心包的极薄抽象，只有一个方法。

```csharp
public interface IPdfConverter
{
    Task<byte[]> ConvertAsync(string htmlString, CancellationToken cancellationToken = default);
}
```

> 使用示例

```csharp
[HttpGet("textToPdf")]
public async Task<IActionResult> TextToPdfAsync([FromServices] IPdfConverter converter,
    [Required] string text, CancellationToken cancellationToken)
{
    var res = await converter.ConvertAsync(text, cancellationToken);
    return File(res, "application/pdf");
}
```

> 默认转换参数

`DinkToPdfConverter` 内部固定使用以下设置，**无配置项可改**：

| 设置 | 值 |
|---|---|
| 颜色模式 | `ColorMode.Color` |
| 方向 | `Orientation.Portrait` |
| 纸张 | `PaperKind.A4` |
| 编码 | `utf-8` |

> 注意事项

- 内容按 HTML 解析，纯文本建议自行包 `<html><body>...</body></html>`。
- 方法返回 `byte[]`，大文档会占用内存（无流式重载）。
- 当前实现未使用传入的 `cancellationToken`。

## 进阶用法

直接使用底层 `IConverter` 可自定义纸张、页边距、页眉页脚，并订阅转换事件。

```csharp
public interface IConverter
{
    byte[] Convert(IDocument document);
    event EventHandler<PhaseChangedArgs> PhaseChanged;
    event EventHandler<ProgressChangedArgs> ProgressChanged;
    event EventHandler<FinishedArgs> Finished;
    event EventHandler<ErrorArgs> Error;
    event EventHandler<WarningArgs> Warning;
}
```

> 使用示例

```csharp
using Findx.DinkToPdf.Contracts;
using Findx.DinkToPdf.Documents;
using Findx.DinkToPdf.Settings;
using Findx.DinkToPdf.Utils;

public class ReportExporter
{
    private readonly IConverter _converter;
    public ReportExporter(IConverter converter) => _converter = converter;

    public byte[] Export(string html)
    {
        var doc = new HtmlToPdfDocument
        {
            GlobalSettings =
            {
                PaperSize = PaperKind.A4,                       // 隐式转换到 PechkinPaperSize
                Orientation = Orientation.Landscape,
                ColorMode = ColorMode.Grayscale,
                DPI = 300,
                Margins = new MarginSettings { Unit = Unit.Millimeters, Top = 15, Bottom = 15, Left = 10, Right = 10 },
                Out = "",                                       // 空 = 输出到内存缓冲区
                DocumentTitle = "季度报表"
            },
            Objects =
            {
                new ObjectSettings
                {
                    HtmlContent = html,
                    WebSettings =
                    {
                        DefaultEncoding = "utf-8",
                        Background = true,
                        EnableJavascript = true
                    },
                    HeaderSettings = { Center = "季度报表", FontSize = 9, Line = true },
                    FooterSettings = { Right = "第 ${pageNumber} 页", FontSize = 9 },
                    LoadSettings  = { JSDelay = 200, ZoomFactor = 1.0 }
                }
            }
        };

        _converter.Finished += (s, e) => Console.WriteLine($"转换完成: {e.Success}");
        _converter.Error    += (s, e) => Console.WriteLine($"错误: {e.Message}");

        return _converter.Convert(doc);
    }
}
```

## 结构说明

| 目录 | 内容 |
|---|---|
| `Contracts/` | 抽象契约：`IConverter`、`IDocument`、`IObject`、`ITools` |
| `Settings/` | 与 wkhtmltopdf 参数一一映射的强类型 POCO，用 `[WkHtml("name")]` 标注参数名 |
| `Documents/` | `HtmlToPdfDocument`（`Objects` 列表 + `GlobalSettings`） |
| `Utils/` | `PaperKind` 枚举、`[WkHtml]` 特性、P/Invoke 委托类型 |

主要设置类：

| 类 | 说明 |
|---|---|
| `GlobalSettings` | 方向、颜色模式、纸张、DPI、边距、输出路径、文档标题 |
| `ObjectSettings` | HTML 内容、分页、Web 设置、页眉页脚、加载设置 |
| `WebSettings` | 背景图片、JS 开关、打印媒体、最小字号、默认编码、用户样式表 |
| `HeaderSettings` / `FooterSettings` | 页眉页脚文本、字号、字体、分隔线 |
| `LoadSettings` | 认证、JS 延迟、缩放、错误处理策略、代理、自定义请求头 |
| `MarginSettings` | 边距与单位（英寸 / 毫米 / 厘米） |
| `PechkinPaperSize` | 约 110 种纸张尺寸字典 |

> 设置值转换规则

`BasicConverter.ApplyConfig` 通过反射遍历设置对象的属性：

| 类型 | 转换 |
|---|---|
| `bool` | `"true"` / `"false"` |
| `double` | `"0.##"` 格式，不变文化 |
| `Dictionary<string, string>` | 展开为 `name.append` + `name[index]` |
| 其他 | `ToString()` |

嵌套的 `ISettings` 会递归处理。

> 线程安全

`SynchronizedConverter` 把所有转换任务投递到**单一专用后台线程**串行执行（`BlockingCollection` + `Monitor.Wait/Pulse`），这是 wkhtmltopdf `init` / `deinit` 线程亲和性的要求。并发请求会排队，不会并行执行。

## 注意事项

- `SynchronizedConverter` 串行化意味着**同一时刻只能有一个转换任务**，高并发场景需评估。
- `ITools` 需正确 `Dispose()` 以调用 `wkhtmltopdf_deinit()`，框架中已通过 `IConverter` 单例的生命周期管理。
- 本包**没有配置节**，所有参数通过强类型 POCO 在代码中构造。