using Findx.Data;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.UserFile;

/// <summary>
///     修改用户文件参数Dto
/// </summary>
public class UserFileEditDto : IRequest<long>
{
    /// <summary>
    ///     主键id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     文件名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     上级id
    /// </summary>
    public long? ParentId { get; set; }
}
