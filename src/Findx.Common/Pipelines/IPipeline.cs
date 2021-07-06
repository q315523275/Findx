using System.Threading;
using System.Threading.Tasks;

namespace Findx.Pipelines
{
    /// <summary>
    /// 管道调用委托
    /// </summary>
    /// <returns></returns>
    public delegate Task PipelineInvokeDelegate();

    /// <summary>
    /// 泛型管道接口
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public interface IPipeline<TContext>
    {
        /// <summary>
        /// 管道调用执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task InvokeAsync(TContext context, PipelineInvokeDelegate next, CancellationToken cancellationToken = default);
    }
}
