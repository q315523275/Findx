﻿using System.Threading;

namespace Findx.Utils
{
    /// <summary>
    /// 原子对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AtomicReference<T>
            where T : class
    {
        private volatile T _value;

        public AtomicReference()
            : this(default(T))
        {
        }

        public AtomicReference(T value)
        {
            _value = value;
        }

        public T Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }

        public bool CompareAndSet(T expected, T update)
        {
            return Interlocked.CompareExchange(ref _value, update, expected) == expected;
        }

        public T GetAndSet(T value)
        {
            return Interlocked.Exchange(ref _value, value);
        }
    }
}
