using Findx.Data;

namespace Findx.Extensions.WorkflowCore.Models;

/// <summary>
///     全量属性实体基类
/// </summary>
public abstract class FullEntityBase: EntityBase, IDataOwner<long>
{
    /// <summary>
    ///     机构Id
    /// </summary>
    public long? OrgId { get; set; }
     
    /// <summary>
    ///     机构名称
    /// </summary>
    public string OrgName { get; set; }
     
    /// <summary>
    ///     数据拥有者Id
    /// </summary>
    public long? OwnerId { get; set; }
     
    /// <summary>
    ///     数据拥有者
    /// </summary>
    public string Owner { get; set; }
}