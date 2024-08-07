using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Module.EleAdminPlus.Dtos.Auth;

/// <summary>
///     登录参数
/// </summary>
public class LoginDto: IRequest
{
    /// <summary>
    ///     账号
    /// </summary>
    [Required]
    public string UserName { set; get; }

    /// <summary>
    ///     密码
    /// </summary>
    [Required]
    public string Password { set; get; }

    /// <summary>
    ///     验证码
    /// </summary>
    public string Code { set; get; }

    /// <summary>
    ///     租户
    /// </summary>
    public Guid TenantId { set; get; }

    /// <summary>
    ///     uuid
    /// </summary>
    [Required]
    public string Uuid { set; get; }
}