# Findx.ImageSharp

ImageSharp 组件，实现图片处理功能，包括尺寸调整、裁剪、压缩、灰度化、二值化、旋转、翻转、图片合并、绘制文本、图片水印、文字水印以及验证码生成等。

> Nuget 包引用

```
Findx.ImageSharp
```

目标框架：`net6.0` / `net8.0` / `net10.0`，底层依赖 `SixLabors.ImageSharp 3.1.12`、`SixLabors.ImageSharp.Drawing 2.1.7`、`SixLabors.Fonts 2.1.3`。

> 装配方式

```csharp
builder.Services.AddFindx().AddModules();   // 自动发现 ImageSharpModule
```

`ImageSharpModule`（`ModuleLevel.Framework`，`Order = 60`）注册三个单例：

```csharp
services.AddSingleton<IImageProcessor, ImageProcessor>();
services.AddSingleton<IVerifyCoder, VerifyCoder>();
services.AddSingleton<IFontFamilyProvider, FontFamilyProvider>();
```

本包**没有配置节**，所有参数通过 DTO 传入。

## 图片处理

`IImageProcessor` 提供 11 组能力，每组都有 `byte[]` 与 `Stream` 两个重载，共 22 个方法。

> 接口一览

| 能力 | 方法 | 附加参数（默认值） |
|---|---|---|
| 尺寸缩放 | `ResizeAsync` | `ImageResizeDto(width, height, mode)` |
| 区域裁剪 | `CropAsync` | `ImageCropDto(x, y, width, height)` |
| 压缩 | `CompressAsync` | `int quality = 75` |
| 灰度化 | `GrayAsync` | — |
| 二值化 | `BinaryAsync` | `int threshold = 180` |
| 旋转 | `RotateAsync` | `float degrees` |
| 翻转 | `FlipAsync` | `ImageFlipMode mode` |
| 图片合并 | `MergeImageAsync` | `MergeImageDto(x, y, width, height, resizeMode, opacity)` |
| 绘制文本 | `DrawTextAsync` | `DrawTextDto` |
| 图片水印 | `ImageWatermarkAsync` | `int location, float opacity` |
| 文字水印 | `LetterWatermarkAsync` | `int location, DrawTextDto` |

> 支持的图片格式

| 能力 | 支持 MIME |
|---|---|
| 缩放 / 裁剪 / 灰度 / 二值化 / 旋转 / 翻转 / 水印 / 合并 | `image/jpeg`、`image/png`、`image/gif`、`image/bmp`、`image/tiff`、`image/webp` |
| **压缩** | 仅 `image/jpeg`、`image/png`、`image/webp` |

不支持时抛 `FindxException("Unsupported", "图片格式不支持XX")`。

> 使用示例

```csharp
public class ImageService
{
    private readonly IImageProcessor _processor;

    public ImageService(IImageProcessor processor) => _processor = processor;

    public async Task<byte[]> ResizeAsync(byte[] source)
        => await _processor.ResizeAsync(source, new ImageResizeDto(800, 0, ImageResizeMode.Max));

    public async Task<byte[]> CompressAsync(Stream source)
    {
        await using var result = await _processor.CompressAsync(source, 80, CancellationToken.None);
        return result.ToArray();
    }
}
```

> 注意事项

- **`ImageResizeMode.Default`(7) 不在模式映射表中**，调用 `ResizeAsync` 时**必须显式传 mode**，否则抛 `FindxException`。
- `ImageResizeDto` 的宽高为 `0` 表示该维度不限制，如 `new ImageResizeDto(800, 0, ImageResizeMode.Max)` 表示宽度最大 800、高度自适应。
- **PNG 压缩会忽略 `quality` 参数**，硬编码为最佳压缩级别。
- 返回 `Stream` 的重载来自 `Pool.MemoryStream.Rent()`，**必须 `await using` 释放**。
- 灰度化与二值化是 O(W×H) 逐像素 C# 循环，大图 CPU 开销显著。

## 绘制文本与水印

> DrawTextDto

