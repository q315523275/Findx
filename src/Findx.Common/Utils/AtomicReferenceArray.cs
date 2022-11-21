namespace Findx.Utils
{
    /// <summary>
    /// 原子数组对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AtomicReferenceArray<T>
    {
        private readonly T[] _array;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="length"></param>
        public AtomicReferenceArray(int length)
        {
            _array = new T[length];
        }

        /// <summary>
        /// 获取或设置集合成员
        /// </summary>
        /// <param name="index"></param>
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

        /// <summary>
        /// 转换为集合
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            lock (_array)
            {
                return (T[])_array.Clone();
            }
        }

        /// <summary>
        /// 集合长度
        /// </summary>
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
