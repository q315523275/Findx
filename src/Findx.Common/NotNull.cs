using System;

namespace Findx
{
    /// <summary>
    /// 值Null检测
    /// <para>隐式运算符</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NotNull<T>
    {
        public NotNull(T value)
        {
            this.Value = value;
        }

        public T Value { get; set; }

        public static implicit operator NotNull<T>(T value)
        {
            if (value == null)
                throw new ArgumentNullException();

            return new NotNull<T>(value);
        }
    }
}
