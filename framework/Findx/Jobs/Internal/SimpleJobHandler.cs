using System.Threading.Tasks;
using Findx.Extensions;
using Findx.Messaging;

namespace Findx.Jobs.Internal;

internal class SimpleJobHandler : IApplicationEventHandler<JobInfo>
{
    private readonly IJobListener _listener;
    private readonly IServiceProvider _provider;

    public SimpleJobHandler(IServiceProvider provider, IJobListener listener)
    {
        _provider = provider;
        _listener = listener;
    }

    public async Task HandleAsync(JobInfo message, CancellationToken cancellationToken = default)
    {
        Check.NotNull(message, nameof(message));

        var context = new JobExecutionContext(_provider, message.Id, message.Id, message.FullName)
        {
            Parameter = (message.JsonParam ?? "{}").ToObject<Dictionary<string, string>>(),
            JobName = message.Name
        };

        await _listener.JobToRunAsync(context, cancellationToken);

        // TODO 进程内存版本可以使用，分布式需改为执行节点上报
        // 固定间隔时间任务，上报执行结果
        if (message.FixedDelay > 0)
            message.Increment();
    }
}