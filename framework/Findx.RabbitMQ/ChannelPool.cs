using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Findx.Locks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Findx.RabbitMQ;

/// <summary>
///     虚拟连接池服务
/// </summary>
public class ChannelPool : IChannelPool
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="connectionPool"></param>
    /// <param name="logger"></param>
    public ChannelPool(IConnectionPool connectionPool, ILogger<ChannelPool> logger)
    {
        ConnectionPool = connectionPool;
        Channels = new ConcurrentDictionary<string, ChannelPoolItem>();
        Logger = logger;
    }

    /// <summary>
    ///     连接池
    /// </summary>
    protected IConnectionPool ConnectionPool { get; }

    /// <summary>
    ///     虚拟连接字典
    /// </summary>
    protected ConcurrentDictionary<string, ChannelPoolItem> Channels { get; }

    /// <summary>
    ///     异步锁
    /// </summary>
    protected readonly AsyncLock AsyncLock = new();
    
    /// <summary>
    ///     是否释放
    /// </summary>
    protected bool IsDisposed { get; private set; }

    /// <summary>
    ///     释放等待时间
    /// </summary>
    protected TimeSpan TotalDisposeWaitDuration { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    ///     日期
    /// </summary>
    public ILogger<ChannelPool> Logger { get; set; }

    /// <summary>
    ///     获取虚拟连接
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="connectionName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual async Task<IChannelAccessor> AcquireAsync(string channelName = null, string connectionName = null, CancellationToken cancellationToken = default)
    {
        CheckDisposed();

        channelName ??= "";

        ChannelPoolItem poolItem;

        if (Channels.TryGetValue(channelName, out var existingChannelPoolItem))
        {
            poolItem = existingChannelPoolItem;
        }
        else
        {
            using (await AsyncLock.LockAsync(cancellationToken))
            {
                if (Channels.TryGetValue(channelName, out var existingChannelPoolItem2))
                {
                    poolItem = existingChannelPoolItem2;
                }
                else
                {
                    poolItem = new ChannelPoolItem(await CreateChannelAsync(channelName, connectionName, cancellationToken));
                    Channels.TryAdd(channelName, poolItem);
                }
            }
        }
        
        poolItem.Acquire();

        if (poolItem.Channel.IsClosed)
        {
            await poolItem.DisposeAsync();
            Channels.TryRemove(channelName, out _);

            using (await AsyncLock.LockAsync(cancellationToken))
            {
                if (Channels.TryGetValue(channelName, out var existingChannelPoolItem3))
                {
                    poolItem = existingChannelPoolItem3;
                }
                else
                {
                    poolItem = new ChannelPoolItem(await CreateChannelAsync(channelName, connectionName, cancellationToken));
                    Channels.TryAdd(channelName, poolItem);
                }
            }

            poolItem.Acquire();
        }
        
        return new ChannelAccessor(poolItem.Channel, channelName, () => poolItem.Release());
    }

    /// <summary>
    ///     创建虚拟连接
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="connectionName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<IChannel> CreateChannelAsync(string channelName, string connectionName, CancellationToken cancellationToken = default)
    {
        return await (await ConnectionPool.GetAsync(connectionName, cancellationToken)).CreateChannelAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    ///     检查释放状态
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    protected void CheckDisposed()
    {
        if (IsDisposed) throw new ObjectDisposedException(nameof(ChannelPool));
    }

    /// <summary>
    ///     资源释放
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;

        if (!Channels.Any())
        {
            Logger.LogDebug($"Disposed channel pool with no channels in the pool.");
            return;
        }

        var poolDisposeStopwatch = Stopwatch.StartNew();

        Logger.LogInformation($"Disposing channel pool ({Channels.Count} channels).");

        var remainingWaitDuration = TotalDisposeWaitDuration;

        foreach (var poolItem in Channels.Values)
        {
            var poolItemDisposeStopwatch = Stopwatch.StartNew();

            try
            {
                poolItem.WaitIfInUse(remainingWaitDuration);
                await poolItem.DisposeAsync();
            }
            catch
            {
                // ignored
            }

            poolItemDisposeStopwatch.Stop();

            remainingWaitDuration = remainingWaitDuration > poolItemDisposeStopwatch.Elapsed
                ? remainingWaitDuration.Subtract(poolItemDisposeStopwatch.Elapsed)
                : TimeSpan.Zero;
        }

        poolDisposeStopwatch.Stop();

        Logger.LogInformation($"Disposed RabbitMQ Channel Pool ({Channels.Count} channels in {poolDisposeStopwatch.Elapsed.TotalMilliseconds:0.00} ms).");

        if (poolDisposeStopwatch.Elapsed.TotalSeconds > 5.0)
        {
            Logger.LogWarning($"Disposing RabbitMQ Channel Pool got time greather than expected: {poolDisposeStopwatch.Elapsed.TotalMilliseconds:0.00} ms.");
        }

        Channels.Clear();
    }

    /// <summary>
    ///     虚拟连接信息
    /// </summary>
    protected class ChannelPoolItem : IAsyncDisposable
    {
        public IChannel Channel { get; }

        public bool IsInUse { get => _isInUse; private set => _isInUse = value; }
        
        private volatile bool _isInUse;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="channel"></param>
        public ChannelPoolItem(IChannel channel)
        {
            Channel = channel;
        }

        public void Acquire()
        {
            lock (this)
            {
                while (IsInUse)
                {
                    Monitor.Wait(this);
                }

                IsInUse = true;
            }
        }

        public void WaitIfInUse(TimeSpan timeout)
        {
            lock (this)
            {
                if (!IsInUse)
                {
                    return;
                }

                Monitor.Wait(this, timeout);
            }
        }

        public void Release()
        {
            lock (this)
            {
                IsInUse = false;
                Monitor.PulseAll(this);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Channel.DisposeAsync();
        }
    }

    /// <summary>
    ///     虚拟连接访问器
    /// </summary>
    protected class ChannelAccessor : IChannelAccessor
    {
        public IChannel Channel { get; }

        public string Name { get; }

        private readonly Action _disposeAction;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="name"></param>
        /// <param name="disposeAction"></param>
        public ChannelAccessor(IChannel channel, string name, Action disposeAction)
        {
            _disposeAction = disposeAction;
            Name = name;
            Channel = channel;
        }

        public void Dispose()
        {
            _disposeAction.Invoke();
        }
    }
}