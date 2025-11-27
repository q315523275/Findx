using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Module.EleAdminPlus.Shared.Dtos.Auth;

/// <summary>
///     更新用户头像参数Dto
/// </summary>
public partial class UpdateUserAvatarDto : IRequest
{
    /// <summary>
    ///     头像(base64)
    /// </summary>
    [Required(ErrorMessage = "请选择头像文件")]
    public string Avatar { get; set; }
}