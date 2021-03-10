using Findx.DependencyInjection;
using Findx.Serialization;

namespace Findx.Extensions
{
    /// <summary>
    /// 系统扩展 - JSON序列化
    /// </summary>
    public partial class Extensions
    {
        /// <summary>
        /// 将对象转换为json格式的字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj"></param>
        /// <returns>json格式的字符串</returns>
        public static string ToJson<T>(this T obj, bool camelCase = true)
        {
            var serializer = ServiceLocator.GetService<IJsonSerializer>();
            return serializer?.Serialize(obj, camelCase);
        }

        /// <summary>
        /// 将json格式的字符串转为指定对象
        /// 如果json格式字符串格式不对则抛异常
        /// </summary>
        /// <typeparam name="T">要转换的对象类型</typeparam>
        /// <param name="json">json格式字符串</param>
        /// <returns>指定对象的实例</returns>
        public static T ToObject<T>(this string json, bool camelCase = true)
        {
            var serializer = ServiceLocator.GetService<IJsonSerializer>();
            return serializer.Deserialize<T>(json, camelCase);
        }
    }
}
