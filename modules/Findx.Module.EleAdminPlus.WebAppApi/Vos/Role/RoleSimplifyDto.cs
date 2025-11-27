using Findx.Data;
using Findx.Module.EleAdminPlus.Shared.Enums;

namespace Findx.Module.EleAdminPlus.WebAppApi.Vos.Role;

/// <summary>
///     角色简化信息Vo
/// </summary>
public partial class RoleSimplifyDto: IResponse
{
    /// <summary>
    ///     角色id
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
    ///     数据范围（1：全部数据权限 2：自定数据权限 3：本部门数据权限 4：本部门及以下数据权限）
    /// </summary>
    public DataScope DataScope { get; set; }
        
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
    ///     创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }
}