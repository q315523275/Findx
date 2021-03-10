using System.Threading;

namespace Findx.Utils
{
    public class AtomicBoolean
    {
        private volatile int _value;

        public AtomicBoolean()
            : this(false)
        {
        }

        public AtomicBoolean(bool value)
        {
            Value = value;
        }

        public bool Value
        {
            get => _value != 0;

            set => _value = value ? 1 : 0;
        }

        public bool CompareAndSet(bool expected, bool update)
        {
            var expectedInt = expected ? 1 : 0;
            var updateInt = update ? 1 : 0;
            return Interlocked.CompareExchange(ref _value, updateInt, expectedInt) == expectedInt;
        }
    }
}
