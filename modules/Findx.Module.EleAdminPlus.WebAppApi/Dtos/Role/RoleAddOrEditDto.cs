using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.Role;

/// <summary>
///     角色信息新增或修改参数Dto
/// </summary>
public partial class RoleAddOrEditDto : IRequest<long>
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
    ///     Ip限定
    /// </summary>
    public bool IpLimit { get; set; }
    
    /// <summary>
    ///     Ip地址
    /// </summary>
    public string IpAddress { get; set; }
    
    /// <summary>
    ///     备注
    /// </summary>
    public string Comments { get; set; }

    /// <summary>
    ///     菜单集合
    /// </summary>
    public List<long> MenuIds { get; set; } = [];
}