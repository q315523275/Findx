namespace Findx.Drawing
{
    /// <summary>
    /// 图片处理器
    /// </summary>
    public interface IImageProcessor
    {
        /// <summary>
        /// 图片缩放
        /// </summary>
        /// <param name="byteData">文件字节数组</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式(默认填充)</param>
        /// <returns></returns>
        byte[] Scale(byte[] byteData, int width, int height, ImageResizeMode mode = ImageResizeMode.Pad);

        /// <summary>
        /// 图片裁剪
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="x">裁剪的矩形区域X坐标</param>
        /// <param name="y">裁剪的矩形区域Y坐标</param>
        /// <param name="width">裁剪的矩形区域长宽度</param>
        /// <returns></returns>
        byte[] Crop(byte[] byteData, int x, int y, int width);

        /// <summary>
        /// 图片裁剪
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="x">裁剪的矩形区域X坐标</param>
        /// <param name="y">裁剪的矩形区域Y坐标</param>
        /// <param name="width">裁剪的矩形区域宽度</param>
        /// <param name="height">裁剪的矩形区域高度</param>
        /// <returns></returns>
        byte[] Crop(byte[] byteData, int x, int y, int width, int height);

        /// <summary>
        /// 图片压缩(仅支持Jpg图片)
        /// </summary>
        /// <param name="byteData"></param>
        /// <param name="quality">压缩比率：alpha 必须是范围 [0.0, 1.0] 之内（包含边界值）的一个浮点数字</param>
        /// <returns></returns>
        byte[] Compress(byte[] byteData, float quality = 0.8f);

        /// <summary>
        /// 图片灰色化
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <returns></returns>
        byte[] Gray(byte[] byteData);

        /// <summary>
        /// 图片旋转图片
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="degre">旋转角度</param>
        /// <returns></returns>
        byte[] Rotate(byte[] byteData, float degre);

        /// <summary>
        /// 图片水平翻转图片
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <returns></returns>
        byte[] Flip(byte[] byteData);

        /// <summary>
        /// 图片添加图片
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="mergeImagePath">添加图片物理路径</param>
        /// <param name="x">绘制X轴</param>
        /// <param name="y">绘制Y轴</param>
        /// <param name="width">添加图片宽度,0不处理</param>
        /// <param name="height">添加合并图片高度,0不处理</param>
        /// <param name="opacity">透明度：alpha 必须是范围 [0.0, 1.0] 之内（包含边界值）的一个浮点数字</param>
        /// <returns></returns>
        byte[] PressImage(byte[] byteData, string mergeImagePath, int x, int y, int width = 0, int height = 0, float opacity = 1);

        /// <summary>
        /// 图片添加文本
        /// </summary>
        /// <param name="byteData"></param>
        /// <param name="x">绘制X轴</param>
        /// <param name="y">绘制Y轴</param>
        /// <param name="text"></param>
        /// <param name="fontOptions">文字样式</param>
        /// <returns></returns>
        byte[] PressText(byte[] byteData, int x, int y, string text, FontOptions fontOptions);

        /// <summary>
        /// 图片水印
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="imageWatermarkPath">水印文件物理路径</param>
        /// <param name="location">图片水印位置：0=不使用 1=左上 2=中上 3=右上 4=中左 5=中中 6=中右 7=下左 8=下中 9=下右</param>
        /// <param name="opacity">图片水印透明度：alpha 必须是范围 [0.0, 1.0] 之内（包含边界值）的一个浮点数字</param>
        /// <returns></returns>
        byte[] ImageWatermark(byte[] byteData, string imageWatermarkPath, int location, float opacity);

        /// <summary>
        /// 文字水印
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="text">水印文字</param>
        /// <param name="location">图片水印位置：0=不使用 1=左上 2=中上 3=右上 4=中左 5=中中 6=中右 7=下左 8=下中 9=下右</param>
        /// <param name="fontOptions">文字样式</param>
        /// <returns></returns>
        byte[] LetterWatermark(byte[] byteData, string text, int location, FontOptions fontOptions);
    }
}
