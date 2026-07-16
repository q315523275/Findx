using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Module.EleAdminPlus.Shared.Dtos.Auth;

/// <summary>
///     登录请求参数Dto
/// </summary>
public partial class LoginRequestDto: ValidatableObject, IRequest
{
    /// <summary>
    ///     租户编码（用于识别租户）
    /// </summary>
    public string TenantCode { set; get; }
    
    /// <summary>
    ///     账号
    /// </summary>
    [Required]
    public string Username { set; get; }

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
    ///     CaptchaKey
    /// </summary>
    public string CaptchaKey { set; get; }
}