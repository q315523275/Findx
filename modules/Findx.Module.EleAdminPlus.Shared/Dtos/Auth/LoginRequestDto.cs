using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Module.EleAdminPlus.Shared.Dtos.Auth;

/// <summary>
///     登录请求参数Dto
/// </summary>
public partial class LoginRequestDto: ValidatableObject, IRequest
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
    public string TenantId { set; get; }

    /// <summary>
    ///     uuid
    /// </summary>
    [Required]
    public string Uuid { set; get; }
}