| 属性 | 类型 | 默认值 | 说明 |
|---|---|---|---|
| `X` / `Y` | `int` | `0` | 文本起始坐标 |
| `LineSpacing` | `float` | `1f` | 行间距 |
| `WrappingLength` | `float` | `-1f` | 自动换行宽度，`-1` 表示不换行 |
| `FontFamily` | `string` | `null` | 字体名（先查字体池） |
| `FontFamilyPath` | `string` | `null` | 字体**文件路径**（字体池未命中时的兜底） |
| `FontSize` | `int` | `12` | 字号 |
| `FontColor` | `string` | `"#000000"` | 十六进制色值 |
| `FontStyle` | `FontStyle` | `Regular` | `Regular` / `Bold` / `Italic` / `BoldItalic` |
| `UseOverlay` | `bool` | `false` | 是否加遮罩底衬 |
| `OverlayColor` | `string` | `"#000000"` | 遮罩色 |
| `OverlayOpacity` | `float` | `0.7f` | 遮罩透明度 |
| `OverlayX` / `OverlayY` | `int` | `10` | 遮罩边距 |

> 水印位置

`ImageWatermarkAsync` 与 `LetterWatermarkAsync` 的 `location` 参数使用九宫格位置值：

| 值 | 位置 | 值 | 位置 |
|---|---|---|---|
| `0` | 不使用 | `5` | 中中 |
| `1` | 左上 | `6` | 中右 |
| `2` | 中上 | `7` | 下左 |
| `3` | 右上 | `8` | 下中 |
| `4` | 中左 | `9` | 下右 |

边缘留白为**图片尺寸的 1%**，大图会留出较大边距。

> 使用示例

```csharp
// 图片水印
await using var wm = watermarkStream;
await _processor.ImageWatermarkAsync(sourceStream, wm, location: 5, opacity: 0.5f, ct);

// 文字水印
var dto = new DrawTextDto
{
    FontFamilyPath = "/app/fonts/MyChineseFont.ttf",   // 中文字体文件路径
    FontSize = 28,
    FontColor = "#FFFFFF",
    WrappingLength = -1f
};
await _processor.LetterWatermarkAsync(source, "版权所有", location: 5, dto, ct);
```

> 注意事项

方法名是 **`LetterWatermark`**（`Letter` 拼写），不是 `TextWatermark`。

## 字体

包内 `fonts/` 目录随包分发两个字体文件：

| 文件 | 格式 | 用途 |
|---|---|---|
| `Actionj.ttf` | TrueType | 手写体 |
| `Sketch-3D-1.otf` | OpenType | 3D 素描风 |

两者作为**内嵌资源**打进程序集，`VerifyCoder` 启动时通过 `GetManifestResourceNames()` 全部加载到字体池。

> 重要

**这两个字体都不包含中日韩（CJK）字形。**

| 场景 | 可用内置字体 | 说明 |
|---|---|---|
| 数字/字母验证码 | 可以 | 靠内嵌字体，与操作系统无关，Linux / Docker 零依赖 |
| 英文文字水印 | 勉强 | 仅覆盖有限拉丁字符集 |
| **中文文字水印 / 绘制文本** | **不可以** | 会渲染为豆腐块（□）或空白，需自备 CJK 字体 |

> 字体解析三级回退

```
① FontFamily 命中字体池        → 直接使用
② 未命中且未给字体文件路径     → SystemFonts.CreateFont(...)（依赖系统字体）
③ 未命中但给了字体文件路径     → 从指定文件加载（可传任意中文字体）
④ 最终兜底                    → SystemFonts.CreateFont("Arial", ...)
```

> Linux 容器注意事项

精简容器镜像（`alpine`、`scratch`）中没有 fontconfig 配置也没有字体文件，此时走分支 ② / ④ 的 `SystemFonts` 会失败。

推荐做法是直接 COPY 一个中文字体到镜像，并用 `FontFamilyPath` 指定：

```csharp
public async Task<byte[]> StampAsync(byte[] source, string text)
{
    var fontPath = OperatingSystem.IsWindows()
        ? @"C:\Windows\Fonts\msyh.ttc"
        : "/usr/share/fonts/truetype/wqy/wqy-zenhei.ttc";

    return await _processor.LetterWatermarkAsync(source, text, 5, new DrawTextDto
    {
        FontFamilyPath = fontPath,
        FontFamily = null,          // 不要命名为内嵌字体，否则命中分支①
        FontSize = 28,
        FontColor = "#FFFFFF"
    });
}
```

另一条路是在启动时把字体灌进全局字体池，之后直接按名字使用：

