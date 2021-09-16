namespace Findx.Drawing
{
    /// <summary>
    /// 图片处理器
    /// </summary>
    public interface IImageProcessor
    {
        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="byteData">文件字节数组</param>
        /// <param name="fileExt">文件扩展名</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="mode">生成缩略图的方式(默认填充)</param>
        /// <returns></returns>
        byte[] MakeThumbnail(byte[] byteData, string fileExt, int width, int height, ImageResizeMode mode = ImageResizeMode.Pad);

        /// <summary>
        /// 图片水印
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="fileExt">图片扩展名</param>
        /// <param name="imageWatermarkPath">水印文件物理路径</param>
        /// <param name="location">图片水印位置 0=不使用 1=左上 2=中上 3=右上 4=左中  9=右下</param>
        /// <param name="opacity">水印的透明度 0--1 1为不透明</param>
        /// <returns></returns>
        byte[] ImageWatermark(byte[] byteData, string fileExt, string imageWatermarkPath, int location, float opacity);

        /// <summary>
        /// 文字水印
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="fileExt">图片扩展名</param>
        /// <param name="text">水印文字</param>
        /// <param name="location">图片水印位置 0=不使用 1=左上 2=中上 3=右上 4=左中  9=右下</param>
        /// <param name="fontName">字体</param>
        /// <param name="fontSize">字体大小</param>
        /// <returns></returns>
        byte[] LetterWatermark(byte[] byteData, string fileExt, string text, int location, string fontName, int fontSize);

        /// <summary>
        /// 合并图片
        /// </summary>
        /// <param name="byteData">图片字节数组</param>
        /// <param name="fileExt">图片扩展名</param>
        /// <param name="mergeImagePath">合并图片物理路径</param>
        /// <param name="X">X轴</param>
        /// <param name="Y">Y轴</param>
        /// <param name="width">合并图片宽度,0不处理</param>
        /// <param name="height">合并图片高度,0不处理</param>
        /// <returns></returns>
        byte[] MergeImage(byte[] byteData, string fileExt, string mergeImagePath, int X, int Y, int width = 0, int height = 0);

        /// <summary>
        /// 绘制多行文本
        /// </summary>
        /// <param name="byteData"></param>
        /// <param name="fileExt"></param>
        /// <param name="text"></param>
        /// <param name="fontName"></param>
        /// <param name="fontSize"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        byte[] MultilineText(byte[] byteData, string fileExt, string text, string fontName, int fontSize, int X, int Y);
    }
}
