using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Vos.Dictionary;

/// <summary>
///     字典数据Vo
/// </summary>
public class DictionaryDataTypeSimplifyVo: IResponse
{
    /// <summary>
    ///     字典名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     字典编码
    /// </summary>
    public string Code { get; set; }
}