using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Linq;

namespace Findx.Module.EleAdminPlus.Dtos;

/// <summary>
///     查询登录日志参数
/// </summary>
public class QueryLoginRecordRequest : PageBase
{
    /// <summary>
    ///     账号
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string UserName { set; get; }

    /// <summary>
    ///     用户名
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Nickname { set; get; }

    /// <summary>
    ///     开始时间
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.GreaterOrEqual, Name = "CreatedTime")]
    public DateTime? CreatedTimeStart { set; get; }

    /// <summary>
    ///     结束时间
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Less, Name = "CreatedTime")]
    public DateTime? CreatedTimeEnd { set; get; }
}