﻿using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.ExceptionHandling;

namespace Findx.Threading;

/// <summary>
///     异步定时器
/// </summary>
public class AsyncTimer : ITransientDependency
{
    private readonly Timer _taskTimer;
    private volatile bool _isRunning;
    private volatile bool _performingTasks;

    /// <summary>
    ///     执行方法
    /// </summary>
    public Func<AsyncTimer, Task> Elapsed = _ => Task.CompletedTask;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="exceptionNotifier"></param>
    /// <param name="logger"></param>
    public AsyncTimer(IExceptionNotifier exceptionNotifier, ILogger<AsyncTimer> logger)
    {
        ExceptionNotifier = exceptionNotifier;
        Logger = logger;

        _taskTimer = new Timer(
            TimerCallBack,
            null,
            Timeout.Infinite,
            Timeout.Infinite
        );
    }

    /// <summary>
    ///     循环间隔时间
    /// </summary>
    public int Period { get; set; }

    /// <summary>
    ///     是否立即启动定时执行
    /// </summary>
    public bool RunOnStart { get; set; }

    /// <summary>
    ///     logger
    /// </summary>
    public ILogger<AsyncTimer> Logger { get; set; }

    /// <summary>
    ///     异常通知器
    /// </summary>
    public IExceptionNotifier ExceptionNotifier { get; set; }

    /// <summary>
    ///     开始定时执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="Exception"></exception>
    public void Start(CancellationToken cancellationToken = default)
    {
        if (Period <= 0) throw new Exception("Period should be set before starting the timer!");

        lock (_taskTimer)
        {
            _taskTimer.Change(RunOnStart ? 0 : Period, Timeout.Infinite);
            _isRunning = true;
        }
    }

    /// <summary>
    ///     停止定时执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void Stop(CancellationToken cancellationToken = default)
    {
        lock (_taskTimer)
        {
            _taskTimer.Change(Timeout.Infinite, Timeout.Infinite);
            while (_performingTasks) Monitor.Wait(_taskTimer);

            _isRunning = false;
        }
    }

    /// <summary>
    ///     任务回调
    /// </summary>
    /// <param name="state"></param>
    private void TimerCallBack(object state)
    {
        lock (_taskTimer)
        {
            if (!_isRunning || _performingTasks) return;

            _taskTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _performingTasks = true;
        }

        _ = Timer_Elapsed();
    }

    private async Task Timer_Elapsed()
    {
        try
        {
            await Elapsed(this);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "");
            await ExceptionNotifier?.NotifyAsync(new ExceptionNotificationContext(ex))!;
        }
        finally
        {
            lock (_taskTimer)
            {
                _performingTasks = false;
                if (_isRunning) _taskTimer.Change(Period, Timeout.Infinite);

                Monitor.Pulse(_taskTimer);
            }
        }
    }
}