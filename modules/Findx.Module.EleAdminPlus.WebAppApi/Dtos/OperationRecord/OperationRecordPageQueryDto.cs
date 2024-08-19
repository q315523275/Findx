using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.OperationRecord;

/// <summary>
///     分页查询操作系统记录
/// </summary>
public class OperationRecordPageQueryDto: OperationRecordQueryDto, IPager
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