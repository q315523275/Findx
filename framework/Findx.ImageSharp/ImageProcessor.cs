using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Findx.Exceptions;
using Findx.Extensions;
using Findx.Imaging;
using Findx.Utilities;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using FontStyle = SixLabors.Fonts.FontStyle;

namespace Findx.ImageSharp;

/// <summary>
/// 图片处理器
/// </summary>
public sealed class ImageProcessor : IImageProcessor
{
    private readonly IFontFamilyProvider _fontFamilyProvider;
    
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="fontFamilyProvider"></param>
    public ImageProcessor(IFontFamilyProvider fontFamilyProvider)
    {
        _fontFamilyProvider = fontFamilyProvider;
    }
    
    #region 尺寸调整

    /// <summary>
    /// 尺寸缩放
    /// </summary>
    /// <param name="imageByte"></param>
    /// <param name="imageResizeDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<byte[]> ResizeAsync(byte[] imageByte, ImageResizeDto imageResizeDto,  CancellationToken cancellationToken = default)
    {
        using var originalImage = Image.Load(imageByte);

        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        
        if (!CanResize(imageFormat?.DefaultMimeType))
            throw new FindxException("Unsupported", "图片格式不支持");
        
        if (ResizeModeMap.TryGetValue(imageResizeDto.Mode, out var resizeMode))
        {
            originalImage.Mutate(x => x.Resize(new ResizeOptions { Size = GetSize(imageResizeDto), Mode = resizeMode }));
        }
        else
        {
            throw new FindxException("Unsupported", $"图片缩放模式{imageResizeDto.Mode}不支持");
        }

        return await originalImage.SaveAndGetAllBytesAsync(imageFormat, cancellationToken);
    }

    /// <summary>
    /// 尺寸调整
    /// </summary>
    /// <param name="imageStream"></param>
    /// <param name="imageResizeDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Stream> ResizeAsync(Stream imageStream, ImageResizeDto imageResizeDto, CancellationToken cancellationToken = default)
    {
        using var originalImage = await Image.LoadAsync(imageStream, cancellationToken);
        
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        
        if (!CanResize(imageFormat?.DefaultMimeType))
            throw new FindxException("Unsupported", "图片格式不支持");

        if (ResizeModeMap.TryGetValue(imageResizeDto.Mode, out var resizeMode))
        {
            originalImage.Mutate(x => x.Resize(new ResizeOptions
                { Size = GetSize(imageResizeDto), Mode = resizeMode }));
        }
        else
        {
            throw new FindxException("Unsupported", $"图片缩放模式{imageResizeDto.Mode}不支持");
        }

        return await originalImage.SaveAndGetAllStreamAsync(imageFormat, cancellationToken);
    }

    private static bool CanResize(string mimeType)
    {
        return mimeType switch {
            MimeTypeUtility.Image.Jpeg => true,
            MimeTypeUtility.Image.Png => true,
            MimeTypeUtility.Image.Gif => true,
            MimeTypeUtility.Image.Bmp => true,
            MimeTypeUtility.Image.Tiff => true,
            MimeTypeUtility.Image.Webp => true,
            _ => false
        };
    }

    private static readonly Dictionary<ImageResizeMode, ResizeMode> ResizeModeMap = new() {
        { ImageResizeMode.None, default },
        { ImageResizeMode.Stretch, ResizeMode.Stretch },
        { ImageResizeMode.BoxPad, ResizeMode.BoxPad },
        { ImageResizeMode.Min, ResizeMode.Min },
        { ImageResizeMode.Max, ResizeMode.Max },
        { ImageResizeMode.Crop, ResizeMode.Crop },
        { ImageResizeMode.Pad, ResizeMode.Pad }
    };
    
    private static Size GetSize(ImageResizeDto imageResizeDto)
    {
        var size = new Size();
        
        if (imageResizeDto.Width > 0)
        {
            size.Width = imageResizeDto.Width;
        }

        if (imageResizeDto.Height > 0)
        {
            size.Height = imageResizeDto.Height;
        }

        return size;
    }
    #endregion

    #region 指定区域裁剪
    
