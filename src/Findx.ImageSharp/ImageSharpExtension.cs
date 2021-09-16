using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Findx.ImageSharp
{
    public static class ImageSharpExtension
    {
        public static IImageProcessingContext DrawingEnText(this IImageProcessingContext processingContext, int containerWidth, int containerHeight, string text, string[] colorHexArr, Font[] fonts)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Random random = new Random();
                var textWidth = (containerWidth / text.Length);
                var img2Size = Math.Min(textWidth, containerHeight);
                var fontMiniSize = (int)(img2Size * 0.9);
                var fontMaxSize = (int)(img2Size * 1.37);
                Array fontStyleArr = Enum.GetValues(typeof(FontStyle));

                for (int i = 0; i < text.Length; i++)
                {
                    using (Image<Rgba32> img2 = new Image<Rgba32>(img2Size, img2Size))
                    {
                        Font scaledFont = new Font(fonts[random.Next(0, fonts.Length)], random.Next(fontMiniSize, fontMaxSize), (FontStyle)fontStyleArr.GetValue(random.Next(fontStyleArr.Length)));
                        var point = new Point(i * textWidth, (containerHeight - img2.Height) / 2);
                        var colorHex = colorHexArr[random.Next(0, colorHexArr.Length)];
                        var drawingOptions = new DrawingOptions { TextOptions = new TextOptions { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top } };

                        img2.Mutate(ctx => ctx
                                        .DrawText(drawingOptions, text[i].ToString(), scaledFont, Rgba32.ParseHex(colorHex), new Point(0, 0))
                                        .DrawingGrid(containerWidth, containerHeight, Rgba32.ParseHex(colorHex), 6, 1)
                                        .Rotate(random.Next(-45, 45))
                                    );
                        processingContext.DrawImage(img2, point, 1);
                    }
                }
            }

            return processingContext;
        }

        /// <summary>
        /// 画杂线
        /// </summary>
        /// <typeparam name="TPixel"></typeparam>
        /// <param name="processingContext"></param>
        /// <param name="containerWidth"></param>
        /// <param name="containerHeight"></param>
        /// <param name="color"></param>
        /// <param name="count"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
        public static IImageProcessingContext DrawingGrid(this IImageProcessingContext processingContext, int containerWidth, int containerHeight, Color color, int count, float thickness)
        {

            var points = new List<PointF> { new PointF(0, 0) };
            for (int i = 0; i < count; i++)
            {
                GetCirclePoginF(containerWidth, containerHeight, 9, ref points);
            }
            points.Add(new PointF(containerWidth, containerHeight));

            processingContext.DrawLines(color, thickness, points.ToArray());

            return processingContext;
        }
        
        /// <summary>
        /// 散 随机点
        /// </summary>
        /// <param name="containerWidth"></param>
        /// <param name="containerHeight"></param>
        /// <param name="lapR"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private static PointF GetCirclePoginF(int containerWidth, int containerHeight, double lapR, ref List<PointF> list)
        {
            Random random = new Random();
            PointF newPoint = new PointF();
            int retryTimes = 10;
            double tempDistance = 0;

            do
            {
                newPoint.X = random.Next(0, containerWidth);
                newPoint.Y = random.Next(0, containerHeight);
                bool tooClose = false;
                foreach (var p in list)
                {
                    tooClose = false;
                    tempDistance = Math.Sqrt((Math.Pow((p.X - newPoint.X), 2) + Math.Pow((p.Y - newPoint.Y), 2)));
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
            }
            while (retryTimes-- > 0);

            if (retryTimes <= 0)
            {
                list.Add(newPoint);
            }
            return newPoint;
        }
    }
}
