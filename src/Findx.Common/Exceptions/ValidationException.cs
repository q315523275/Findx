using System.ComponentModel.DataAnnotations;

namespace Findx.Exceptions;

/// <summary>
///     模型验证异常
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="target"></param>
    /// <param name="validationResults"></param>
    public ValidationException(Type target, IList<ValidationResult> validationResults)
    {
        TargetType = target;
        ValidationResults = validationResults;
    }

    /// <summary>
    ///     验证结果集合
    /// </summary>
    public IList<ValidationResult> ValidationResults { get; }

    /// <summary>
    ///     模型类型
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    ///     异常消息
    /// </summary>
    public override string Message
    {
        get
        {
            return string.Concat(TargetType.ToString(), ": ",
                string.Join(';', ValidationResults.Select(x => $"{x.ErrorMessage}")));
        }
    }
}