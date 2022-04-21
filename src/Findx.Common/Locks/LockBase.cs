using System;
using System.Threading;
using System.Threading.Tasks;
using Findx.Extensions;

namespace Findx.Locks
{
    public abstract class LockBase : ILock
    {
        /// <summary>
        /// 申请锁
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="timeUntilExpires"></param>
        /// <param name="isWait"></param>
        /// <param name="renew"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<RLock> AcquireAsync(string resource, TimeSpan? timeUntilExpires = null, bool isWait = false, bool renew = false, CancellationToken cancellationToken = default)
        {
            if (!timeUntilExpires.HasValue)
                timeUntilExpires = TimeSpan.FromSeconds(30);

            DateTime startTime = DateTime.Now;
            bool gotLock = false;
            string lockId = GenerateNewLockId();
            do
            {
                try
                {
                    gotLock = await TryLockAsync(resource, lockId, timeUntilExpires);
                }
                catch { }

                // 拿到锁
                // 线程取消
                // 不需要等待争抢锁
                if (gotLock || cancellationToken.IsCancellationRequested || !isWait)
                    break;

                // 循环相关计算
                var delayAmount = timeUntilExpires.Value;

                // delay a minimum of 50ms
                if (delayAmount < TimeSpan.FromMilliseconds(50))
                    delayAmount = TimeSpan.FromMilliseconds(50);

                // delay a maximum of 3 seconds
                if (delayAmount > TimeSpan.FromSeconds(3))
                    delayAmount = TimeSpan.FromSeconds(3);

                // 到达最大等待时间
                if (startTime.Add(timeUntilExpires.Value) >= DateTime.Now.Add(-delayAmount))
                    break;

                await Task.Delay(delayAmount);

                Thread.Yield();

            } while (!cancellationToken.IsCancellationRequested);

            if (!gotLock)
                return null;

            return new RLock(resource, lockId, this, timeUntilExpires, autoRenew: renew, period: timeUntilExpires.Value.TotalMilliseconds.To<int>() / 3);
        }
        protected string GenerateNewLockId()
        {
            return Guid.NewGuid().ToString();
        }
        protected string GenerateNewLockKey(string key)
        {
            return $"lock:{key}";
        }


        public abstract LockType LockType { get; }
        public abstract Task<bool> TryLockAsync(string resource, string lockId, TimeSpan? timeUntilExpires = null);
        public abstract Task ReleaseAsync(string resource, string lockId);
        public abstract Task RenewAsync(string resource, string lockId, TimeSpan? timeUntilExpires = null);
    }
}

