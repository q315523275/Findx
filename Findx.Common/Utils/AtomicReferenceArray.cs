namespace Findx.Utils
{
    public class AtomicReferenceArray<T>
    {
        private readonly T[] _array;

        public AtomicReferenceArray(int length)
        {
            _array = new T[length];
        }

        public T this[int index]
        {
            get
            {
                lock (_array)
                {
                    return _array[index];
                }
            }

            set
            {
                lock (_array)
                {
                    _array[index] = value;
                }
            }
        }

        public T[] ToArray()
        {
            lock (_array)
            {
                return (T[])_array.Clone();
            }
        }

        public int Length
        {
            get
            {
                lock (_array)
                {
                    return _array.Length;
                }
            }
        }
    }
}
