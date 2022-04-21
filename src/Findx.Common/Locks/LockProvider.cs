using System;
using System.Collections.Generic;
using System.Linq;

namespace Findx.Locks
{
    public class LockProvider : ILockProvider
    {
        private readonly IDictionary<LockType, ILock> _locks;

        public LockProvider(IEnumerable<ILock> locks)
        {
            _locks = locks.ToDictionary(it => it.LockType, it => it); ;
        }

        public ILock Get(LockType lockType)
        {
            _locks.TryGetValue(lockType, out var _lock);

            Check.NotNull(_lock, nameof(_lock));

            return _lock;
        }
    }
}

