using System.Threading.Tasks;

namespace Findx.Pipelines;

/// <summary>
///     管道调用委托
/// </summary>
/// <typeparam name="TContext">管道调用上下文</typeparam>
public delegate Task PipelineInvokeDelegate<in TContext>(TContext context);

/// <summary>
///     泛型管道接口
/// </summary>
/// <typeparam name="TContext"></typeparam>
public interface IPipelineBehavior<TContext>
{
    /// <summary>
    ///     管道调用执行
    /// </summary>
    /// <param name="context">上下文</param>
    /// <param name="next">下一个管道</param>
    /// <returns></returns>
    Task InvokeAsync(TContext context, PipelineInvokeDelegate<TContext> next);
}