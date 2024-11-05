using Findx.Data;

namespace Findx.Module.EleAdmin.Dtos.Dict;

/// <summary>
///     查询字典入参
/// </summary>
public class DictDataQueryDto : PageBase
{
    /// <summary>
    ///     TypeId
    /// </summary>
    public Guid TypeId { set; get; } = Guid.Empty;

    /// <summary>
    ///     字典编号
    /// </summary>
    public string TypeCode { set; get; }

    /// <summary>
    ///     关键字
    /// </summary>
    public string Keywords { set; get; }
}