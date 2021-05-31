namespace Findx.Data
{
    /// <summary>
    /// 分页结果
    /// </summary>
    /// <typeparam name="TList"></typeparam>
    public class PagedResult<TList>
    {
        /// <summary>
        /// 当前分页数
        /// </summary>
        public int PageNo { get; }
        /// <summary>
        /// 当前分页记录数
        /// </summary>
        public int PageSize { get; }
        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalRows { get; }
        /// <summary>
        /// 分页数据
        /// </summary>
        public TList Rows { get; }
        /// <summary>
        /// Ctor
        /// </summary>
        public PagedResult() { }
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRows"></param>
        /// <param name="rows"></param>
        public PagedResult(int pageNo, int pageSize, int totalRows, TList rows)
        {
            PageNo = pageNo;
            PageSize = pageSize;
            TotalRows = totalRows;
            Rows = rows;
        }
    }
}
