using System.Threading.Tasks;

namespace Findx.Imaging;

/// <summary>
///     图片处理器
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    ///     图片大小调整
    /// </summary>
    /// <param name="imageByte">文件字节数组</param>
    /// <param name="imageResizeDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> ResizeAsync(byte[] imageByte, ImageResizeDto imageResizeDto, CancellationToken cancellationToken = default);

    /// <summary>
    ///     图片大小调整
    /// </summary>
    /// <param name="imageStream">文件流数据</param>
    /// <param name="imageResizeDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> ResizeAsync(Stream imageStream, ImageResizeDto imageResizeDto, CancellationToken cancellationToken = default);

    
    /// <summary>
    ///     图片裁剪
    /// </summary>
    /// <param name="imageByte">图片字节数组</param>
    /// <param name="imageCropDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> CropAsync(byte[] imageByte, ImageCropDto imageCropDto, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     图片裁剪
    /// </summary>
    /// <param name="imageStream">图片流数据</param>
    /// <param name="imageCropDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> CropAsync(Stream imageStream, ImageCropDto imageCropDto, CancellationToken cancellationToken = default);

    
    /// <summary>
    ///     图片压缩
    /// </summary>
    /// <param name="imageByte"></param>
    /// <param name="quality">压缩比率：alpha 必须是范围 (0~100] 的数字</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> CompressAsync(byte[] imageByte, int quality = 75, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     图片压缩
    /// </summary>
    /// <param name="imageStream"></param>
    /// <param name="quality">压缩比率：alpha 必须是范围 (0~100] 的数字</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> CompressAsync(Stream imageStream, int quality = 75, CancellationToken cancellationToken = default);

    
    /// <summary>
    ///     图片灰色化
    /// </summary>
    /// <param name="imageByte">图片字节数组</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> GrayAsync(byte[] imageByte, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     图片灰色化
    /// </summary>
    /// <param name="imageStream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> GrayAsync(Stream imageStream, CancellationToken cancellationToken = default);

    
    /// <summary>
    ///     图片二值化
    /// </summary>
    /// <param name="imageByte">图片字节数组</param>
    /// <param name="threshold">阀值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> BinaryAsync(byte[] imageByte, int threshold = 180, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     图片二值化
    /// </summary>
    /// <param name="imageStream">图片数据流</param>
    /// <param name="threshold">阀值</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> BinaryAsync(Stream imageStream, int threshold = 180, CancellationToken cancellationToken = default);


    /// <summary>
    ///     图片旋转图片
    /// </summary>
    /// <param name="imageByte">图片字节数组</param>
    /// <param name="degrees">旋转角度</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> RotateAsync(byte[] imageByte, float degrees, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     图片旋转图片
    /// </summary>
    /// <param name="imageStream">图片数据流</param>
    /// <param name="degrees">旋转角度</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> RotateAsync(Stream imageStream, float degrees, CancellationToken cancellationToken = default);


    /// <summary>
    ///     图片翻转
    /// </summary>
    /// <param name="imageByte">图片字节数组</param>
    /// <param name="imageFlipMode"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> FlipAsync(byte[] imageByte, ImageFlipMode imageFlipMode, CancellationToken cancellationToken = default);

    /// <summary>
    ///     图片翻转
    /// </summary>
    /// <param name="imageStream">图片数据流</param>
    /// <param name="imageFlipMode"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> FlipAsync(Stream imageStream, ImageFlipMode imageFlipMode, CancellationToken cancellationToken = default);


    /// <summary>
    ///     合并图片
    /// </summary>
    /// <param name="imageByte">图片字节数组</param>
    /// <param name="mergeImageByte">合并图片字节数组</param>
    /// <param name="mergeImageDto">合并图片参数</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> MergeImageAsync(byte[] imageByte, byte[] mergeImageByte, MergeImageDto mergeImageDto, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     合并图片
    /// </summary>
    /// <param name="imageStream">图片数据流</param>
    /// <param name="mergeImageStream">合并图片数据流</param>
    /// <param name="mergeImageDto">合并图片参数</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> MergeImageAsync(Stream imageStream, Stream mergeImageStream, MergeImageDto mergeImageDto, CancellationToken cancellationToken = default);

    /// <summary>
    ///     绘制文本
    /// </summary>
    /// <param name="imageByte"></param>
    /// <param name="text"></param>
    /// <param name="drawTextDto">文本参数</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> DrawTextAsync(byte[] imageByte, string text, DrawTextDto drawTextDto, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     图片添加文本
    /// </summary>
    /// <param name="imageStream"></param>
    /// <param name="text"></param>
    /// <param name="drawTextDto">文本参数</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> DrawTextAsync(Stream imageStream, string text, DrawTextDto drawTextDto, CancellationToken cancellationToken = default);

    /// <summary>
    ///     图片水印
    /// </summary>
    /// <param name="imageByte">图片字节数组</param>
    /// <param name="watermarkImageByte">水印文件物理路径</param>
    /// <param name="location">图片水印位置：0=不使用 1=左上 2=中上 3=右上 4=中左 5=中中 6=中右 7=下左 8=下中 9=下右</param>
    /// <param name="opacity">图片水印透明度：alpha 必须是范围 [0.0, 1.0] 之内（包含边界值）的一个浮点数字</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> ImageWatermarkAsync(byte[] imageByte, byte[] watermarkImageByte, int location, float opacity, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     图片水印
    /// </summary>
    /// <param name="imageStream">图片字节数组</param>
    /// <param name="watermarkImageStream">水印文件物理路径</param>
    /// <param name="location">图片水印位置：0=不使用 1=左上 2=中上 3=右上 4=中左 5=中中 6=中右 7=下左 8=下中 9=下右</param>
    /// <param name="opacity">图片水印透明度：alpha 必须是范围 [0.0, 1.0] 之内（包含边界值）的一个浮点数字</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> ImageWatermarkAsync(Stream imageStream, Stream watermarkImageStream, int location, float opacity, CancellationToken cancellationToken = default);

    /// <summary>
    ///     文字水印
    /// </summary>
    /// <param name="imageByte">图片字节数组</param>
    /// <param name="text">水印文字</param>
    /// <param name="location">图片水印位置：0=不使用 1=左上 2=中上 3=右上 4=中左 5=中中 6=中右 7=下左 8=下中 9=下右</param>
    /// <param name="drawTextDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> LetterWatermarkAsync(byte[] imageByte, string text, int location, DrawTextDto drawTextDto, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     文字水印
    /// </summary>
    /// <param name="imageStream">图片字节数组</param>
    /// <param name="text">水印文字</param>
    /// <param name="location">图片水印位置：0=不使用 1=左上 2=中上 3=右上 4=中左 5=中中 6=中右 7=下左 8=下中 9=下右</param>
    /// <param name="drawTextDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Stream> LetterWatermarkAsync(Stream imageStream, string text, int location, DrawTextDto drawTextDto, CancellationToken cancellationToken = default);
}