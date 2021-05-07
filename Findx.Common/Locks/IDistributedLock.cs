using System.Threading.Tasks;

namespace Findx.Locks
{
    /// <summary>
    /// 分布式锁
    /// </summary>
    public interface IDistributedLock : ILock
    {
        /// <summary>
        /// 刷新缓存锁续期
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        Task<bool> RefreshTimeToLiveAsync(string key, int seconds);
    }
}
