using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Module.EleAdminPlus.Dtos.Auth;

/// <summary>
///     修改密码
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