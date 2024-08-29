using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Findx.Expressions;

namespace Findx.Data;

/// <summary>
///     查询请求分页基类
/// </summary>
public abstract class PageBase : SortCondition, IPager
{
    /// <summary>
    ///     当前分页数
    ///     默认：1
    /// </summary>
    [Range(1, 99999999)]
    public virtual int PageNo { get; set; } = 1;

    /// <summary>
    ///     当前分页记录数
    ///     默认：2
    /// </summary>
    [Range(1, 9999)]
    public virtual int PageSize { get; set; } = 20;
}