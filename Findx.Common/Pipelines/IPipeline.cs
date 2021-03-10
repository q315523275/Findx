using System.Threading;
using System.Threading.Tasks;

namespace Findx.Pipelines
{
    public delegate Task PipelineInvokeDelegate();
    public interface IPipeline<TContext>
    {
        Task InvokeAsync(TContext context, PipelineInvokeDelegate next, CancellationToken cancellationToken = default);
    }
}
