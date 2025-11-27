namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.User;

/// <summary>
///     修改状态参数Dto
/// </summary>
public class UpdateStatusDto
{
    /// <summary>
    ///     编号
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     状态
    /// </summary>
    public int Status { get; set; }
}