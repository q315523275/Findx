using Findx.Drawing;
using Findx.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.IO;
using FontStyle = SixLabors.Fonts.FontStyle;
namespace Findx.ImageSharp
{
    public class ImageProcessor : IImageProcessor
    {
        /// <summary>
        /// 字体缓存
        /// </summary>
        private readonly static IDictionary<string, FontFamily> FontFamilyDict = new Dictionary<string, FontFamily>();

        /// <summary>
        /// 获取图片缩放形式
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        private ResizeMode GetResizeMode(ImageResizeMode mode)
        {
            switch (mode)
            {
                case ImageResizeMode.Crop:
                    return ResizeMode.Crop;
                case ImageResizeMode.AutoByWidth:
                    return ResizeMode.Max;
                case ImageResizeMode.AutoByHeight:
                    return ResizeMode.Min;
                case ImageResizeMode.Stretch:
                    return ResizeMode.Stretch;
                default:
                case ImageResizeMode.Pad:
                    return ResizeMode.BoxPad;
            }
        }

        private FontStyle ConvertToFontStyle(Drawing.FontStyle fontStyle)
        {
            switch (fontStyle)
            {
                case Drawing.FontStyle.Bold:
                    return FontStyle.Bold;
                case Drawing.FontStyle.Italic:
                    return FontStyle.Italic;
                case Drawing.FontStyle.BoldItalic:
                    return FontStyle.BoldItalic;
                default:
                case Drawing.FontStyle.Regular:
                    return FontStyle.Regular;
            }
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="byteData">文件字节数组</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式(默认填充)</param>
        /// <returns></returns>
        public byte[] Scale(byte[] byteData, int width, int height, ImageResizeMode mode = ImageResizeMode.Pad)
        {
            using (var originalImage = Image.Load(byteData, out IImageFormat imageFormat))
            {
                var option = new ResizeOptions { Size = new Size(width, height), Mode = GetResizeMode(mode) };

                originalImage.Mutate(m => { m.Resize(option); });

                // 返回字节数组
                using (var ms = new MemoryStream())
                {
                    originalImage.SaveImage(ms, imageFormat);
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 合并图片
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="mergeImagePath">合并图片物理路径</param>
        /// <param name="X">X轴</param>
        /// <param name="Y">Y轴</param>
        /// <param name="width">合并图片宽度,0不处理</param>
        /// <param name="height">合并图片高度,0不处理</param>
        /// <returns></returns>
        public byte[] PressImage(byte[] byteData, string mergeImagePath, int X, int Y, int width = 0, int height = 0, float opacity = 1.0f)
        {
            using (var mergeImage = Image.Load(mergeImagePath))
            {
                if (width > 0 && height > 0)
                {
                    mergeImage.Mutate(m => { m.Resize(new Size(width, height)); });
                }

                using (var originalImage = Image.Load(byteData, out IImageFormat imageFormat))
                {
                    originalImage.Mutate(o => { o.DrawImage(mergeImage, new Point(X, Y), opacity); });

                    using (var ms = new MemoryStream())
                    {
                        originalImage.SaveImage(ms, imageFormat);
                        return ms.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// 绘制多行文本
        /// </summary>
        /// <param name="byteData"></param>
        /// <param name="text"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public byte[] PressText(byte[] byteData, int X, int Y, string text, FontOptions fontOptions)
        {
            Check.NotNull(fontOptions, nameof(fontOptions));
            Check.NotNullOrWhiteSpace(text, nameof(text));
            // 设置字体大小与样式
            Font font;
            if (text.IsEnglish() || fontOptions.FontFamilyFilePath.IsNullOrWhiteSpace())
            {
                font = SystemFonts.CreateFont("Arial", fontOptions.FontSize, ConvertToFontStyle(fontOptions.FontStyle));
            }
            else
            {
                Check.NotNullOrWhiteSpace(fontOptions.FontFamilyFilePath, nameof(fontOptions.FontFamilyFilePath));
                // 装载字体文件
                var fontFamily = FontFamilyDict.GetOrAdd(fontOptions.FontFamilyFilePath, () =>
                {
                    var fonts = new FontCollection();
                    return fonts.Add(fontOptions.FontFamilyFilePath);
                });
                font = new Font(fontFamily, fontOptions.FontSize, ConvertToFontStyle(fontOptions.FontStyle));
            }
            // 获取文本绘制所需大小
            // var size = TextMeasurer.Measure(text, new TextOptions(font));
            // 装载图片
            using (var originalImage = Image.Load(byteData, out IImageFormat imageFormat))
            {
                // 绘制文字
                originalImage.Mutate(o => { o.DrawText(text, font, Rgba32.ParseHex(fontOptions.FontColor ?? "#000000"), new PointF(X, Y)); });
                using (var ms = new MemoryStream())
                {
                    originalImage.SaveImage(ms, imageFormat);
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 图片水印
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="imageWatermarkPath">水印文件物理路径</param>
        /// <param name="location">图片水印位置 0=不使用 1=左上 2=中上 3=右上 4=左中  9=右下</param>
        /// <param name="opacity">水印的透明度</param>
        /// <returns></returns>
        public byte[] ImageWatermark(byte[] byteData, string imageWatermarkPath, int location, float opacity)
        {
            // 装载原图
            using (var originalImage = Image.Load(byteData, out IImageFormat imageFormat))
            {
                // 装载水印图片
                using (var watermarkImage = Image.Load(imageWatermarkPath))
                {
                    // 计算位置
                    int xpos = 0;
                    int ypos = 0;
                    switch (location)
                    {
                        case 1:
                            xpos = (int)(originalImage.Width * (float).01);
                            ypos = (int)(originalImage.Height * (float).01);
                            break;
                        case 2:
                            xpos = (int)((originalImage.Width * (float).50) - (watermarkImage.Width / 2));
                            ypos = (int)(originalImage.Height * (float).01);
                            break;
                        case 3:
                            xpos = (int)((originalImage.Width * (float).99) - (watermarkImage.Width));
                            ypos = (int)(originalImage.Height * (float).01);
                            break;
                        case 4:
                            xpos = (int)(originalImage.Width * (float).01);
                            ypos = (int)((originalImage.Height * (float).50) - (watermarkImage.Height / 2));
                            break;
                        case 5:
                            xpos = (int)((originalImage.Width * (float).50) - (watermarkImage.Width / 2));
                            ypos = (int)((originalImage.Height * (float).50) - (watermarkImage.Height / 2));
                            break;
                        case 6:
                            xpos = (int)((originalImage.Width * (float).99) - (watermarkImage.Width));
                            ypos = (int)((originalImage.Height * (float).50) - (watermarkImage.Height / 2));
                            break;
                        case 7:
                            xpos = (int)(originalImage.Width * (float).01);
                            ypos = (int)((originalImage.Height * (float).99) - watermarkImage.Height);
                            break;
                        case 8:
                            xpos = (int)((originalImage.Width * (float).50) - (watermarkImage.Width / 2));
                            ypos = (int)((originalImage.Height * (float).99) - watermarkImage.Height);
                            break;
                        case 9:
                            xpos = (int)((originalImage.Width * (float).99) - (watermarkImage.Width));
                            ypos = (int)((originalImage.Height * (float).99) - watermarkImage.Height);
                            break;
                    }

                    // 绘制水印
                    originalImage.Mutate(o => { o.DrawImage(watermarkImage, new Point(xpos, ypos), opacity); });

                    // 返回字节数组
                    using (var ms = new MemoryStream())
                    {
                        originalImage.SaveImage(ms, imageFormat);
                        return ms.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// 文字水印
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="text">水印文字</param>
        /// <param name="location">图片水印位置 0=不使用 1=左上 2=中上 3=右上 4=左中  9=右下</param>
        /// <returns></returns>
        public byte[] LetterWatermark(byte[] byteData, string text, int location, FontOptions fontOptions)
        {
            Check.NotNull(fontOptions, nameof(fontOptions));
            // 设置字体大小与样式
            Font font;
            if (text.IsEnglish() || fontOptions.FontFamilyFilePath.IsNullOrWhiteSpace())
            {
                font = SystemFonts.CreateFont("Arial", fontOptions.FontSize, ConvertToFontStyle(fontOptions.FontStyle));
            }
            else
            {
                Check.NotNullOrWhiteSpace(fontOptions.FontFamilyFilePath, nameof(fontOptions.FontFamilyFilePath));
                // 装载字体文件
                var fontFamily = FontFamilyDict.GetOrAdd(fontOptions.FontFamilyFilePath, () =>
                {
                    var fonts = new FontCollection();
                    return fonts.Add(fontOptions.FontFamilyFilePath);
                });
                font = new Font(fontFamily, fontOptions.FontSize, ConvertToFontStyle(fontOptions.FontStyle));
            }
            // 获取文本绘制所需大小
            var size = TextMeasurer.Measure(text, new TextOptions(font));
            // 装载图片
            using (var originalImage = Image.Load(byteData, out IImageFormat imageFormat))
            {
                // 计算位置
                float xpos = 0;
                float ypos = 0;
                switch (location)
                {
                    case 1:
                        xpos = (float)originalImage.Width * (float).01;
                        ypos = (float)originalImage.Height * (float).01;
                        break;
                    case 2:
                        xpos = ((float)originalImage.Width * (float).50) - (size.Width / 2);
                        ypos = (float)originalImage.Height * (float).01;
                        break;
                    case 3:
                        xpos = ((float)originalImage.Width * (float).99) - size.Width;
                        ypos = (float)originalImage.Height * (float).01;
                        break;
                    case 4:
                        xpos = (float)originalImage.Width * (float).01;
                        ypos = ((float)originalImage.Height * (float).50) - (size.Height / 2);
                        break;
                    case 5:
                        xpos = ((float)originalImage.Width * (float).50) - (size.Width / 2);
                        ypos = ((float)originalImage.Height * (float).50) - (size.Height / 2);
                        break;
                    case 6:
                        xpos = ((float)originalImage.Width * (float).99) - size.Width;
                        ypos = ((float)originalImage.Height * (float).50) - (size.Height / 2);
                        break;
                    case 7:
                        xpos = (float)originalImage.Width * (float).01;
                        ypos = ((float)originalImage.Height * (float).99) - size.Height;
                        break;
                    case 8:
                        xpos = ((float)originalImage.Width * (float).50) - (size.Width / 2);
                        ypos = ((float)originalImage.Height * (float).99) - size.Height;
                        break;
                    case 9:
                        xpos = ((float)originalImage.Width * (float).99) - size.Width;
                        ypos = ((float)originalImage.Height * (float).99) - size.Height;
                        break;
                }

                // 绘制文字水印
                originalImage.Mutate(o => { o.DrawText(text, font, Rgba32.ParseHex(fontOptions.FontColor ?? "#000000"), new PointF(xpos, ypos)); });

                // 返回字节数组
                using (var ms = new MemoryStream())
                {
                    originalImage.SaveImage(ms, imageFormat);
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 图片矩形域裁剪
        /// </summary>
        /// <param name="byteData"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public byte[] Crop(byte[] byteData, int X, int Y, int width)
        {
            using (var originalImage = Image.Load(byteData, out IImageFormat imageFormat))
            {
                originalImage.Mutate(m => { m.Crop(new Rectangle(X, Y, width, width)); });
                using (var ms = new MemoryStream())
                {
                    originalImage.SaveImage(ms, imageFormat);
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 图片矩形域裁剪
        /// </summary>
        /// <param name="byteData"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public byte[] Crop(byte[] byteData, int X, int Y, int width, int height)
        {
            using (var originalImage = Image.Load(byteData, out IImageFormat imageFormat))
            {
                originalImage.Mutate(m => { m.Crop(new Rectangle(X, Y, width, height)); });
                using (var ms = new MemoryStream())
                {
                    originalImage.SaveImage(ms, imageFormat);
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 图片压缩
        /// </summary>
        /// <param name="byteData"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public byte[] Compress(byte[] byteData, float quality = 0.8f)
        {
            using (var originalImage = Image.Load(byteData, out IImageFormat imageFormat))
            {
                if (imageFormat is JpegFormat)
                {
                    using (var ms = new MemoryStream())
                    {
                        originalImage.SaveAsJpeg(ms, new JpegEncoder { Quality = (quality * 100).To<int>() });
                        return ms.ToArray();
                    }
                }
                return byteData;
            }
        }

        public byte[] Gray(byte[] byteData)
        {
            using (var originalImage = Image.Load<Rgba32>(byteData, out IImageFormat imageFormat))
            {
                originalImage.ToGray();

                using (var ms = new MemoryStream())
                {
                    originalImage.SaveImage(ms, imageFormat);
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 图片旋转
        /// </summary>
        /// <param name="byteData"></param>
        /// <param name="degre"></param>
        /// <returns></returns>
        public byte[] Rotate(byte[] byteData, float degre)
        {
            using (var originalImage = Image.Load(byteData, out IImageFormat imageFormat))
            {
                originalImage.Mutate(m => { m.Rotate(degre); });

                using (var ms = new MemoryStream())
                {
                    originalImage.SaveImage(ms, imageFormat);
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// 图片水平翻转
        /// </summary>
        /// <param name="byteData"></param>
        /// <returns></returns>
        public byte[] Flip(byte[] byteData)
        {
            using (var originalImage = Image.Load(byteData, out IImageFormat imageFormat))
            {
                originalImage.Mutate(m => { m.Flip(FlipMode.Horizontal); });

                using (var ms = new MemoryStream())
                {
                    originalImage.SaveImage(ms, imageFormat);
                    return ms.ToArray();
                }
            }
        }
    }
}
