namespace Findx.Module.EleAdmin.Dtos.Role;

/// <summary>
///     角色数据机构范围
/// </summary>
public class RoleOrgDto
{
    /// <summary>
    ///     id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     上级id, 0是顶级
    /// </summary>
    public Guid ParentId { get; set; }

    /// <summary>
    ///     机构名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     机构全称
    /// </summary>
    public string FullName { get; set; }
    
    /// <summary>
    ///     排序号
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     是否选中
    /// </summary>
    public virtual bool Checked { get; set; }
}