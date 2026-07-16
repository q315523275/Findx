using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.Dictionary;

/// <summary>
///     分页查询字典数据入参
/// </summary>
public class DictionaryDataPageQueryDto: DictionaryDataQueryDto, IPager
{
    /// <summary>
    ///     页码
    /// </summary>
    public int PageNo { get; set; }
    
    /// <summary>
    ///     每页数量
    /// </summary>
    public int PageSize { get; set; }
}