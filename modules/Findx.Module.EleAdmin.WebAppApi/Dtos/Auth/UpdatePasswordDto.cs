using System.ComponentModel.DataAnnotations;

namespace Findx.Module.EleAdmin.Dtos.Auth;

/// <summary>
///     修改密码
/// </summary>
public class UpdatePasswordDto
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