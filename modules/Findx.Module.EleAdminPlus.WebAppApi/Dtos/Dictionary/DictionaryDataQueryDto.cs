using Findx.Expressions;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.Dictionary;

/// <summary>
///     查询字典数据入参
/// </summary>
public class DictionaryDataQueryDto : SortCondition
{
    /// <summary>
    ///     DictId
    /// </summary>
    public long? DictId { set; get; }

    /// <summary>
    ///     字典编号
    /// </summary>
    public string DictCode { set; get; }

    /// <summary>
    ///     关键字
    /// </summary>
    public string Keywords { set; get; }
}