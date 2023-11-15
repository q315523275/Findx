using Findx.Data;

namespace Findx.Module.EleAdminPlus.Dtos;

/// <summary>
///     查询字典入参
/// </summary>
public class QueryDictDataRequest : PageBase
{
    /// <summary>
    ///     TypeId
    /// </summary>
    public long? TypeId { set; get; }

    /// <summary>
    ///     字典编号
    /// </summary>
    public string TypeCode { set; get; }

    /// <summary>
    ///     关键字
    /// </summary>
    public string Keywords { set; get; }
}