using System.ComponentModel.DataAnnotations;
using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.Auth;

/// <summary>
///     保存用户头像信息Dto
/// </summary>
public class SaveUserAvatarDto : IRequest
{
    /// <summary>
    ///     头像
    /// </summary>
    [Required(ErrorMessage = "请选择头像文件")]
    public string Avatar { get; set; }
}