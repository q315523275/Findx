using Findx.Data;

namespace Findx.Module.EleAdminPlus.Dtos;

/// <summary>
///     设置角色信息Dto模型
/// </summary>
public class SetRoleRequest : IRequest
{
    /// <summary>
    ///     编号
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    ///     角色名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     角色标识
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     备注
    /// </summary>
    public string Comments { get; set; }
}