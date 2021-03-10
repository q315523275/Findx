using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Findx.Extensions
{
    /// <summary>
    /// 系统扩展 - 对象
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 安全转换为字符串，去除两端空格，当值为null时返回""
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SafeString(this object input)
        {
            return input?.ToString().Trim() ?? string.Empty;
        }

        /// <summary>
        /// 安全获取值，当值为null时，不会抛出异常
        /// </summary>
        /// <param name="value">可空值</param>
        public static T SafeValue<T>(this T? value) where T : struct
        {
            return value ?? default;
        }

        /// <summary>
        /// 对象转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T As<T>(this object obj) where T : class
        {
            return (T)obj;
        }

        /// <summary>
        /// 使用<see cref ="Convert.ChangeType(object，System.Type)" />方法将给定的对象转换为值类型。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T To<T>(this object obj)
        {
            if (obj == null)
            {
                return default;
            }

            Type conversionType = typeof(T);

            if (conversionType.IsNullableType())
            {
                conversionType = conversionType.GetUnNullableType();
            }

            if (conversionType.IsEnum)
            {
                return (T)Enum.Parse(conversionType, obj.ToString());
            }

            if (conversionType == typeof(Guid))
            {
                return (T)TypeDescriptor.GetConverter(conversionType).ConvertFromInvariantString(obj.ToString());
            }

            return (T)Convert.ChangeType(obj, conversionType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 检查项是否存在于集合中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsIn<T>(this T item, params T[] list)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// 条件满足
        /// 执行委托
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="condition"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static T If<T>(this T obj, bool condition, Func<T, T> func)
        {
            if (condition)
            {
                return func(obj);
            }

            return obj;
        }

        /// <summary>
        /// 条件满足
        /// 执行委托
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="condition"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static T If<T>(this T obj, bool condition, Action<T> action)
        {
            if (condition)
            {
                action(obj);
            }

            return obj;
        }

        /// <summary>
        /// 对象深度拷贝，复制出一个数据一样，但地址不一样的新版本
        /// </summary>
        public static T DeepClone<T>(this T obj) where T : class
        {
            if (obj == null)
            {
                return default;
            }
            if (typeof(T).HasAttribute<SerializableAttribute>())
            {
                throw new NotSupportedException(string.Format("当前对象未标记特性“{0}”，无法进行DeepClone操作", typeof(SerializableAttribute)));
            }
            BinaryFormatter formatter = new BinaryFormatter();
            using MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, obj);
            ms.Seek(0L, SeekOrigin.Begin);
            return (T)formatter.Deserialize(ms);
        }
    }
}
