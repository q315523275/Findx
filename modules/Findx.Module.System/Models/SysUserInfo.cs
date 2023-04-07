using Findx.Data;
using FreeSql.DataAnnotations;

namespace Findx.Module.System.Models;

/// <summary>
/// 用户信息表
/// </summary>
[Table(Name = "sys_user")]
[EntityExtension(DataSource = "sys")]
public class SysUserInfo
{
    
}