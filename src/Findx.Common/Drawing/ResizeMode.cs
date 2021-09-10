namespace Findx.Drawing
{
    /// <summary>
    /// 图片缩放方式
    /// </summary>
    public enum ImageResizeMode
    {
        /// <summary>
        /// 裁剪
        /// </summary>
        Crop,
        /// <summary>
        /// 自适应填充
        /// </summary>
        Pad,
        /// <summary>
        /// 固定宽度等比缩放
        /// </summary>
        AutoByWidth,
        /// <summary>
        /// 固定高度等比缩放
        /// </summary>
        AutoByHeight,
        /// <summary>
        /// 拉伸填充(容易变形)
        /// </summary>
        Stretch
    }
}
