namespace Findx.Data
{
    /// <summary>
    /// 查询请求分页基类
    /// </summary>
    public class PagerBase : IPager
    {
        /// <summary>
        /// 当前分页数
        /// 默认：1
        /// </summary>
        public int PageNo { get; set; } = 1;
        /// <summary>
        /// 当前分页记录数
        /// 默认：2
        /// </summary>
        public int PageSize { get; set; } = 20;
        /// <summary>
        /// 分页条件
        /// </summary>
        public string Order { get; set; } = "id";
        /// <summary>
        /// 排序
        /// </summary>
        public bool Asc { get; set; } = true;
    }
}
