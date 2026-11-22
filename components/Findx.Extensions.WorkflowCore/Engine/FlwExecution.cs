using Findx.Extensions.WorkflowCore.Entity;
using Findx.Extensions.WorkflowCore.Models;

namespace Findx.Extensions.WorkflowCore.Engine;

/// <summary>
///     引擎执行上下文
/// </summary>
internal class FlwExecution
{
    /// <summary>
    ///     流程实例
    /// </summary>
    public FlwInstanceInfo Instance { get; set; }

    /// <summary>
    ///     流程模型
    /// </summary>
    public FlwModel Model { get; set; }

    /// <summary>
    ///     流程变量
    /// </summary>
    public Dictionary<string, object> Variables { get; set; } = new();

    /// <summary>
    ///     当前操作人
    /// </summary>
    public FlwCreator Operator { get; set; }

    /// <summary>
    ///     本次动作的审批意见：终态回调时透传给业务适配器，写入单据操作记录
    /// </summary>
    public string LastComment { get; set; }
}

/// <summary>
///     引擎异常
/// </summary>
public class FlwEngineException: Exception
{
    /// <summary>
    ///     Ctor
    /// </summary>
    public FlwEngineException(string message): base(message)
    {
    }
}
