namespace Findx.Extensions.WorkflowCore.Models;

/// <summary>
///     流程操作人（发起/审批/转办）
/// </summary>
public class FlwCreator
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public FlwCreator()
    {
    }

    /// <summary>
    ///     Ctor
    /// </summary>
    public FlwCreator(string id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    ///     操作人ID
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     操作人名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     操作人所属部门ID（部门办理人匹配使用）
    /// </summary>
    public string DeptId { get; set; }

    /// <summary>
    ///     操作人角色编码集合（角色办理人匹配使用）
    /// </summary>
    public List<string> RoleIds { get; set; }
}