```csharp
public class FontInitializer : IHostedService
{
    private readonly IFontFamilyProvider _fonts;
    public FontInitializer(IFontFamilyProvider fonts) => _fonts = fonts;

    public Task StartAsync(CancellationToken ct)
    {
        foreach (var file in Directory.GetFiles("/app/fonts", "*.ttf"))
            _fonts.Create(file);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

services.AddHostedService<FontInitializer>();
```

## 验证码

`IVerifyCoder` 提供验证码生成能力。

```csharp
public interface IVerifyCoder
{
    string GetCode(int length, VerifyCodeType verifyCodeType);
    Task<byte[]> CreateImageAsync(string text, int width = 120, int height = 50, int fontSize = 32);
    Task<Stream> CreateImageStreamAsync(string text, int width = 120, int height = 50, int fontSize = 32);
}
```

> 验证码类型

```csharp
public enum VerifyCodeType { Number, NumberAndLetter }
```

`NumberAndLetter` 类型的字符集**刻意排除了易混淆字符**（不含 `0`、`1`、`I`、`O`、`l`）。

> 生成机制

`CreateImageAsync` 逐字符独立绘制，每个字符随机字体、样式、字号（±10%）、颜色与倾斜角度（±15°），并叠加 10 条随机干扰线，最后输出 **PNG** 格式。

> 完整用法

```csharp
public class CaptchaController : ControllerBase
{
    private readonly IVerifyCoder _verifyCoder;
    private readonly ICacheFactory _cacheFactory;

    public CaptchaController(IVerifyCoder verifyCoder, ICacheFactory cacheFactory)
    {
        _verifyCoder = verifyCoder;
        _cacheFactory = cacheFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int width = 150, [FromQuery] int height = 50)
    {
        var uuid = Guid.NewGuid().ToString("N");
        var code = _verifyCoder.GetCode(4, VerifyCodeType.NumberAndLetter);

        // 输出是 PNG，别声明 image/jpeg
        var png = await _verifyCoder.CreateImageAsync(code, width, height);

        var cache = _cacheFactory.Create(CacheType.DefaultMemory);
        await cache.AddAsync($"verifyCode:{uuid}", code.ToLower(), TimeSpan.FromMinutes(5));

        return Json(new { uuid, img = "data:image/png;base64," + Convert.ToBase64String(png) });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromQuery] string uuid, [FromQuery] string code)
    {
        var cache = _cacheFactory.Create(CacheType.DefaultMemory);
        var key = $"verifyCode:{uuid}";
        var saved = cache.Get<string>(key);

        if (saved != null && string.Equals(saved, code.ToLower(), StringComparison.OrdinalIgnoreCase))
        {
            await cache.RemoveAsync(key);   // 一次性消费
            return Json(new { success = true });
        }
        return BadRequest(new { success = false, message = "验证码错误" });
    }
}
```

> 注意事项

- `GetCode` 与 `CreateImageAsync` **无状态绑定**，验证码与会话的映射需自行维护。
- 校验务必**一次性消费**（成功后删除缓存），否则验证码可被重放。
- `CreateImageStreamAsync` 返回的流来自对象池，调用方负责处置。

## 上传即压缩

生产环境常见的「上传时缩放 + 压缩」组合。

```csharp
if (FileUtility.IsImage(fileInfo.Extension) && (imageOptions.Compress || imageOptions.ScaleSize))
{
    using var ms = Pool.MemoryStream.Rent();

    if (imageOptions.ScaleSize)
    {
        // height 传 0 = 高度自适应
        await using var res = await _imageProcessor.ResizeAsync(fileStream,
            new ImageResizeDto(imageOptions.ScaleMaxWidth, 0, imageOptions.ResizeMode), ct);
        await res.CopyToAsync(ms, ct);
        ms.Position = 0;
    }

    if (imageOptions.Compress)
    {
        await using var res = await _imageProcessor.CompressAsync(
            ms.Length > 0 ? ms : fileStream, imageOptions.CompressQuality, ct);
        await res.CopyToAsync(ms, ct);
        ms.Position = 0;
    }

    await _fileStorage.SaveFileAsync(path, ms, ct);
}
else
{
    await _fileStorage.SaveFileAsync(path, fileStream, ct);
}
```

> 注意事项

本包**不提供二维码生成**功能。如需该能力需自行引入 `QRCoder` 或 `ZXing.Net`，再通过 `MergeImageAsync` 把二维码贴到目标图上。