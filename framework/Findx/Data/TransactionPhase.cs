using System.ComponentModel;

namespace Findx.Data;

/// <summary>
///     事物阶段枚举
/// </summary>
public enum TransactionPhase
{
    /// <summary>
    ///     事务提交前触发
    /// </summary>
    [Description("事务提交前触发")]
    BeforeCommit,
    
    /// <summary>
    ///     事务提交后触发
    /// </summary>
    [Description("事务提交后触发")]
    AfterCommit,
    
    /// <summary>
    ///     事务回滚触发
    /// </summary>
    [Description("事务回滚触发")]
    AfterRollback,
    
    /// <summary>
    ///     事务完成后触发
    /// </summary>
    [Description("事务完成后触发")]
    AfterCompletion
}