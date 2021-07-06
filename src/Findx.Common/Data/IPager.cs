namespace Findx.Data
{
    /// <summary>
    /// 分页器
    /// </summary>
    public interface IPager
    {
        /// <summary>
        /// 分页页数
        /// </summary>
        int PageNo { get; set; }
        /// <summary>
        /// 分页条数
        /// </summary>
        int PageSize { get; set; }
        /// <summary>
        /// 排序字段
        /// </summary>
        string Order { get; set; }
        /// <summary>
        /// 是否正序
        /// </summary>
        bool Asc { get; set; }
    }
}