    /// <summary>
    /// 指定区域裁剪
    /// </summary>
    /// <param name="imageByte"></param>
    /// <param name="imageCropDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<byte[]> CropAsync(byte[] imageByte, ImageCropDto imageCropDto, CancellationToken cancellationToken = default)
    {
        using var originalImage = Image.Load(imageByte);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        
        if (!CanResize(imageFormat?.DefaultMimeType))
            throw new FindxException("Unsupported", "图片格式不支持裁剪");
        
        originalImage.Mutate(m => { m.Crop(new Rectangle(imageCropDto.X, imageCropDto.Y, imageCropDto.Width, imageCropDto.Height)); });
        
        return await originalImage.SaveAndGetAllBytesAsync(imageFormat, cancellationToken);
    }

    /// <summary>
    /// 指定区域裁剪
    /// </summary>
    /// <param name="imageStream"></param>
    /// <param name="imageCropDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Stream> CropAsync(Stream imageStream, ImageCropDto imageCropDto, CancellationToken cancellationToken = default)
    {
        using var originalImage = await Image.LoadAsync(imageStream, cancellationToken);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;

        if (!CanResize(imageFormat?.DefaultMimeType))
            throw new FindxException("Unsupported", "图片格式不支持裁剪");

        originalImage.Mutate(m =>
        {
            m.Crop(new Rectangle(imageCropDto.X, imageCropDto.Y, imageCropDto.Width, imageCropDto.Height));
        });

        return await originalImage.SaveAndGetAllStreamAsync(imageFormat, cancellationToken);
    }

    #endregion

    #region 图片压缩

    /// <summary>
    ///     图片压缩
    /// </summary>
    /// <param name="imageByte"></param>
    /// <param name="quality">压缩比率：alpha 必须是范围 (0~100] 的数字</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<byte[]> CompressAsync(byte[] imageByte, int quality = 75, CancellationToken cancellationToken = default)
    {
        using var originalImage = Image.Load(imageByte);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        
        if (!CanCompress(imageFormat?.DefaultMimeType))
            throw new FindxException("Unsupported", "图片格式不支持压缩");

        var encoder = GetCompressEncoder(imageFormat, quality);
        
        using var memoryStream = Pool.MemoryStream.Rent();
        await originalImage.SaveAsync(memoryStream, encoder, cancellationToken: cancellationToken);
        return memoryStream.ToArray();
    }

    /// <summary>
    ///     图片压缩
    /// </summary>
    /// <param name="imageStream"></param>
    /// <param name="quality">压缩比率：alpha 必须是范围 (0~100] 的数字</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Stream> CompressAsync(Stream imageStream, int quality = 75, CancellationToken cancellationToken = default)
    {
        using var originalImage = await Image.LoadAsync(imageStream, cancellationToken);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        
        if (!CanCompress(imageFormat?.DefaultMimeType))
            throw new FindxException("Unsupported", "图片格式不支持压缩");

        var encoder = GetCompressEncoder(imageFormat, quality);

        var memoryStream = Pool.MemoryStream.Rent();
        await originalImage.SaveAsync(memoryStream, encoder, cancellationToken: cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;

    }

    private static bool CanCompress(string mimeType)
    {
        return mimeType switch {
            MimeTypeUtility.Image.Jpeg => true,
            MimeTypeUtility.Image.Png => true,
            MimeTypeUtility.Image.Webp => true,
            _ => false
        };
    }

    private static IImageEncoder GetCompressEncoder(IImageFormat format, int quality)
    {
        return format.DefaultMimeType switch
        {
            MimeTypeUtility.Image.Jpeg => new JpegEncoder { Quality = quality },
            MimeTypeUtility.Image.Png => new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression },
            MimeTypeUtility.Image.Webp => new WebpEncoder { Quality = quality },
            _ => throw new FindxException($"图片格式{format.Name}不支持压缩")
        };
    }
    #endregion

    #region 图片灰化
    
