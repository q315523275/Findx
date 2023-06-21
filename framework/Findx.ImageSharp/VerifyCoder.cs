using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Imaging;
using Findx.Utils;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Findx.ImageSharp;

/// <summary>
///     验证码实现
/// </summary>
public class VerifyCoder : IVerifyCoder
{
    private static readonly string[] ColorHexArr =
    {
        "#00E5EE", "#000000", "#2F4F4F", "#000000", "#43CD80", "#191970", "#006400", "#458B00", "#8B7765", "#CD5B45"
    };

    private static readonly string[] LightColorHexArr =
        { "#FFFACD", "#FDF5E6", "#F0FFFF", "#BBFFFF", "#FAFAD2", "#FFE4E1", "#DCDCDC", "#F0E68C" };

    private static Font[] _fontArr;

    public VerifyCoder()
    {
        InitFonts(50);
    }

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="text"></param>
    /// <param name="imageWidth"></param>
    /// <param name="imageHeight"></param>
    /// <returns></returns>
    public async Task<byte[]> CreateImageAsync(string text, int imageWidth = 120, int imageHeight = 50)
    {
        // 图片太小容易导致
        using var imgText = new Image<Rgba32>(imageWidth, imageHeight);

        var lightColorHex = LightColorHexArr[RandomUtil.RandomInt(0, LightColorHexArr.Length)];

        imgText.Mutate(ctx => ctx.Fill(Rgba32.ParseHex(LightColorHexArr[RandomUtil.RandomInt(0, LightColorHexArr.Length)])));
        imgText.Mutate(ctx => ctx.Glow(Rgba32.ParseHex(lightColorHex)));
        imgText.Mutate(ctx => ctx.DrawingEnText(imageWidth, imageHeight, text, ColorHexArr, _fontArr));
        imgText.Mutate(ctx => ctx.GaussianBlur(0.4f));
        using var ms = Pool.MemoryStream.Rent();
        await imgText.SaveAsJpegAsync(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// 获取验证码字符串
    /// </summary>
    /// <param name="length"></param>
    /// <param name="codeType"></param>
    /// <returns></returns>
    public string GetCode(int length, VerifyCodeType codeType)
    {
        switch (codeType)
        {
            case VerifyCodeType.Number:
                return GetRandomNums(length);
            default:
                return GetRandomNumsAndLetters(length);
        }
    }

    /// <summary>
    ///     初始化字体池
    /// </summary>
    /// <param name="fontSize">一个初始大小</param>
    private static void InitFonts(int fontSize)
    {
        if (_fontArr == null)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();

            if (names.Length > 0)
            {
                var fontList = new List<Font>();
                var fontCollection = new FontCollection();

                foreach (var name in names)
                    fontList.Add(new Font(
                        fontCollection.Add(assembly.GetManifestResourceStream(name) ??
                                           throw new InvalidOperationException()), fontSize));

                _fontArr = fontList.ToArray();
            }
            else
            {
                throw new Exception("绘制验证码字体文件加载失败");
            }
        }
    }

    /// <summary>
    /// 获取数字随机码
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    private static string GetRandomNums(int length)
    {
        var ints = new int[length];
        for (var i = 0; i < length; i++) ints[i] = RandomUtil.RandomInt(0, 9);
        return ints.ExpandAndToString("");
    }

    /// <summary>
    /// 获取字母+数字随机码
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    private static string GetRandomNumsAndLetters(int length)
    {
        const string allChar = "2,3,4,5,6,7,8,9," +
                               "A,B,C,D,E,F,G,H,J,K,M,N,P,Q,R,S,T,U,V,W,X,Y,Z," +
                               "a,b,c,d,e,f,g,h,k,m,n,p,q,r,s,t,u,v,w,x,y,z";
        var allChars = allChar.Split(',');
        var result = new List<string>();
        while (result.Count < length)
        {
            var index = RandomUtil.RandomInt(allChars.Length);
            var c = allChars[index];
            result.Add(c);
        }

        return result.ExpandAndToString("");
    }
}