using Findx.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using Findx.Extensions;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Findx.ImageSharp
{
    public class ImageProcessor : IImageProcessor
    {
        /// <summary>
        /// 字体缓存
        /// </summary>
        private readonly static IDictionary<string, FontFamily> FontFamilyDict = new ConcurrentDictionary<string, FontFamily>();

        /// <summary>
        /// 获取图片格式
        /// </summary>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        private IImageFormat GetFormat(string fileExt)
        {
            var ext = fileExt.SafeString().TrimStart('.');
            switch (ext.ToLower())
            {
                case "bmp":
                    return BmpFormat.Instance;
                case "png":
                    return PngFormat.Instance;
                case "gif":
                    return GifFormat.Instance;
                default:
                    return JpegFormat.Instance;
            }
        }

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

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="byteData">文件字节数组</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式(默认填充)</param>
        /// <returns></returns>
        public byte[] MakeThumbnail(byte[] byteData, string fileExt, int width, int height, ImageResizeMode mode = ImageResizeMode.Pad)
        {
            var originalImage = Image.Load(byteData);

            var option = new ResizeOptions { Size = new Size(width, height), Mode = GetResizeMode(mode) };

            originalImage.Mutate(m => { m.Resize(option); });

            // 返回字节数组
            using (var ms = new MemoryStream())
            {
                originalImage.Save(ms, GetFormat(fileExt));

                return ms.ToArray();
            }
        }

        /// <summary>
        /// 图片水印
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="fileExt">图片扩展名</param>
        /// <param name="imageWatermarkPath">水印文件物理路径</param>
        /// <param name="location">图片水印位置 0=不使用 1=左上 2=中上 3=右上 4=左中  9=右下</param>
        /// <param name="opacity">水印的透明度</param>
        /// <returns></returns>
        public byte[] ImageWatermark(byte[] byteData, string fileExt, string imageWatermarkPath, int location, float opacity)
        {
            // 装载原图
            var originalImage = Image.Load(byteData);
            // 装载水印图片
            var watermarkImage = Image.Load(imageWatermarkPath);
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
                originalImage.Save(ms, GetFormat(fileExt));

                return ms.ToArray();
            }
        }

        /// <summary>
        /// 文字水印
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="fileExt">图片扩展名</param>
        /// <param name="text">水印文字</param>
        /// <param name="location">图片水印位置 0=不使用 1=左上 2=中上 3=右上 4=左中  9=右下</param>
        /// <param name="fontName">字体</param>
        /// <param name="fontSize">字体大小</param>
        /// <returns></returns>
        public byte[] LetterWatermark(byte[] byteData, string fileExt, string text, int location, string fontFamilyPath, int fontSize)
        {
            // 设置字体大小与样式
            Font font;
            if (text.IsEnglish() || fontFamilyPath.IsNullOrWhiteSpace())
            {
                font = SystemFonts.CreateFont("Arial", fontSize, FontStyle.Regular);
            }
            else
            {
                Check.NotNullOrWhiteSpace(fontFamilyPath, nameof(fontFamilyPath));
                // 装载字体文件
                var fontFamily = FontFamilyDict.GetOrAdd(fontFamilyPath, () =>
                {
                    var fonts = new FontCollection();
                    return fonts.Install(fontFamilyPath);
                });
                font = new Font(fontFamily, fontSize, FontStyle.Regular);
            }
            // 获取文本绘制所需大小
            var size = TextMeasurer.Measure(text, new RendererOptions(font));
            // 装载图片
            var originalImage = Image.Load(byteData);
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
            originalImage.Mutate(o => { o.DrawText(text, font, Color.Black, new PointF(xpos, ypos)); });

            // 返回字节数组
            using (var ms = new MemoryStream())
            {
                originalImage.Save(ms, GetFormat(fileExt));

                return ms.ToArray();
            }
        }

        /// <summary>
        /// 合并图片
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="fileExt">图片扩展名</param>
        /// <param name="mergeImagePath">合并图片物理路径</param>
        /// <param name="X">X轴</param>
        /// <param name="Y">Y轴</param>
        /// <param name="width">合并图片宽度,0不处理</param>
        /// <param name="height">合并图片高度,0不处理</param>
        /// <returns></returns>
        public byte[] MergeImage(byte[] byteData, string fileExt, string mergeImagePath, int X, int Y, int width = 0, int height = 0)
        {
            var mergeImage = Image.Load(mergeImagePath);

            if (width > 0 && height > 0)
            {
                mergeImage.Mutate(m => { m.Resize(new Size(width, height)); });
            }

            var originalImage = Image.Load(byteData);

            originalImage.Mutate(o => { o.DrawImage(mergeImage, new Point(X, Y), 1); });

            // 返回字节数组
            using (var ms = new MemoryStream())
            {
                originalImage.Save(ms, GetFormat(fileExt));

                return ms.ToArray();
            }
        }

        /// <summary>
        /// 绘制多行文本
        /// </summary>
        /// <param name="byteData"></param>
        /// <param name="fileExt"></param>
        /// <param name="text"></param>
        /// <param name="fontFamilyPath">字体资源路径</param>
        /// <param name="fontSize"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public byte[] MultilineText(byte[] byteData, string fileExt, string text, string fontFamilyPath, int fontSize, int X, int Y)
        {
            // 设置字体大小与样式
            Font font;
            if (text.IsEnglish() || fontFamilyPath.IsNullOrWhiteSpace())
            {
                font = SystemFonts.CreateFont("Arial", fontSize, FontStyle.Regular);
            }
            else
            {
                Check.NotNullOrWhiteSpace(fontFamilyPath, nameof(fontFamilyPath));
                // 装载字体文件
                var fontFamily = FontFamilyDict.GetOrAdd(fontFamilyPath, () =>
                {
                    var fonts = new FontCollection();
                    return fonts.Install(fontFamilyPath);
                });
                font = new Font(fontFamily, fontSize, FontStyle.Regular);
            }
            // 获取文本绘制所需大小
            var size = TextMeasurer.Measure(text, new RendererOptions(font));
            // 装载图片
            var originalImage = Image.Load(byteData);
            // 绘制文字
            originalImage.Mutate(o => { o.DrawText(text, font, Color.Black, new PointF(X, Y)); });
            // 返回字节数组
            using (var ms = new MemoryStream())
            {
                originalImage.Save(ms, GetFormat(fileExt));

                return ms.ToArray();
            }
        }
    }
}
