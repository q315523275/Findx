namespace Findx.Module.EleAdminPlus.WebAppApi.Vos.Role;

/// <summary>
///     角色数据机构简化信息Vo
/// </summary>
public partial class RoleOrgSimplifyDto
{
    /// <summary>
    ///     id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     上级id, 0是顶级
    /// </summary>
    public long ParentId { get; set; }

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