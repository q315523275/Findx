using System;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Locks
{
    public class AutomaticRenewalLock : IRedLock, IDisposable
    {
        private readonly IDistributedLock _distributedLock;
        private Timer _timer;
        private readonly string _lockKey;
        private readonly string _lockValue;
        private readonly int _seconds;
        private readonly int _refreshSeconds;
        private bool _polling;
        public AutomaticRenewalLock(IDistributedLock distributedLock, string lockKey, int seconds = 60)
        {
            Check.NotNull(distributedLock, nameof(distributedLock));
            Check.NotNull(lockKey, nameof(lockKey));

            _distributedLock = distributedLock;
            _lockKey = lockKey;
            _seconds = seconds;
            _lockValue = Guid.NewGuid().ToString();
            _refreshSeconds = _seconds / 2 * 1000;

            _timer = new Timer(async x =>
            {
                if (_polling) return;

                _polling = true;
                await RefreshTimeToLiveAsync();
                _polling = false;

            }, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
        }

        public async Task<bool> RefreshTimeToLiveAsync()
        {
            // Console.WriteLine($"分布式锁{_lockKey}进行自动续期,时长{_seconds}秒,时间{DateTime.Now}");

            var result = await _distributedLock.RefreshTimeToLiveAsync(_lockKey, TimeSpan.FromSeconds(_seconds));

            return result;
        }

        public bool TryLock()
        {
            var res = _distributedLock.LockTake(_lockKey, _lockValue, TimeSpan.FromSeconds(_seconds));
            if (res)
                _timer?.Change(_refreshSeconds, _refreshSeconds);
            else
                _timer?.Dispose();
            return res;
        }

        public async Task<bool> TryLockAsync()
        {
            var res = await _distributedLock.LockTakeAsync(_lockKey, _lockValue, TimeSpan.FromSeconds(_seconds));
            if (res)
                _timer?.Change(_refreshSeconds, _refreshSeconds);
            else
                _timer?.Dispose();
            return res;
        }

        public bool UnLock()
        {
            var res = _distributedLock.LockRelease(_lockKey, _lockValue);
            if (res) _timer?.Dispose();
            return res;
        }

        public async Task<bool> UnLockAsync()
        {
            var res = await _distributedLock.LockReleaseAsync(_lockKey, _lockValue);
            if (res) _timer?.Dispose();
            return res;
        }
    }
}
