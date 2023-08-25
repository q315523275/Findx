using System.Threading.Channels;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.ExceptionHandling;

namespace Findx.Messaging;

/// <summary>
///     消息通知者
/// </summary>
public class ApplicationEventPublisher : IApplicationEventPublisher, IDisposable
{
    private readonly Channel<IApplicationEvent> _channel;
    private readonly CancellationTokenSource _cts;
    private readonly IExceptionNotifier _exceptionNotifier;
    private readonly ILogger<ApplicationEventPublisher> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionNotifier"></param>
    public ApplicationEventPublisher(IConfiguration configuration, ILogger<ApplicationEventPublisher> logger,
        IExceptionNotifier exceptionNotifier)
    {
        _logger = logger;
        _exceptionNotifier = exceptionNotifier;

        var queueCapacity = configuration.GetValue<int>("Findx:MessageQueueCapacity");
        _channel = queueCapacity > 0
            ? Channel.CreateBounded<IApplicationEvent>(new BoundedChannelOptions(queueCapacity)
                { FullMode = BoundedChannelFullMode.Wait })
            : Channel.CreateUnbounded<IApplicationEvent>();

        var consumerThreadCount = configuration.GetValue<int>("Findx:MessageHandlerMaxTaskCount");
        consumerThreadCount = consumerThreadCount == 0 ? Environment.ProcessorCount + 1 : consumerThreadCount;
        consumerThreadCount = consumerThreadCount <= 0 ? 1 : consumerThreadCount;

        _cts = new CancellationTokenSource();

        // StartConsuming(_cancellationToken.Token);
        Task.WhenAll(Enumerable.Range(0, consumerThreadCount).Select(_ => Task.Factory.StartNew(async () => await ProcessingAsync(_channel.Reader, _cts.Token), _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default)));
    }

    /// <summary>
    ///     推送消息
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="applicationEvent"></param>
    /// <returns></returns>
    public bool Publish<TEvent>(TEvent applicationEvent) where TEvent : IApplicationEvent
    {
        Check.NotNull(applicationEvent, nameof(applicationEvent));

        return _channel.Writer.TryWrite(applicationEvent);
    }

    /// <summary>
    ///     推送消息
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="applicationEvent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task PublishAsync<TEvent>(TEvent applicationEvent, CancellationToken cancellationToken = default) where TEvent : IApplicationEvent
    {
        Check.NotNull(applicationEvent, nameof(applicationEvent));

        await _channel.Writer.WriteAsync(applicationEvent, cancellationToken);
    }

    /// <summary>
    ///     释放
    /// </summary>
    public void Dispose()
    {
        _cts?.Cancel();
        _channel.Writer.Complete();
        MessageConst.ApplicationEventHandlers?.Clear();
    }

    /// <summary>
    ///     消费执行方法
    /// </summary>
    /// <param name="channelReader"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ProcessingAsync(ChannelReader<IApplicationEvent> channelReader, CancellationToken cancellationToken)
    {
        try
        {
            // 异步流方式
            await foreach (var message in channelReader.ReadAllAsync(cancellationToken))
            {
                var messageType = message.GetType();
                var handler = (ApplicationEventHandlerWrapper)MessageConst.ApplicationEventHandlers.GetOrAdd(messageType, _ => Activator.CreateInstance(typeof(ApplicationEventHandlerWrapperImpl<>).MakeGenericType(messageType)));
                try
                {
                    await using var scope = ServiceLocator.Instance.CreateAsyncScope();
                    await handler.HandleAsync(message, scope.ServiceProvider, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "执行应用事件“{MessageTypeName}”的处理器“{Type}”时引发异常：{ExMessage}", messageType.Name,
                        handler.GetType(), ex.Message);

                    await _exceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex), cancellationToken);
                }
            }
        }
        catch (Exception)
        {
            // expected
        }
    }
}