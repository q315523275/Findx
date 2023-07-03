using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Utils;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Findx.ImageSharp;

public static class ImageSharpExtension
{
    /// <summary>
    /// 绘画文字
    /// </summary>
    /// <param name="processingContext"></param>
    /// <param name="containerWidth"></param>
    /// <param name="containerHeight"></param>
    /// <param name="text"></param>
    /// <param name="colorHexArr"></param>
    /// <param name="fontSize"></param>
    /// <param name="fontFamilies"></param>
    /// <returns></returns>
    public static void DrawingEnText(this IImageProcessingContext processingContext, int containerWidth, int containerHeight, string text, string[] colorHexArr, int fontSize, IReadOnlyList<FontFamily> fontFamilies)
    {
        if (string.IsNullOrEmpty(text)) return;

        var textWidth = containerWidth / text.Length;
        var img2Size = Math.Min(textWidth, containerHeight);
        var fontMiniSize = (int)(fontSize * 0.9);
        var fontMaxSize = (int)(fontSize * 1.1);
        var fontStyleArr = Enum.GetValues(typeof(FontStyle));

        // 逐字绘制,方便控制
        for (var i = 0; i < text.Length; i++)
        {
            var fontFamily = fontFamilies.Count > 1 ? fontFamilies[RandomUtil.RandomInt(0, fontFamilies.Count)] : fontFamilies.First();
            var fontStyle = (FontStyle)fontStyleArr.GetValue(RandomUtil.RandomInt(fontStyleArr.Length));
            // 随机字体大小
            fontSize = RandomUtil.RandomInt(fontMiniSize, fontMaxSize);
            var scaledFont = fontFamily.CreateFont(fontSize, fontStyle);
            var point = new Point(i * textWidth, 0);
            var colorHex = colorHexArr[RandomUtil.RandomInt(0, colorHexArr.Length)];
            
            using Image<Rgba32> img2 = new(img2Size, containerHeight);
   
            // 文字尺寸、位置
            var size = TextMeasurer.Measure(text[i].ToString(), new TextOptions(scaledFont));
            var offsetLeft = ((img2.Width - size.Width) / 2).To<int>();
            var offsetTop = ((img2.Height - size.Height) / 2).To<int>();

            var i1 = i;
            img2.Mutate(ctx =>
                        ctx.DrawText(text[i1].ToString(), scaledFont, Rgba32.ParseHex(colorHex), new Point(offsetLeft, offsetTop))
                           .DrawingGrid(containerWidth, containerHeight, Rgba32.ParseHex(colorHex), 6, 1)
                           .Rotate(RandomUtil.RandomInt(-15, 15)) // 字体自带旋转，意思一下就行
            );

            processingContext.DrawImage(img2, point, 1);
        }
    }

    /// <summary>
    ///     画杂线
    /// </summary>
    /// <param name="processingContext"></param>
    /// <param name="containerWidth"></param>
    /// <param name="containerHeight"></param>
    /// <param name="color"></param>
    /// <param name="count"></param>
    /// <param name="thickness"></param>
    /// <returns></returns>
    public static IImageProcessingContext DrawingGrid(this IImageProcessingContext processingContext, int containerWidth, int containerHeight, Color color, int count, float thickness)
    {
        var points = new List<PointF> { new(0, 0) };
        for (var i = 0; i < count; i++) 
            GetCirclePointF(containerWidth, containerHeight, 9, ref points);
        points.Add(new PointF(containerWidth, containerHeight));

        processingContext.DrawLines(color, thickness, points.ToArray());

        return processingContext;
    }

    /// <summary>
    ///     散 随机点
    /// </summary>
    /// <param name="containerWidth"></param>
    /// <param name="containerHeight"></param>
    /// <param name="lapR"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    private static PointF GetCirclePointF(int containerWidth, int containerHeight, double lapR, ref List<PointF> list)
    {
        var random = new Random();
        var newPoint = new PointF();
        var retryTimes = 10;

        do
        {
            newPoint.X = random.Next(0, containerWidth);
            newPoint.Y = random.Next(0, containerHeight);
            var tooClose = false;
            foreach (var p in list)
            {
                tooClose = false;
                var tempDistance = Math.Sqrt(Math.Pow(p.X - newPoint.X, 2) + Math.Pow(p.Y - newPoint.Y, 2));
                if (tempDistance < lapR)
                {
                    tooClose = true;
                    break;
                }
            }

            if (tooClose == false)
            {
                list.Add(newPoint);
                break;
            }
            
        } while (retryTimes-- > 0);

        if (retryTimes <= 0) list.Add(newPoint);
        
        return newPoint;
    }
    

    /// <summary>
    ///     图像灰度处理
    /// </summary>
    /// <param name="img"></param>
    /// <returns></returns>
    public static Image<Rgba32> ToGray(this Image<Rgba32> img)
    {
        for (var i = 0; i < img.Width; i++)
        for (var j = 0; j < img.Height; j++)
        {
            var color = img[i, j];
            // 计算灰度值
            var gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
            var newColor = System.Drawing.Color.FromArgb(gray, gray, gray);
            // 修改该像素点的RGB的颜色
            img[i, j] = new Rgba32(newColor.R, newColor.G, newColor.B, newColor.A);
        }

        return img;
    }

    /// <summary>
    ///     黑白二值化
    /// </summary>
    /// <param name="image"></param>
    /// <param name="threshold">阈值</param>
    /// <returns></returns>
    public static void ToBinary(this Image<Rgba32> image, int threshold = 180)
    {
        image = image.ToGray(); // 先灰度处理
        for (var i = 0; i < image.Width; i++)
        for (var j = 0; j < image.Height; j++)
        {
            // 获取该像素点的RGB的颜色
            var color = image[i, j];
            // 计算颜色,大于平均值为黑,小于平均值为白
            var newColor = color.B < threshold ? System.Drawing.Color.FromArgb(0, 0, 0) : System.Drawing.Color.FromArgb(255, 255, 255);
            // 修改该像素点的RGB的颜色
            image[i, j] = new Rgba32(newColor.R, newColor.G, newColor.B, newColor.A);
        }
    }


    /// <summary>
    /// 获取图片处理后字节数组
    /// </summary>
    /// <param name="originalImage"></param>
    /// <param name="imageFormat"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<byte[]> SaveAndGetAllBytesAsync(this Image originalImage, IImageFormat imageFormat, CancellationToken cancellationToken = default)
    {
        using var memoryStream = Pool.MemoryStream.Rent();
        await originalImage.SaveAsync(memoryStream, imageFormat, cancellationToken: cancellationToken);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// 获取图片处理后数据流
    /// </summary>
    /// <param name="originalImage"></param>
    /// <param name="imageFormat"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Stream> SaveAndGetAllStreamAsync(this Image originalImage, IImageFormat imageFormat, CancellationToken cancellationToken = default)
    {
        var memoryStream = Pool.MemoryStream.Rent();
        await originalImage.SaveAsync(memoryStream, imageFormat, cancellationToken: cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }
}