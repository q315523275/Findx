namespace Findx.RulesEngine;

/// <summary>
///     规则执行结果
/// </summary>
public class RuleResult
{
    /// <summary>
    ///     规则名称
    /// </summary>
    public string RuleName { get; set; }
    
    /// <summary>
    ///     是否执行成功
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    ///     成功返回事件
    /// </summary>
    public string SuccessEvent { get; set; }
    
    /// <summary>
    ///     异常消息
    /// </summary>
    public string ExceptionMessage { get; set; }
    
    /// <summary>
    ///     执行结果
    /// </summary>
    public RuleActionResult ActionResult { get; set; }
}