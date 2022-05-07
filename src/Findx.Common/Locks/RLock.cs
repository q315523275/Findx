using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.ExceptionHandling;
using Findx.Threading;
using Microsoft.Extensions.Logging;

namespace Findx.Locks
{
	public class RLock: IAsyncDisposable
	{
        private readonly object _lockObj = new();
        private readonly ILock _lock;
        private readonly TimeSpan? _timeUntilExpires;

        private FindxAsyncTimer _timer;
        private int _renewalCount = 0;
        private bool _isRenewal;
        private int _period;
        private bool _isReleased;

        public RLock(string resource, string lockId, ILock @lock, TimeSpan? timeUntilExpires, bool autoRenew = false, int period = 10000)
        {
            _lock = @lock;
            _period = period;
            _timeUntilExpires = timeUntilExpires;
            _isRenewal = autoRenew;

            Resource = resource;
            LockId = lockId;

            // 开启自动续期
            if (autoRenew)
            {
                _timer = new FindxAsyncTimer(ServiceLocator.GetService<IExceptionNotifier>(), ServiceLocator.GetService<ILogger<FindxAsyncTimer>>())
                {
                    Period = period,
                    Elapsed = Timer_Elapsed,
                    RunOnStart = false
                };
                _timer.Start();
            }
        }

        public string Resource { get; }

        public string LockId { get; }

        public int RenewalCount => _renewalCount;

        public bool Renewal => _isRenewal;

        public async Task RenewAsync(TimeSpan? lockExtension = null)
        {
            await _lock.RenewAsync(Resource, LockId, lockExtension);

            _renewalCount++;

            Debug.WriteLine($"the resource ({Resource}) lock ({_lock.LockType}) is renewed {_renewalCount} times, and the current execution time is {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        public async Task ReleaseAsync()
        {
            
            if (_isReleased)
                return;

            _isReleased = true;

            await _lock.ReleaseAsync(Resource, LockId);

            _timer?.Stop();
            _timer = null;
        }

        public async ValueTask DisposeAsync()
        {
            await ReleaseAsync();
        }

        public void RunAutoRenew()
        {
            _timer?.Stop();
            _timer = null;
            _timer = new FindxAsyncTimer(ServiceLocator.GetService<IExceptionNotifier>(), ServiceLocator.GetService<ILogger<FindxAsyncTimer>>())
            {
                Period = _period,
                Elapsed = Timer_Elapsed,
                RunOnStart = false
            };
            _timer.Start();
        }

        protected async Task Timer_Elapsed(FindxAsyncTimer timer)
        {
            await RenewAsync(_timeUntilExpires);
        }
    }
}

