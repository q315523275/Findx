using System.ComponentModel;

namespace Findx.Drawing
{
    /// <summary>
    /// 字体信息
    /// </summary>
    public class FontOptions
    {
        /// <summary>
        /// 字体文件路径
        /// </summary>
        public string FontFamilyFilePath { set; get; }

        /// <summary>
        /// 字体大写
        /// </summary>
        public int FontSize { set; get; }

        /// <summary>
        /// 字体颜色
        /// </summary>
        public string FontColor { set; get; }

        /// <summary>
        /// 是否加粗
        /// </summary>
        public FontStyle FontStyle { set; get; } = FontStyle.Regular;
    }
    /// <summary>
    /// 字体样式
    /// </summary>
    public enum FontStyle
    {
        /// <summary>
        /// 正常
        /// </summary>
        [Description("正常")]
        Regular,
        /// <summary>
        /// 加粗
        /// </summary>
        [Description("加粗")]
        Bold,
        /// <summary>
        /// 斜体
        /// </summary>
        [Description("斜体")]
        Italic,
        /// <summary>
        /// 加粗斜体
        /// </summary>
        [Description("加粗斜体")]
        BoldItalic
    }

}
