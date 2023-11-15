namespace Findx.Module.EleAdminPlus.Dtos;

/// <summary>
///     设置用户属性值Dto模型
/// </summary>
public class SetUserPropertyRequest
{
    /// <summary>
    ///     编号
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     状态
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    ///     密码
    /// </summary>
    public string Password { get; set; }
}