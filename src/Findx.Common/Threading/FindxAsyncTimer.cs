using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.ExceptionHandling;
using Microsoft.Extensions.Logging;

namespace Findx.Threading
{
    public class FindxAsyncTimer : Findx.DependencyInjection.ITransientDependency
    {
        public Func<FindxAsyncTimer, Task> Elapsed = _ => Task.CompletedTask;

        public int Period { get; set; }

        public bool RunOnStart { get; set; }

        public ILogger<FindxAsyncTimer> Logger { get; set; }

        public IExceptionNotifier ExceptionNotifier { get; set; }

        private readonly Timer _taskTimer;
        private volatile bool _performingTasks;
        private volatile bool _isRunning;

        public FindxAsyncTimer(IExceptionNotifier exceptionNotifier, ILogger<FindxAsyncTimer> logger)
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

        public void Start(CancellationToken cancellationToken = default)
        {
            if (Period <= 0)
            {
                throw new Exception("Period should be set before starting the timer!");
            }

            lock (_taskTimer)
            {
                _taskTimer.Change(RunOnStart ? 0 : Period, Timeout.Infinite);
                _isRunning = true;
            }
        }

        public void Stop(CancellationToken cancellationToken = default)
        {
            lock (_taskTimer)
            {
                _taskTimer.Change(Timeout.Infinite, Timeout.Infinite);
                while (_performingTasks)
                {
                    Monitor.Wait(_taskTimer);
                }

                _isRunning = false;
            }
        }


        private void TimerCallBack(object state)
        {
            lock (_taskTimer)
            {
                if (!_isRunning || _performingTasks)
                {
                    return;
                }

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
                Logger.LogError(ex, "");
                await ExceptionNotifier.NotifyAsync(new ExceptionNotificationContext(ex));
            }
            finally
            {
                lock (_taskTimer)
                {
                    _performingTasks = false;
                    if (_isRunning)
                    {
                        _taskTimer.Change(Period, Timeout.Infinite);
                    }

                    Monitor.Pulse(_taskTimer);
                }
            }
        }
    }
}