    /// <summary>
    ///     图片灰色化
    /// </summary>
    /// <param name="imageByte">图片字节数组</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<byte[]> GrayAsync(byte[] imageByte, CancellationToken cancellationToken = default)
    {
        using var originalImage = Image.Load<Rgba32>(imageByte);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        
        if (!CanResize(imageFormat?.DefaultMimeType))
            throw new FindxException("Unsupported", "图片格式不支持灰色化");
        
        // 像素点逐个替换
        originalImage.ToGray();

        return await originalImage.SaveAndGetAllBytesAsync(imageFormat, cancellationToken);
    }

    /// <summary>
    ///     图片灰色化
    /// </summary>
    /// <param name="imageStream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Stream> GrayAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        using var originalImage = await Image.LoadAsync<Rgba32>(imageStream, cancellationToken);

        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        
        if (!CanResize(imageFormat?.DefaultMimeType))
            throw new FindxException("Unsupported", "图片格式不支持灰色化");

        // 像素点逐个替换
        originalImage.ToGray();

        return await originalImage.SaveAndGetAllStreamAsync(imageFormat, cancellationToken);
    }
    #endregion

    #region 黑白二值化处理

    /// <summary>
    ///     图片二值化
    /// </summary>
    /// <param name="imageByte">图片字节数组</param>
    /// <param name="threshold">阀值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<byte[]> BinaryAsync(byte[] imageByte, int threshold = 180, CancellationToken cancellationToken = default)
    {
        using var originalImage = Image.Load<Rgba32>(imageByte);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        
        if (!CanResize(imageFormat?.DefaultMimeType))
            throw new FindxException("Unsupported", "图片格式不支持二值化");
        
        // 像素点逐个替换
        originalImage.ToBinary(threshold);

        return await originalImage.SaveAndGetAllBytesAsync(imageFormat, cancellationToken);
    }
    
    /// <summary>
    ///     图片二值化
    /// </summary>
    /// <param name="imageStream">图片字节数组</param>
    /// <param name="threshold">阀值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Stream> BinaryAsync(Stream imageStream, int threshold = 180, CancellationToken cancellationToken = default)
    {
        using var originalImage = await Image.LoadAsync<Rgba32>(imageStream, cancellationToken);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        
        if (!CanResize(imageFormat?.DefaultMimeType))
            throw new FindxException("Unsupported", "图片格式不支持二值化");

        // 像素点逐个替换
        originalImage.ToBinary(threshold);

        return await originalImage.SaveAndGetAllStreamAsync(imageFormat, cancellationToken);
    }

    #endregion

    #region 旋转

    /// <summary>
    ///     图片旋转
    /// </summary>
    /// <param name="byteData"></param>
    /// <param name="degrees"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<byte[]> RotateAsync(byte[] byteData, float degrees, CancellationToken cancellationToken = default)
    {
        using var originalImage = Image.Load(byteData);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        
        if (!CanResize(imageFormat?.DefaultMimeType))
            throw new FindxException("Unsupported", "图片格式不支持旋转");
        
        originalImage.Mutate(m => { m.Rotate(degrees); });

        return await originalImage.SaveAndGetAllBytesAsync(imageFormat, cancellationToken);
    }

    /// <summary>
    /// 图片旋转
    /// </summary>
    /// <param name="imageStream"></param>
    /// <param name="degrees"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FindxException"></exception>
    public async Task<Stream> RotateAsync(Stream imageStream, float degrees, CancellationToken cancellationToken = default)
    {
        using var originalImage = await Image.LoadAsync(imageStream, cancellationToken);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        
        
        if (!CanResize(imageFormat?.DefaultMimeType))
            throw new FindxException("Unsupported", "图片格式不支持旋转");

        originalImage.Mutate(m => { m.Rotate(degrees); });

        return await originalImage.SaveAndGetAllStreamAsync(imageFormat, cancellationToken);
    }

    #endregion

    #region 翻转
    
    /// <summary>
    /// 图片翻转
    /// </summary>
    /// <param name="imageByte"></param>
    /// <param name="imageFlipMode"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<byte[]> FlipAsync(byte[] imageByte, ImageFlipMode imageFlipMode, CancellationToken cancellationToken = default)
    {
        using var originalImage = Image.Load(imageByte);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        
        if (!CanResize(imageFormat?.DefaultMimeType))
            throw new FindxException("Unsupported", "图片格式不支持翻转");
        
        if (FlipModeMap.TryGetValue(imageFlipMode, out var flipMode))
        {
            originalImage.Mutate(m => { m.Flip(flipMode); });
        }
        else
        {
            throw new FindxException("Unsupported", $"图片翻转模式{imageFlipMode}不支持");
        }

        return await originalImage.SaveAndGetAllBytesAsync(imageFormat, cancellationToken);
    }

    /// <summary>
    /// 图片翻转
    /// </summary>
    /// <param name="imageStream"></param>
    /// <param name="imageFlipMode"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Stream> FlipAsync(Stream imageStream, ImageFlipMode imageFlipMode, CancellationToken cancellationToken = default)
    {
        using var originalImage = await Image.LoadAsync(imageStream, cancellationToken);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;

        if (!CanResize(imageFormat?.DefaultMimeType))
            throw new FindxException("Unsupported", "图片格式不支持翻转");

        if (FlipModeMap.TryGetValue(imageFlipMode, out var flipMode))
        {
            originalImage.Mutate(m => { m.Flip(flipMode); });
        }
        else
        {
            throw new FindxException("Unsupported", $"图片翻转模式{imageFlipMode}不支持");
        }

        return await originalImage.SaveAndGetAllStreamAsync(imageFormat, cancellationToken);
    }

    private static readonly Dictionary<ImageFlipMode, FlipMode> FlipModeMap = new() {
        { ImageFlipMode.Horizontal, FlipMode.Horizontal },
        { ImageFlipMode.Vertical, FlipMode.Vertical }
    };
    
    #endregion

    #region 合并图片
    
    /// <summary>
    ///     合并图片
    /// </summary>
    /// <param name="imageByte">图片字节数组</param>
    /// <param name="mergeImageByte">合并图片字节数组</param>
    /// <param name="mergeImageDto">合并图片参数</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<byte[]> MergeImageAsync(byte[] imageByte, byte[] mergeImageByte, MergeImageDto mergeImageDto, CancellationToken cancellationToken = default)
    {
        var mergeImage = Image.Load(mergeImageByte);
        var mergeImageFormat = mergeImage.Metadata.DecodedImageFormat;

        using (mergeImage)
        {
            // 合并图片需要缩放
            if (mergeImageDto.Width > 0 || mergeImageDto.Height > 0)
            {
                if (!CanResize(mergeImageFormat?.DefaultMimeType))
                    throw new FindxException("Unsupported", "图片格式不支持缩放");

                if (ResizeModeMap.TryGetValue(mergeImageDto.ResizeMode, out var resizeMode))
                {
                    mergeImage.Mutate(x => x.Resize(new ResizeOptions { Size = GetSize(mergeImageDto), Mode = resizeMode }));
                }
                else
                {
                    throw new FindxException("Unsupported", $"图片缩放模式{mergeImageDto.ResizeMode}不支持");
                }
            }

            using var originalImage = Image.Load(imageByte);
            var imageFormat = originalImage.Metadata.DecodedImageFormat;
            originalImage.Mutate(o =>
            {
                o.DrawImage(mergeImage, new Point(mergeImageDto.X, mergeImageDto.Y), mergeImageDto.Opacity);
            });

            return await originalImage.SaveAndGetAllBytesAsync(imageFormat, cancellationToken);
        }
    }

    /// <summary>
    ///     合并图片
    /// </summary>
    /// <param name="imageStream">图片数据流</param>
    /// <param name="mergeImageStream">合并图片数据流</param>
    /// <param name="mergeImageDto">合并图片参数</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Stream> MergeImageAsync(Stream imageStream, Stream mergeImageStream, MergeImageDto mergeImageDto, CancellationToken cancellationToken = default)
    {
        var mergeImage = await Image.LoadAsync(mergeImageStream, cancellationToken);
        var mergeImageFormat = mergeImage.Metadata.DecodedImageFormat;

        using (mergeImage)
        {
            // 合并图片需要缩放
            if (mergeImageDto.Width > 0 || mergeImageDto.Height > 0)
            {
                if (!CanResize(mergeImageFormat?.DefaultMimeType))
                    throw new FindxException("Unsupported", "图片格式不支持缩放");

                if (ResizeModeMap.TryGetValue(mergeImageDto.ResizeMode, out var resizeMode))
                {
                    mergeImage.Mutate(x => x.Resize(new ResizeOptions
                        { Size = GetSize(mergeImageDto), Mode = resizeMode }));
                }
                else
                {
                    throw new FindxException("Unsupported", $"图片缩放模式{mergeImageDto.ResizeMode}不支持");
                }
            }

            using var originalImage = await Image.LoadAsync(imageStream, cancellationToken);
            var imageFormat = originalImage.Metadata.DecodedImageFormat;
            originalImage.Mutate(o =>
            {
                o.DrawImage(mergeImage, new Point(mergeImageDto.X, mergeImageDto.Y), mergeImageDto.Opacity);
            });

            return await originalImage.SaveAndGetAllStreamAsync(imageFormat, cancellationToken);
        }
    }

    private static Size GetSize(MergeImageDto imageResizeDto)
    {
        var size = new Size();
        
        if (imageResizeDto.Width > 0)
        {
            size.Width = imageResizeDto.Width;
        }

        if (imageResizeDto.Height > 0)
        {
            size.Height = imageResizeDto.Height;
        }

        return size;
    }
    #endregion

    #region 绘制多行文本
    
    /// <summary>
    ///     绘制多行文本
    /// </summary>
    /// <param name="imageByte"></param>
    /// <param name="text"></param>
    /// <param name="drawTextDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<byte[]> DrawTextAsync(byte[] imageByte, string text, DrawTextDto drawTextDto, CancellationToken cancellationToken = default)
    {
        var font = CreateFont(drawTextDto);
        var textOptions = new RichTextOptions(font)
        {
            Origin = new PointF(drawTextDto.X, drawTextDto.Y),
            WrappingLength = drawTextDto.WrappingLength,
            HorizontalAlignment = HorizontalAlignment.Left,
            LineSpacing = drawTextDto.LineSpacing,
        };
        var textBrush = Brushes.Solid(Color.ParseHex(drawTextDto.FontColor));
        
        using var originalImage = Image.Load(imageByte);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        if (drawTextDto.UseOverlay)
        {
            var textSize = TextMeasurer.MeasureSize(text, textOptions);
            var overlay = new Image<Rgba32>((int)textSize.Width + drawTextDto.X * 2, (int)textSize.Height + drawTextDto.Y * 2);
            using (overlay)
            {
                overlay.Mutate(ctx =>
                {
                    ctx.Fill(Color.ParseHex(drawTextDto.OverlayColor).WithAlpha(drawTextDto.OverlayOpacity));
                    ctx.DrawText(textOptions: textOptions, text: text, brush: textBrush);
                });
                originalImage.Mutate(o => { o.DrawImage(overlay, new Point(drawTextDto.OverlayX, drawTextDto.OverlayY), opacity: 1); });
            }
        }
        else
        {
            originalImage.Mutate(o => { o.DrawText(textOptions, text, textBrush); });
        }
        

        return await originalImage.SaveAndGetAllBytesAsync(imageFormat, cancellationToken);
    }

    /// <summary>
    /// 绘制多行文本
    /// </summary>
    /// <param name="imageStream"></param>
    /// <param name="text"></param>
    /// <param name="drawTextDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Stream> DrawTextAsync(Stream imageStream, string text, DrawTextDto drawTextDto, CancellationToken cancellationToken = default)
    {
        var font = CreateFont(drawTextDto);
        var textOptions = new RichTextOptions(font)
        {
            Origin = new PointF(drawTextDto.X, drawTextDto.Y),
            WrappingLength = drawTextDto.WrappingLength,
            HorizontalAlignment = HorizontalAlignment.Left,
            LineSpacing = drawTextDto.LineSpacing,
        };
        var textBrush = Brushes.Solid(Color.ParseHex(drawTextDto.FontColor));
        
        using var originalImage = await Image.LoadAsync(imageStream, cancellationToken);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        if (drawTextDto.UseOverlay)
        {
            var textSize = TextMeasurer.MeasureSize(text, textOptions);
            var overlay = new Image<Rgba32>((int)textSize.Width + drawTextDto.X * 2, (int)textSize.Height + drawTextDto.Y * 2);
            using (overlay)
            {
                overlay.Mutate(ctx =>
                {
                    ctx.Fill(Color.ParseHex(drawTextDto.OverlayColor).WithAlpha(drawTextDto.OverlayOpacity));
                    ctx.DrawText(textOptions, text, textBrush);
                });
                originalImage.Mutate(o => { o.DrawImage(overlay, new Point(drawTextDto.OverlayX, drawTextDto.OverlayY), opacity: 1); });
            }
        }
        else
        {
            originalImage.Mutate(o => { o.DrawText(textOptions, text, textBrush); });
        }
            
        return await originalImage.SaveAndGetAllStreamAsync(imageFormat, cancellationToken);
    }
    
    private Font CreateFont(DrawTextDto drawTextDto)
    {
        if (_fontFamilyProvider.TryGet(drawTextDto.FontFamily.SafeString(), out var family))
        {
            return family.CreateFont(drawTextDto.FontSize, ConvertToFontStyle(drawTextDto.FontStyle));
        }
        
        // 字体不存在且没有字体文件
        if (!_fontFamilyProvider.TryGet(drawTextDto.FontFamily.SafeString(), out _) && drawTextDto.FontFamilyPath.IsNullOrWhiteSpace())
        {
            return SystemFonts.CreateFont(drawTextDto.FontFamily ?? "Arial", drawTextDto.FontSize, ConvertToFontStyle(drawTextDto.FontStyle));
        }
        
        // 字体不存在但有字体文件
        if (!_fontFamilyProvider.TryGet(drawTextDto.FontFamily.SafeString(), out _) && !drawTextDto.FontFamilyPath.IsNullOrWhiteSpace())
        {
            var fontFamily = _fontFamilyProvider.Create(drawTextDto.FontFamilyPath);
            return fontFamily.CreateFont(drawTextDto.FontSize, ConvertToFontStyle(drawTextDto.FontStyle));
        }

        return SystemFonts.CreateFont("Arial", drawTextDto.FontSize, ConvertToFontStyle(drawTextDto.FontStyle));
    }
    
    private static FontStyle ConvertToFontStyle(Imaging.FontStyle fontStyle)
    {
        switch (fontStyle)
        {
            case Imaging.FontStyle.Bold:
                return FontStyle.Bold;
            case Imaging.FontStyle.Italic:
                return FontStyle.Italic;
            case Imaging.FontStyle.BoldItalic:
                return FontStyle.BoldItalic;
            default:
            case Imaging.FontStyle.Regular:
                return FontStyle.Regular;
        }
    }
    
    #endregion

    #region 图片水印

    /// <summary>
    ///     图片水印
    /// </summary>
    /// <param name="imageByte">图片字节数组</param>
    /// <param name="watermarkImageByte">水印文件物理路径</param>
    /// <param name="location">图片水印位置：0=不使用 1=左上 2=中上 3=右上 4=中左 5=中中 6=中右 7=下左 8=下中 9=下右</param>
    /// <param name="opacity">图片水印透明度：alpha 必须是范围 [0.0, 1.0] 之内（包含边界值）的一个浮点数字</param>
    /// <param name="cancellationToken"></param>
    public async Task<byte[]> ImageWatermarkAsync(byte[] imageByte, byte[] watermarkImageByte, int location, float opacity, CancellationToken cancellationToken = default)
    {
        // 装载原图
        using var originalImage = Image.Load(imageByte);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        
        // 装载水印图片
        var watermarkImage = Image.Load(watermarkImageByte);
        using (watermarkImage)
        {
            var point = GetWatermarkPoint(location, originalImage.Width, originalImage.Height, watermarkImage.Width,
                watermarkImage.Height);
            // 绘制水印
            originalImage.Mutate(o => { o.DrawImage(watermarkImage, point, opacity); });
            return await originalImage.SaveAndGetAllBytesAsync(imageFormat, cancellationToken);
        }
    }

    /// <summary>
    ///     图片水印
    /// </summary>
    /// <param name="imageStream">图片字节数组</param>
    /// <param name="watermarkImageStream">水印文件物理路径</param>
    /// <param name="location">图片水印位置：0=不使用 1=左上 2=中上 3=右上 4=中左 5=中中 6=中右 7=下左 8=下中 9=下右</param>
    /// <param name="opacity">图片水印透明度：alpha 必须是范围 [0.0, 1.0] 之内（包含边界值）的一个浮点数字</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Stream> ImageWatermarkAsync(Stream imageStream, Stream watermarkImageStream, int location, float opacity, CancellationToken cancellationToken = default)
    {
        // 装载原图
        using var originalImage = await Image.LoadAsync(imageStream, cancellationToken);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        
        // 装载水印图片
        var watermarkImage = await Image.LoadAsync(watermarkImageStream, cancellationToken);
        using (watermarkImage)
        {
            var point = GetWatermarkPoint(location, originalImage.Width, originalImage.Height, watermarkImage.Width,
                watermarkImage.Height);
            // 绘制水印
            originalImage.Mutate(o => { o.DrawImage(watermarkImage, point, opacity); });
            return await originalImage.SaveAndGetAllStreamAsync(imageFormat, cancellationToken);
        }
    }

    /// <summary>
    /// 获取水印Size位置
    /// </summary>
    /// <param name="location">0=不使用 1=左上 2=中上 3=右上 4=中左 5=中中 6=中右 7=下左 8=下中 9=下右</param>
    /// <param name="originWidth"></param>
    /// <param name="originHeight"></param>
    /// <param name="watermarkWidth"></param>
    /// <param name="watermarkHeight"></param>
    /// <returns></returns>
    private static Point GetWatermarkPoint(int location, int originWidth, int originHeight, int watermarkWidth, int watermarkHeight)
    {
        var point = new Point();
        switch (location)
        {
            case 1:
                point.X = (int)(originWidth * (float).01);
                point.Y = (int)(originHeight * (float).01);
                break;
            case 2:
                point.X = (originWidth - watermarkWidth) / 2;
                point.Y = (int)(originHeight * (float).01);
                break;
            case 3:
                point.X = (int)(originWidth * (float).99 - watermarkWidth);
                point.Y = (int)(originHeight * (float).01);
                break;
            case 4:
                point.X = (int)(originWidth * (float).01);
                point.Y = (originHeight- watermarkHeight) / 2;
                break;
            case 5:
                point.X = (originWidth - watermarkWidth) / 2;
                point.Y = (originHeight- watermarkHeight) / 2;
                break;
            case 6:
                point.X = (int)(originWidth * (float).99 - watermarkWidth);
                point.Y = (originHeight- watermarkHeight) / 2;
                break;
            case 7:
                point.X = (int)(originWidth * (float).01);
                point.Y = (int)(originHeight * (float).99 - watermarkHeight);
                break;
            case 8:
                point.X = (originWidth - watermarkWidth) / 2;
                point.Y = (int)(originHeight * (float).99 - watermarkHeight);
                break;
            case 9:
                point.X = (int)(originWidth * (float).99 - watermarkWidth);
                point.Y = (int)(originHeight * (float).99 - watermarkHeight);
                break;
            default:
                point.X = 0;
                point.Y = 0;
                break;
        }

        return point;
    }

    #endregion

    #region 文字水印

    /// <summary>
    ///     文字水印
    /// </summary>
    /// <param name="imageByte">图片字节数组</param>
    /// <param name="text">水印文字</param>
    /// <param name="location">图片水印位置：0=不使用 1=左上 2=中上 3=右上 4=中左 5=中中 6=中右 7=下左 8=下中 9=下右</param>
    /// <param name="drawTextDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<byte[]> LetterWatermarkAsync(byte[] imageByte, string text, int location, DrawTextDto drawTextDto, CancellationToken cancellationToken = default)
    {
        var font = CreateFont(drawTextDto);
        var textOptions = new RichTextOptions(font)
        {
            Origin = new PointF(drawTextDto.X, drawTextDto.Y),
            WrappingLength = drawTextDto.WrappingLength,
            HorizontalAlignment = HorizontalAlignment.Left,
            LineSpacing = drawTextDto.LineSpacing,
        };
        var textBrush = Brushes.Solid(Color.ParseHex(drawTextDto.FontColor));
        var textSize = TextMeasurer.MeasureSize(text, textOptions);
        
        using var originalImage = Image.Load(imageByte);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        if (drawTextDto.UseOverlay)
        {
            
            var overlay = new Image<Rgba32>((int)textSize.Width + drawTextDto.X * 2, (int)textSize.Height + drawTextDto.Y * 2);
            using (overlay)
            {
                overlay.Mutate(ctx =>
                {
                    ctx.Fill(Color.ParseHex(drawTextDto.OverlayColor).WithAlpha(drawTextDto.OverlayOpacity));
                    ctx.DrawText(textOptions, text, textBrush);
                });
                var point = GetWatermarkPoint(location, originalImage.Width, originalImage.Height, overlay.Width, overlay.Height);
                originalImage.Mutate(o => { o.DrawImage(overlay, point, opacity: 1); });
            }
        }
        else
        {
            var point = GetWatermarkPoint(location, originalImage.Width, originalImage.Height, (int)textSize.Width, (int)textSize.Height);
            textOptions.Origin = point;
            originalImage.Mutate(o => { o.DrawText(textOptions, text, textBrush); });
        }

        return await originalImage.SaveAndGetAllBytesAsync(imageFormat, cancellationToken);
    }

    /// <summary>
    ///     文字水印
    /// </summary>
    /// <param name="imageStream">图片字节数组</param>
    /// <param name="text">水印文字</param>
    /// <param name="location">图片水印位置：0=不使用 1=左上 2=中上 3=右上 4=中左 5=中中 6=中右 7=下左 8=下中 9=下右</param>
    /// <param name="drawTextDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Stream> LetterWatermarkAsync(Stream imageStream, string text, int location, DrawTextDto drawTextDto, CancellationToken cancellationToken = default)
    {
        var font = CreateFont(drawTextDto);
        var textOptions = new RichTextOptions(font)
        {
            Origin = new PointF(drawTextDto.X, drawTextDto.Y),
            WrappingLength = drawTextDto.WrappingLength,
            HorizontalAlignment = HorizontalAlignment.Left,
            LineSpacing = drawTextDto.LineSpacing,
        };
        var textBrush = Brushes.Solid(Color.ParseHex(drawTextDto.FontColor));
        var textSize = TextMeasurer.MeasureSize(text, textOptions);
        
        using var originalImage = await Image.LoadAsync(imageStream, cancellationToken);
        var imageFormat = originalImage.Metadata.DecodedImageFormat;
        using (originalImage)
        {
            if (drawTextDto.UseOverlay)
            {
            
                var overlay = new Image<Rgba32>((int)textSize.Width + drawTextDto.X * 2, (int)textSize.Height + drawTextDto.Y * 2);
                using (overlay)
                {
                    overlay.Mutate(ctx =>
                    {
                        ctx.Fill(Color.ParseHex(drawTextDto.OverlayColor).WithAlpha(drawTextDto.OverlayOpacity));
                        ctx.DrawText(textOptions, text, textBrush);
                    });
                    var point = GetWatermarkPoint(location, originalImage.Width, originalImage.Height, overlay.Width, overlay.Height);
                    originalImage.Mutate(o => { o.DrawImage(overlay, point, opacity: 1); });
                }
            }
            else
            {
                var point = GetWatermarkPoint(location, originalImage.Width, originalImage.Height, (int)textSize.Width, (int)textSize.Height);
                textOptions.Origin = point;
                originalImage.Mutate(o => { o.DrawText(textOptions, text, textBrush); });
            }

            return await originalImage.SaveAndGetAllStreamAsync(imageFormat, cancellationToken);
        }
    }

    #endregion
}