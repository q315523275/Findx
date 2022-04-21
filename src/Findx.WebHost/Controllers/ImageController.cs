using Findx.Drawing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Findx.WebHost.Controllers
{
    /// <summary>
    /// 图片
    /// </summary>
    public class ImageController : Controller
    {
        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="imageProcessor"></param>
        /// <param name="applicationInstance"></param>
        /// <param name="filePath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [HttpGet("/image/scale")]
        public IActionResult Scale([FromServices] IImageProcessor imageProcessor, [FromServices] IApplicationContext applicationInstance, string filePath, int width, int height, ImageResizeMode mode = ImageResizeMode.Crop)
        {
            var img = imageProcessor.Scale(System.IO.File.ReadAllBytes(applicationInstance.MapPath(filePath)), width, height, mode);

            return File(img, "image/jpeg");
        }

        /// <summary>
        /// 裁剪
        /// </summary>
        /// <param name="imageProcessor"></param>
        /// <param name="applicationInstance"></param>
        /// <param name="filePath"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        [HttpGet("/image/crop")]
        public IActionResult Crop([FromServices] IImageProcessor imageProcessor, [FromServices] IApplicationContext applicationInstance, string filePath, int x, int y, int width)
        {
            var img = imageProcessor.Crop(System.IO.File.ReadAllBytes(applicationInstance.MapPath(filePath)), x, y, width);

            return File(img, "image/jpeg");
        }

        /// <summary>
        /// 压缩
        /// </summary>
        /// <param name="imageProcessor"></param>
        /// <param name="applicationInstance"></param>
        /// <param name="filePath"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        [HttpGet("/image/compress")]
        public IActionResult Compress([FromServices] IImageProcessor imageProcessor, [FromServices] IApplicationContext applicationInstance, string filePath, float quality = 0.8f)
        {
            var img = imageProcessor.Compress(System.IO.File.ReadAllBytes(applicationInstance.MapPath(filePath)), quality);

            return File(img, "image/jpeg");
        }

        /// <summary>
        /// 黑白
        /// </summary>
        /// <param name="imageProcessor"></param>
        /// <param name="applicationInstance"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [HttpGet("/image/gray")]
        public IActionResult Gray([FromServices] IImageProcessor imageProcessor, [FromServices] IApplicationContext applicationInstance, string filePath)
        {
            var img = imageProcessor.Gray(System.IO.File.ReadAllBytes(applicationInstance.MapPath(filePath)));

            return File(img, "image/jpeg");
        }

        /// <summary>
        /// 旋转
        /// </summary>
        /// <param name="imageProcessor"></param>
        /// <param name="applicationInstance"></param>
        /// <param name="filePath"></param>
        /// <param name="degre"></param>
        /// <returns></returns>
        [HttpGet("/image/rotate")]
        public IActionResult Rotate([FromServices] IImageProcessor imageProcessor, [FromServices] IApplicationContext applicationInstance, string filePath, float degre)
        {
            var img = imageProcessor.Rotate(System.IO.File.ReadAllBytes(applicationInstance.MapPath(filePath)), degre);

            return File(img, "image/jpeg");
        }

        /// <summary>
        /// 水平翻转
        /// </summary>
        /// <param name="imageProcessor"></param>
        /// <param name="applicationInstance"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [HttpGet("/image/flip")]
        public IActionResult Flip([FromServices] IImageProcessor imageProcessor, [FromServices] IApplicationContext applicationInstance, string filePath)
        {
            var img = imageProcessor.Flip(System.IO.File.ReadAllBytes(applicationInstance.MapPath(filePath)));

            return File(img, "image/jpeg");
        }

        /// <summary>
        /// 添加图片
        /// </summary>
        /// <param name="imageProcessor"></param>
        /// <param name="applicationInstance"></param>
        /// <param name="filePath"></param>
        /// <param name="filePath2"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="opacity"></param>
        /// <returns></returns>
        [HttpGet("/image/pressImage")]
        public IActionResult PressImage([FromServices] IImageProcessor imageProcessor, [FromServices] IApplicationContext applicationInstance, string filePath, string filePath2, int x, int y, float opacity = 0.8f)
        {
            var img = imageProcessor.PressImage(System.IO.File.ReadAllBytes(applicationInstance.MapPath(filePath)), applicationInstance.MapPath(filePath2), x, y, opacity: opacity);

            return File(img, "image/jpeg");
        }

        /// <summary>
        /// 添加文字
        /// </summary>
        /// <param name="imageProcessor"></param>
        /// <param name="applicationInstance"></param>
        /// <param name="filePath"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="fontPath"></param>
        /// <param name="fontColor"></param>
        /// <param name="fontStyle"></param>
        /// <returns></returns>
        [HttpGet("/image/pressText")]
        public IActionResult PressText([FromServices] IImageProcessor imageProcessor, [FromServices] IApplicationContext applicationInstance, string filePath, int x, int y, string text, int fontSize, string fontPath, string fontColor, FontStyle fontStyle)
        {
            var img = imageProcessor.PressText(System.IO.File.ReadAllBytes(applicationInstance.MapPath(filePath)), x, y, text, new FontOptions { FontColor = fontColor, FontFamilyFilePath = fontPath, FontSize = fontSize, FontStyle = fontStyle });

            return File(img, "image/jpeg");
        }

        /// <summary>
        /// 图片验证码
        /// </summary>
        /// <param name="verifyCoder"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        [HttpGet("/verifyCode")]
        public async Task<IActionResult> VerifyCode([FromServices] IVerifyCoder verifyCoder, int width = 150, int height = 50)
        {
            var code = verifyCoder.GetCode(4, VerifyCodeType.NumberAndLetter);

            var img = await verifyCoder.CreateImageAsync(code, width: width, height: height);

            return File(img, "image/jpeg");
        }
    }
}
