namespace Findx.Module.EleAdminPlus.WebAppApi.Options;

/// <summary>
///     图片配置信息
/// </summary>
public class ImageOptions
{
    /// <summary>
    ///     是否进行尺寸调整
    /// </summary>
    public bool ScaleSize { set; get; }

    /// <summary>
    ///     尺寸等比缩放最大宽度
    /// </summary>
    public int ScaleMaxWidth { set; get; }

    /// <summary>
    ///     是否进行质量压缩
    /// </summary>
    public bool Compress { set; get; }

    /// <summary>
    ///     进行质量压缩比率
    /// </summary>
    public int CompressQuality { set; get; } = 75;
}