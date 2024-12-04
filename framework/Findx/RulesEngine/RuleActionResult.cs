namespace Findx.RulesEngine;

/// <summary>
///     规则执行结果
/// </summary>
public class RuleActionResult
{
    /// <summary>
    ///     执行结果
    /// </summary>
    public object Output { get; set; }

    /// <summary>
    ///     异常信息
    /// </summary>
    public Exception Exception { get; set; }
}