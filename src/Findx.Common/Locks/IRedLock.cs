using System.Threading.Tasks;

namespace Findx.Locks
{
    /// <summary>
    /// 分布式锁对象
    /// </summary>
    public interface IRedLock
    {
        /// <summary>
        /// 锁
        /// </summary>
        /// <returns></returns>
        bool TryLock();
        /// <summary>
        /// 锁
        /// </summary>
        /// <returns></returns>
        Task<bool> TryLockAsync();
        /// <summary>
        /// 释放锁
        /// </summary>
        /// <returns></returns>
        bool UnLock();
        /// <summary>
        /// 释放锁
        /// </summary>
        /// <returns></returns>
        Task<bool> UnLockAsync();
    }
}
