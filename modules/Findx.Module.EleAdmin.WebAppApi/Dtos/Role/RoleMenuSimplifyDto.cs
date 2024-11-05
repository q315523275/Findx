namespace Findx.Module.EleAdmin.Dtos.Role;

/// <summary>
///     角色菜单Dto模型
/// </summary>
public class RoleMenuSimplifyDto
{
    /// <summary>
    ///     id
    /// </summary>
    public Guid Id { get; set; } = Guid.Empty;

    /// <summary>
    ///     上级id, 0是顶级
    /// </summary>
    public Guid ParentId { get; set; } = Guid.Empty;

    /// <summary>
    ///     菜单名称
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     菜单路由地址
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    ///     菜单组件地址, 目录可为空
    /// </summary>
    public string Component { get; set; }

    /// <summary>
    ///     类型, 0菜单, 1按钮
    /// </summary>
    public int MenuType { get; set; }

    /// <summary>
    ///     排序号
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     权限标识
    /// </summary>
    public string Authority { get; set; }

    /// <summary>
    ///     打开位置
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    ///     菜单图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    ///     图标颜色
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    ///     是否隐藏, 0否, 1是(仅注册路由不显示在左侧菜单)
    /// </summary>
    public int Hide { get; set; }

    /// <summary>
    ///     菜单侧栏选中的path
    /// </summary>
    public string Active { get; set; }

    /// <summary>
    ///     其它路由元信息
    /// </summary>
    public string Meta { get; set; }

    /// <summary>
    ///     创建时间
    /// </summary>
    public virtual DateTime? CreatedTime { get; set; }

    /// <summary>
    ///     是否选中
    /// </summary>
    public virtual bool Checked { get; set; }
}