using System.Threading.Channels;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Common.DisposableEx;
using Findx.ExceptionHandling;

namespace Findx.Jobs.Server;

/// <summary>
///     服务端任务触发器
/// </summary>
public class BackgroundJobTriggerServer : Disposable, IBackgroundJobTriggerServer
{
    private readonly ILogger<BackgroundJobTriggerServer> _logger;
    private readonly IBackgroundJobExecuteNotifyServer _backgroundJobExecuteNotifyServer;
    private readonly IExceptionNotifier _exceptionNotifier;
    private readonly Channel<JobExecuteInfo> _channel;
    private readonly CancellationTokenSource _cts;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="backgroundJobExecuteNotifyServer"></param>
    /// <param name="exceptionNotifier"></param>
    public BackgroundJobTriggerServer(IOptions<JobOptions> options, ILogger<BackgroundJobTriggerServer> logger, IBackgroundJobExecuteNotifyServer backgroundJobExecuteNotifyServer, IExceptionNotifier exceptionNotifier)
    {
        _logger = logger;
        _backgroundJobExecuteNotifyServer = backgroundJobExecuteNotifyServer;
        _exceptionNotifier = exceptionNotifier;
        _channel = Channel.CreateUnbounded<JobExecuteInfo>();
        _cts = new CancellationTokenSource();
        // StartConsuming(_cancellationToken.Token);
        Task.WhenAll(Enumerable.Range(0, options.Value.ServerPoolSize).Select(_ => Task.Factory.StartNew(async () => await ProcessingAsync(_channel.Reader, _cts.Token), _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default)));
    }

    /// <summary>
    ///     触发任务
    /// </summary>
    /// <param name="jobExecuteInfo"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task TriggerAsync(JobExecuteInfo jobExecuteInfo, CancellationToken cancellationToken = default)
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
                    await _backgroundJobExecuteNotifyServer.NotifyAsync(message, cancellationToken);
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
        if (!disposing)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _channel.Writer.Complete();
        }
    }
}