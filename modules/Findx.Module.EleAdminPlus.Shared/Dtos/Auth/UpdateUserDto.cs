using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Module.EleAdminPlus.Shared.Dtos.Auth;

/// <summary>
///     更新用户信息Dto
/// </summary>
public partial class UpdateUserDto : IRequest
{
    /// <summary>
    ///     昵称
    /// </summary>
    [Required]
    public string Nickname { get; set; }

    /// <summary>
    ///     性别
    /// </summary>
    public int Sex { get; set; }

    /// <summary>
    ///     手机号
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    ///     邮箱
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    ///     出生日期
    /// </summary>
    public DateTime? Birthday { get; set; }

    /// <summary>
    ///     个人简介
    /// </summary>
    public string Introduction { get; set; }
}