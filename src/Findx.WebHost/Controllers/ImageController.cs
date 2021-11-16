using Findx.Drawing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Findx.WebHost.Controllers
{
    public class ImageController: Controller
    {
        /// <summary>
        /// 图像处理
        /// </summary>
        /// <param name="imageProcessor"></param>
        /// <returns></returns>
        [HttpGet("/image/makeThumbnail")]
        public IActionResult ImageSharp([FromServices] IImageProcessor imageProcessor, [FromServices] IApplicationInstanceInfo applicationInstance, string filePath, int width, int height, int quality = 100, ImageResizeMode mode = ImageResizeMode.Crop)
        {
            var img = imageProcessor.MakeThumbnail(System.IO.File.ReadAllBytes(applicationInstance.MapPath(filePath)), "jpg", width, height, quality, mode);

            return File(img, "image/jpeg");
        }

        /// <summary>
        /// 图像处理
        /// </summary>
        /// <param name="imageProcessor"></param>
        /// <returns></returns>
        [HttpGet("/image/imageWatermark")]
        public IActionResult ImageWatermark([FromServices] IImageProcessor imageProcessor, [FromServices] IApplicationInstanceInfo applicationInstance, string filePath, string filePath2, int location, float opacity)
        {
            var img = imageProcessor.ImageWatermark(System.IO.File.ReadAllBytes(applicationInstance.MapPath(filePath)), "jpg", applicationInstance.MapPath(filePath2), location, opacity);

            return File(img, "image/jpeg");
        }

        /// <summary>
        /// 图像处理
        /// </summary>
        /// <param name="imageProcessor"></param>
        /// <returns></returns>
        [HttpGet("/image/letterWatermark")]
        public IActionResult LetterWatermark([FromServices] IImageProcessor imageProcessor, [FromServices] IApplicationInstanceInfo applicationInstance, string filePath, string text, int location, int fontSize, string fontPath)
        {
            var img = imageProcessor.LetterWatermark(System.IO.File.ReadAllBytes(applicationInstance.MapPath(filePath)), "jpg", text, location, fontPath, fontSize);

            return File(img, "image/jpeg");
        }

        /// <summary>
        /// 图片验证码
        /// </summary>
        /// <param name="verifyCoder"></param>
        /// <returns></returns>
        [HttpGet("/verifyCode")]
        public async Task<IActionResult> VerifyCode([FromServices] IVerifyCoder verifyCoder, int width = 80, int height = 35)
        {
            var code = verifyCoder.GetCode(4, VerifyCodeType.NumberAndLetter);

            var img = await verifyCoder.CreateImageAsync(code, width: width, height: height);

            return File(img, "image/jpeg");
        }
    }
}
