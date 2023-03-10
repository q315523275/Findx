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

        /// <summary>
        /// Ctor
        /// </summary>
        public AtomicReference()
            : this(default(T))
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="value"></param>
        public AtomicReference(T value)
        {
            _value = value;
        }

        /// <summary>
        /// 结果值
        /// </summary>
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

        /// <summary>
        /// 比较入参与原有值，相同则进行替换
        /// </summary>
        /// <param name="expected">比较值</param>
        /// <param name="update">替换更新值</param>
        /// <returns>替换更新结果</returns>
        public bool CompareAndSet(T expected, T update)
        {
            return Interlocked.CompareExchange(ref _value, update, expected) == expected;
        }

        /// <summary>
        /// 获取原有值并设置新值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public T GetAndSet(T value)
        {
            return Interlocked.Exchange(ref _value, value);
        }
    }
}
