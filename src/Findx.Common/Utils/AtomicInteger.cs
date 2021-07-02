using System.Threading;

namespace Findx.Utils
{
    /// <summary>
    /// 原子整型对象
    /// </summary>
    public class AtomicInteger
    {
        protected volatile int _value;

        public AtomicInteger()
            : this(0)
        {
        }

        public AtomicInteger(int value)
        {
            _value = value;
        }

        public int Value
        {
            get => _value;

            set => _value = value;
        }

        public bool CompareAndSet(int expected, int update)
        {
            return Interlocked.CompareExchange(ref _value, update, expected) == expected;
        }

        public int IncrementAndGet()
        {
            return Interlocked.Increment(ref _value);
        }

        public int DecrementAndGet()
        {
            return Interlocked.Decrement(ref _value);
        }

        public int GetAndIncrement()
        {
            return Interlocked.Increment(ref _value) - 1;
        }

        public int AddAndGet(int value)
        {
            return Interlocked.Add(ref _value, value);
        }
    }
}
