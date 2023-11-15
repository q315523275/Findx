using Findx.Data;

namespace Findx.Module.EleAdminPlus.Dtos;

/// <summary>
///     查询操作系统记录
/// </summary>
public class QueryOperationRecordRequest : PageBase
{
    /// <summary>
    ///     账号
    /// </summary>
    public string UserName { set; get; }

    /// <summary>
    ///     用户名
    /// </summary>
    public string Nickname { set; get; }

    /// <summary>
    ///     开始时间
    /// </summary>
    public DateTime? CreatedTimeStart { set; get; }

    /// <summary>
    ///     结束时间
    /// </summary>
    public DateTime? CreatedTimeEnd { set; get; }
}