using System.Threading.Tasks;

namespace Findx.Locks
{
    public interface IRedLock
    {
        bool TryLock();
        Task<bool> TryLockAsync();
        bool UnLock();
        Task<bool> UnLockAsync();
    }
}
