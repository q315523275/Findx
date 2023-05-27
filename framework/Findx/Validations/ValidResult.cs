namespace Findx.Validations;

/// <summary>
///     校验结果
/// </summary>
public class ValidResult
{
    /// <summary>
    ///     错误成员列表
    /// </summary>
    public List<ErrorMember> ErrorMembers { get; set; }

    /// <summary>
    ///     是否有效
    /// </summary>
    public bool IsVaild { get; set; }
}