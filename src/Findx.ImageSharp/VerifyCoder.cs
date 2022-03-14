using Findx.Drawing;
using Findx.Extensions;
using Findx.Utils;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Findx.ImageSharp
{
    /// <summary>
    /// 验证码实现
    /// 
    /// </summary>
    public class VerifyCoder : IVerifyCoder
    {
        private static readonly string[] _colorHexArr = new string[] { "#00E5EE", "#000000", "#2F4F4F", "#000000", "#43CD80", "#191970", "#006400", "#458B00", "#8B7765", "#CD5B45" };
        private static readonly string[] _lightColorHexArr = new string[] { "#FFFACD", "#FDF5E6", "#F0FFFF", "#BBFFFF", "#FAFAD2", "#FFE4E1", "#DCDCDC", "#F0E68C" };
        private static Font[] _fontArr;

        public VerifyCoder() => initFonts(50);

        /// <summary>
        /// 初始化字体池
        /// </summary>
        /// <param name="fontSize">一个初始大小</param>
        private void initFonts(int fontSize)
        {
            if (_fontArr == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var names = assembly.GetManifestResourceNames();

                if (names?.Length > 0 == true)
                {
                    var fontList = new List<Font>();
                    var fontCollection = new FontCollection();

                    foreach (var name in names)
                    {
                        fontList.Add(new Font(fontCollection.Install(assembly.GetManifestResourceStream(name)), fontSize));
                    }

                    _fontArr = fontList.ToArray();
                }
                else
                {
                    throw new Exception($"绘制验证码字体文件加载失败");
                }
            }
        }

        public async Task<byte[]> CreateImageAsync(string text, int imageWidth = 120, int imageHeight = 50)
        {
            // 图片太小容易导致
            byte[] result;

            using (var imgText = new Image<Rgba32>(imageWidth, imageHeight))
            {
                var colorTextHex = _colorHexArr[RandomUtil.RandomInt(0, _colorHexArr.Length)];
                var lignthColorHex = _lightColorHexArr[RandomUtil.RandomInt(0, _lightColorHexArr.Length)];

                imgText.Mutate(ctx => ctx.Fill(Rgba32.ParseHex(_lightColorHexArr[RandomUtil.RandomInt(0, _lightColorHexArr.Length)])));

                imgText.Mutate(ctx => ctx.Glow(Rgba32.ParseHex(lignthColorHex)));

                imgText.Mutate(ctx => ctx.DrawingEnText(imageWidth, imageHeight, text, _colorHexArr, _fontArr));

                imgText.Mutate(ctx => ctx.GaussianBlur(0.4f));

                using (var ms = new MemoryStream())
                {
                    await imgText.SaveAsJpegAsync(ms);
                    result = ms.ToArray();
                }
            }

            return result;
        }

        public string GetCode(int length, VerifyCodeType codeTyoe)
        {
            switch (codeTyoe)
            {
                case VerifyCodeType.Number:
                    return GetRandomNums(length);
                default:
                    return GetRandomNumsAndLetters(length);
            }
        }

        private static string GetRandomNums(int length)
        {
            int[] ints = new int[length];
            for (int i = 0; i < length; i++)
            {
                ints[i] = RandomUtil.RandomInt(0, 9);
            }
            return ints.ExpandAndToString("");
        }

        private static string GetRandomNumsAndLetters(int length)
        {
            const string allChar = "2,3,4,5,6,7,8,9," +
                "A,B,C,D,E,F,G,H,J,K,M,N,P,Q,R,S,T,U,V,W,X,Y,Z," +
                "a,b,c,d,e,f,g,h,k,m,n,p,q,r,s,t,u,v,w,x,y,z";
            string[] allChars = allChar.Split(',');
            List<string> result = new List<string>();
            while (result.Count < length)
            {
                int index = RandomUtil.RandomInt(allChars.Length);
                string c = allChars[index];
                result.Add(c);
            }
            return result.ExpandAndToString("");
        }
    }
}
