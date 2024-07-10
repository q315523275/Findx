using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Imaging;
using Findx.Utilities;
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
    private static readonly string[] ColorHexArr = { "#0087FF", "#339933", "#FF6666", "#FF9900", "#996600", "#996699", "#339999", "#6666FF", "#0066CC", "#CC3333", "#0099CC", "#003366" };

    private static readonly string[] LightColorHexArr = { "#FFFACD", "#FDF5E6", "#F0FFFF", "#BBFFFF", "#FAFAD2", "#FFE4E1", "#DCDCDC", "#F0E68C" };

    private readonly List<FontFamily> _fontFamilies;

    private readonly IFontFamilyProvider _fontFamilyProvider;
    
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="fontFamilyProvider"></param>
    public VerifyCoder(IFontFamilyProvider fontFamilyProvider)
    {
        _fontFamilies = new List<FontFamily>();
        _fontFamilyProvider = fontFamilyProvider;
        InitFonts();
    }


    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="text"></param>
    /// <param name="imageWidth"></param>
    /// <param name="imageHeight"></param>
    /// <param name="fontSize"></param>
    /// <returns></returns>
    public async Task<byte[]> CreateImageAsync(string text, int imageWidth = 120, int imageHeight = 50, int fontSize = 32)
    {
        // 图片太小容易导致
        using var imgText = new Image<Rgba32>(imageWidth, imageHeight);

        // 增加背景色与中心渐变,普通集成ocr无法直接识别
        imgText.Mutate(ctx => ctx.Fill(Rgba32.ParseHex(LightColorHexArr[RandomUtility.RandomInt(0, LightColorHexArr.Length)])));
        imgText.Mutate(ctx => ctx.Glow(Rgba32.ParseHex(LightColorHexArr[RandomUtility.RandomInt(0, LightColorHexArr.Length)])));
        
        // 绘制验证码
        imgText.Mutate(ctx => ctx.DrawingEnText(imageWidth, imageHeight, text, ColorHexArr, fontSize, _fontFamilies));
        // imgText.Mutate(ctx => ctx.GaussianBlur());

        // 读取流
        using var ms = Pool.MemoryStream.Rent();
        await imgText.SaveAsPngAsync(ms);
        return ms.ToArray();
    }

    public async Task<Stream> CreateImageStreamAsync(string text, int width = 120, int height = 50, int fontSize = 32)
    {
        // 图片太小容易导致
        using var imgText = new Image<Rgba32>(width, height);

        // 增加背景色与中心渐变,普通集成ocr无法直接识别
        var lightColorHex = LightColorHexArr[RandomUtility.RandomInt(0, LightColorHexArr.Length)];
        imgText.Mutate(ctx => ctx.Fill(Rgba32.ParseHex(LightColorHexArr[RandomUtility.RandomInt(0, LightColorHexArr.Length)])));
        imgText.Mutate(ctx => ctx.Glow(Rgba32.ParseHex(lightColorHex)));

        // 绘制验证码
        imgText.Mutate(ctx => ctx.DrawingEnText(width, height, text, ColorHexArr, fontSize, _fontFamilies));
        // imgText.Mutate(ctx => ctx.GaussianBlur(0.4f));
        
        // 读取流
        var ms = Pool.MemoryStream.Rent();
        await imgText.SaveAsPngAsync(ms);
        return ms;
    }

    public Task SetFontAsync(string fontFamilyPath, CancellationToken cancellationToken = default)
    {
        var fontFamily = _fontFamilyProvider.Create(fontFamilyPath);
        _fontFamilies.Add(fontFamily);
        return Task.CompletedTask;
    }
    
    public Task SetFontAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var fontFamily = _fontFamilyProvider.Create(stream);
        _fontFamilies.Add(fontFamily);
        return Task.CompletedTask;
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
    private void InitFonts()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var names = assembly.GetManifestResourceNames();

        if (names.Length > 0)
        {
            foreach (var name in names)
            {
                using var stream = assembly.GetManifestResourceStream(name);
                var fontFamily = _fontFamilyProvider.Create(stream);
                _fontFamilies.Add(fontFamily);
            }
        }
        else
        {
            throw new Exception("绘制验证码字体文件加载失败");
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
        for (var i = 0; i < length; i++) ints[i] = RandomUtility.RandomInt(0, 9);
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
            var index = RandomUtility.RandomInt(allChars.Length);
            var c = allChars[index];
            result.Add(c);
        }

        return result.ExpandAndToString("");
    }
}