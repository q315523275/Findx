using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Module.EleAdminPlus.Shared.Dtos.Auth;

/// <summary>
///     更新密码参数Dto
/// </summary>
public class UpdatePasswordDto: IRequest
{
    /// <summary>
    ///     旧密码
    /// </summary>
    [Required]
    public string OldPassword { set; get; }

    /// <summary>
    ///     旧密码
    /// </summary>
    [Required]
    public string Password { set; get; }

    /// <summary>
    ///     旧密码
    /// </summary>
    [Required]
    [Compare(nameof(Password))]
    public string Password2 { set; get; }
}