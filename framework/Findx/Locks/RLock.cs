using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.ExceptionHandling;
using Findx.Threading;

namespace Findx.Locks;

/// <summary>
///     续期锁
/// </summary>
public class RLock : IAsyncDisposable
{
    private readonly ILock _lock;

    private readonly ILogger<RLock> _logger;
    private readonly int _period;
    private readonly TimeSpan _timeUntilExpires;
    private bool _isReleased;

    private AsyncTimer _timer;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="lockId"></param>
    /// <param name="lock"></param>
    /// <param name="timeUntilExpires"></param>
    /// <param name="autoRenew"></param>
    /// <param name="period"></param>
    public RLock(string resource, string lockId, ILock @lock, TimeSpan timeUntilExpires, bool autoRenew = false, int period = 10000)
    {
        _lock = @lock;
        _period = period;
        _timeUntilExpires = timeUntilExpires;
        Renewal = autoRenew;

        Resource = resource;
        LockId = lockId;

        _logger = ServiceLocator.GetService<ILogger<RLock>>();

        // 开启自动续期
        if (autoRenew) RunAutoRenew();
    }

    /// <summary>
    ///     资源
    /// </summary>
    public string Resource { get; }

    /// <summary>
    ///     锁id
    /// </summary>
    public string LockId { get; }

    /// <summary>
    ///     续期次数
    /// </summary>
    public int RenewalCount { get; private set; }

    /// <summary>
    ///     是否续期
    /// </summary>
    public bool Renewal { get; }

    /// <summary>
    ///     释放类
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await ReleaseAsync();
    }

    /// <summary>
    ///     续期操作
    /// </summary>
    /// <param name="lockExtension"></param>
    public async Task RenewAsync(TimeSpan lockExtension)
    {
        await _lock.RenewAsync(Resource, LockId, lockExtension);

        RenewalCount++;
        
        _logger.LogDebug("the resource ({Resource}) lock is renewed {RenewalCount} times, and the current execution time is {Now}", Resource, RenewalCount, DateTime.Now);
    }

    /// <summary>
    ///     释放锁
    /// </summary>
    public async Task ReleaseAsync()
    {
        if (_isReleased)
            return;

        _isReleased = true;

        await _lock.ReleaseAsync(Resource, LockId);

        _timer?.Stop();
        _timer = null;
    }

    /// <summary>
    ///     执行定期续期
    /// </summary>
    public void RunAutoRenew()
    {
        _timer?.Stop();
        _timer = null;
        _timer = new AsyncTimer
        {
            Period = _period,
            Elapsed = Timer_Elapsed,
            RunOnStart = false
        };
        _timer.Start();
    }

    /// <summary>
    ///     定时续期方法
    /// </summary>
    /// <param name="timer"></param>
    private async Task Timer_Elapsed(AsyncTimer timer)
    {
        await RenewAsync(_timeUntilExpires);
    }
}