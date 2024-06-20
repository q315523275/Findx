using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.Imaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
///     图片处理
/// </summary>
[Route("api/image")]
[Description("图片处理"), Tags("图片处理")]
public class ImageController : ApiControllerBase
{
    private readonly IImageProcessor _processor;
    private readonly IApplicationContext _app;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="processor"></param>
    /// <param name="app"></param>
    public ImageController(IImageProcessor processor, IApplicationContext app)
    {
        _processor = processor;
        _app = app;
    }

    private Task<byte[]> ReadAllBytesAsync(string filePath)
    {
        return System.IO.File.ReadAllBytesAsync(_app.MapPath(filePath));
    }

    /// <summary>
    ///     缩放
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    [HttpGet("resize")]
    public async Task<IActionResult> ResizeAsync(string filePath, int width, int height, ImageResizeMode mode = ImageResizeMode.Crop)
    {
        var imageByte = await _processor.ResizeAsync(await ReadAllBytesAsync(filePath), new ImageResizeDto(width, height, mode));
        return new FileContentResult(imageByte, "image/jpeg");
    }

    /// <summary>
    ///     裁剪
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    [HttpGet("crop")]
    public async Task<IActionResult> CropAsync(string filePath, int x, int y, int width, int height)
    {
        var imageByte = await _processor.CropAsync(await ReadAllBytesAsync(filePath), new ImageCropDto(x, y, width, height));
        return new FileContentResult(imageByte, "image/jpeg");
    }

    /// <summary>
    ///     压缩
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="quality"></param>
    /// <returns></returns>
    [HttpGet("compress")]
    public async Task<IActionResult> CompressAsync(string filePath, int quality = 75)
    {
        var imageByte = await _processor.CompressAsync(await ReadAllBytesAsync(filePath), quality);
        return new FileContentResult(imageByte, "image/jpeg");
    }

    /// <summary>
    ///     黑白
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    [HttpGet("gray")]
    public async Task<IActionResult> Gray(string filePath)
    {
        var imageByte = await _processor.GrayAsync(await ReadAllBytesAsync(filePath));
        return new FileContentResult(imageByte, "image/jpeg");
    }

    /// <summary>
    ///     旋转
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="degrees"></param>
    /// <returns></returns>
    [HttpGet("rotate")]
    public async Task<IActionResult> Rotate(string filePath, float degrees)
    {
        var imageByte = await _processor.RotateAsync(await ReadAllBytesAsync(filePath), degrees);
        return new FileContentResult(imageByte, "image/jpeg");
    }

    /// <summary>
    ///     水平翻转
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    [HttpGet("flip")]
    public async Task<IActionResult> Flip(string filePath, ImageFlipMode mode = ImageFlipMode.Horizontal)
    {
        var imageByte = await _processor.FlipAsync(await ReadAllBytesAsync(filePath), mode);
        return new FileContentResult(imageByte, "image/jpeg");
    }

    /// <summary>
    ///     添加图片
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="filePath2"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="mode"></param>
    /// <param name="opacity"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    [HttpGet("mergeImage")]
    public async Task<IActionResult> MergeImageAsync(string filePath, string filePath2, int x, int y, int width, int height, ImageResizeMode mode, float opacity = 0.75f)
    {
        var imageByte = await _processor.MergeImageAsync(await ReadAllBytesAsync(filePath), await ReadAllBytesAsync(filePath2), new MergeImageDto(x, y, width, height,resizeMode: mode, opacity: opacity));
        return new FileContentResult(imageByte, "image/jpeg");
    }

    /// <summary>
    ///     添加文字
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="text"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("pressText")]
    public async Task<IActionResult> PressText([Required] string filePath, [Required] string text, [FromBody] DrawTextDto dto)
    {
        var imageByte = await _processor.DrawTextAsync(await ReadAllBytesAsync(filePath), text, dto);
        return new FileContentResult(imageByte, "image/jpeg");
    }

    /// <summary>
    ///     图片验证码
    /// </summary>
    /// <param name="verifyCoder"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    [HttpGet("verifyCode")]
    public async Task<IActionResult> VerifyCode([FromServices] IVerifyCoder verifyCoder, int width = 150,
        int height = 50)
    {
        var code = verifyCoder.GetCode(4, VerifyCodeType.NumberAndLetter);

        var imageByte = await verifyCoder.CreateImageAsync(code, width, height);

        return new FileContentResult(imageByte, "image/jpeg");
    }
}