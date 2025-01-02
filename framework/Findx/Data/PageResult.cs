using System.Text.Json.Serialization;

namespace Findx.Data;

/// <summary>
///     分页结果
/// </summary>
/// <typeparam name="TList"></typeparam>
public class PageResult<TList>
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="pageNo"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalRows"></param>
    /// <param name="rows"></param>
    [JsonConstructor]
    public PageResult(int pageNo, int pageSize, int totalRows, TList rows)
    {
        PageNo = pageNo;
        PageSize = pageSize;
        TotalRows = totalRows;
        Rows = rows;
    }

    /// <summary>
    ///     当前分页数
    /// </summary>
    public int PageNo { get; }

    /// <summary>
    ///     当前分页记录数
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    ///     总记录数
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    ///     分页数据
    /// </summary>
    public TList Rows { get; }
}