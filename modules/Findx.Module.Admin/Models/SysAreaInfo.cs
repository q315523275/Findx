namespace Findx.Module.Admin.Models
{

    /// <summary>
    /// 中国行政地区表
    /// </summary>
    public partial class SysArea
    {

        public ulong Id { get; set; }

        /// <summary>
        /// 行政代码
        /// </summary>
        public string AreaCode { get; set; } = string.Empty;

        /// <summary>
        /// 区号
        /// </summary>
        public string CityCode { get; set; } = string.Empty;

        /// <summary>
        /// 纬度
        /// </summary>
        public decimal? Lat { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        public byte? LevelCode { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public decimal? Lng { get; set; }

        /// <summary>
        /// 组合名
        /// </summary>
        public string MergerName { get; set; } = string.Empty;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 父级行政代码
        /// </summary>
        public string ParentCode { get; set; } = string.Empty;

        /// <summary>
        /// 拼音
        /// </summary>
        public string Pinyin { get; set; } = string.Empty;

        /// <summary>
        /// 简称
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// 邮政编码
        /// </summary>
        public string ZipCode { get; set; } = string.Empty;

    }

}
