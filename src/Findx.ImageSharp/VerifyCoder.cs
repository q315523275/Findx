using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace Findx.ImageSharp
{
    public class VerifyCoder
    {

        private static readonly string[] _colorHexArr = new string[] { "#00E5EE", "#000000", "#2F4F4F", "#000000", "#43CD80", "#191970", "#006400", "#458B00", "#8B7765", "#CD5B45" };
        private static readonly string[] _lightColorHexArr = new string[] { "#FFFACD", "#FDF5E6", "#F0FFFF", "#BBFFFF", "#FAFAD2", "#FFE4E1", "#DCDCDC", "#F0E68C" };
        private static readonly Random _random = new Random();


        public byte[] CreateImage(string code, int width, int height)
        {
            Image<Rgba32> img = new Image<Rgba32>(width, height);
            var colorTextHex = _colorHexArr[_random.Next(0, _colorHexArr.Length)];
            var lignthColorHex = _lightColorHexArr[_random.Next(0, _lightColorHexArr.Length)];

            img.Mutate(ctx => ctx
                        .Fill(Rgba32.FromHex(_lightColorHexArr[_random.Next(0, _lightColorHexArr.Length)]))
                        .Glow(Rgba32.FromHex(lignthColorHex))
                        .DrawingGrid(width, height, Rgba32.FromHex(lignthColorHex), 8, 1)
                        .DrawingEnText(width, height, text, _colorHexArr, _fontArr)
                        .GaussianBlur(0.4f)
                        .DrawingCircles(width, height, 15, _miniCircleR, _maxCircleR, Rgba32.White)
                    );

            return null;
        }
    }
}
