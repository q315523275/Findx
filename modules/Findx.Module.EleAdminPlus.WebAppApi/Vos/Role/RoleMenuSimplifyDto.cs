namespace Findx.Module.EleAdminPlus.WebAppApi.Vos.Role;

/// <summary>
///     角色数据菜单简化信息Vo
/// </summary>
public partial class RoleMenuSimplifyDto
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
    ///     菜单名称
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     排序号
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     是否选中
    /// </summary>
    public virtual bool Checked { get; set; }
}