using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Module.EleAdminPlus.Shared.Dtos.Auth;

/// <summary>
///     用户头像修改Dto
/// </summary>
public partial class UserAvatarEditDto : IRequest
{
    /// <summary>
    ///     头像(base64)
    /// </summary>
    [Required(ErrorMessage = "请选择头像文件")]
    public string Avatar { get; set; }
}