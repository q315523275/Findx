using System.Threading.Channels;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Common.DisposableEx;
using Findx.DependencyInjection;
using Findx.Exceptions;

namespace Findx.Jobs.Client;

/// <summary>
///     任务执行调度器
/// </summary>
public class JobExecuteDispatcher: Disposable, IJobExecuteDispatcher
{
    private readonly ILogger<JobExecuteDispatcher> _logger;
    private readonly IExceptionNotifier _exceptionNotifier;
    private readonly Channel<JobExecuteInfo> _channel;
    private readonly CancellationTokenSource _cts;
    private readonly List<Task> _workers;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionNotifier"></param>
    public JobExecuteDispatcher(IOptions<JobOptions> options, ILogger<JobExecuteDispatcher> logger, IExceptionNotifier exceptionNotifier)
    {
        _logger = logger;
        _exceptionNotifier = exceptionNotifier;

        //  使用有界通道，消费积压超容量后写入端等待，防止内存无上限增长
        _channel = Channel.CreateBounded<JobExecuteInfo>(new BoundedChannelOptions(options.Value.JobChannelCapacity) { FullMode = BoundedChannelFullMode.Wait });
        _cts = new CancellationTokenSource();

        //  启动消费线程并持有任务引用，便于释放时等待其退出
        _workers = Enumerable.Range(0, options.Value.ClientPoolSize).Select(_ => Task.Run(() => ProcessingAsync(_channel.Reader, _cts.Token))).ToList();
    }

    /// <summary>
    ///     发送执行
    /// </summary>
    /// <param name="jobExecuteInfo"></param>
    /// <param name="cancellationToken"></param>
    public async Task SendAsync(JobExecuteInfo jobExecuteInfo, CancellationToken cancellationToken = default)
    {
        Check.NotNull(jobExecuteInfo, nameof(jobExecuteInfo));
        await _channel.Writer.WriteAsync(jobExecuteInfo, cancellationToken);
    }
    
    /// <summary>
    ///     消费执行方法
    /// </summary>
    /// <param name="channelReader"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ProcessingAsync(ChannelReader<JobExecuteInfo> channelReader, CancellationToken cancellationToken)
    {
        try
        {
            // 异步流方式
            await foreach (var message in channelReader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    await using var scope = ServiceLocator.CreateAsyncScope();
                    var jobExecutor = scope.ServiceProvider.GetRequiredService<IJobExecutor>();
                    var context = new JobExecuteContext(scope.ServiceProvider, message);
                    await jobExecutor.ExecuteAsync(context, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "执行服务通知任务节点时引发异常：{ExMessage}", ex.Message);

                    await _exceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex), cancellationToken);
                }
            }
        }
        catch (Exception)
        {
            // expected
        }
    }

    /// <summary>
    ///     释放
    /// </summary>
    /// <param name="disposing"></param>
    protected override void OnDispose(bool disposing)
    {
        //  基类 Dispose()/DisposeAsync() 仅回调 OnDispose(true) 且无终结器，托管资源释放必须放在 disposing 分支
        if (!disposing) return;

        //  取消消费令牌并关闭通道写入，通知所有消费循环退出
        _cts.Cancel();
        _channel.Writer.Complete();

        try
        {
            //  同步等待消费线程退出，避免后台线程与 CTS 泄漏
            Task.WhenAll(_workers).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "等待作业执行消费线程退出时发生异常");
        }
        finally
        {
            _cts.Dispose();
        }
    }
}