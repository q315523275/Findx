using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Findx.Data;

/// <summary>
///     查询请求分页基类
/// </summary>
public abstract class PageBase : IPager
{
    /// <summary>
    ///     当前分页数
    ///     默认：1
    /// </summary>
    [Range(1, 9999)]
    public virtual int PageNo { get; set; } = 1;

    /// <summary>
    ///     当前分页记录数
    ///     默认：2
    /// </summary>
    [Range(1, 9999)]
    public virtual int PageSize { get; set; } = 20;


    /// <summary>
    ///     排序字段
    /// </summary>
    public string SortField { get; set; } = "id";

    /// <summary>
    ///     排序方向
    /// </summary>
    public ListSortDirection SortDirection { get; set; } = ListSortDirection.Descending;
}