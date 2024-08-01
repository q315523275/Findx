using Findx.Serialization;

namespace Findx.Extensions;

/// <summary>
///     系统扩展 - JSON序列化
/// </summary>
public partial class Extensions
{
    /// <summary>
    ///     将对象转换为json格式的字符串
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj"></param>
    /// <returns>json格式的字符串</returns>
    public static string ToJson<T>(this T obj)
    {
        return JsonSerializer.Serialize(obj, SystemTextJsonStringSerializer.Options);
    }

    /// <summary>
    ///     将json格式的字符串转为指定对象
    ///     如果json格式字符串格式不对则抛异常
    /// </summary>
    /// <typeparam name="T">要转换的对象类型</typeparam>
    /// <param name="json">json格式字符串</param>
    /// <returns>指定对象的实例</returns>
    public static T ToObject<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, SystemTextJsonStringSerializer.Options);
    }

    /// <summary>
    ///     将json格式的字符串转为指定对象
    ///     如果json格式字符串格式不对则抛异常
    /// </summary>
    /// <param name="json">json格式字符串</param>
    /// <param name="type">要转换的对象类型</param>
    /// <returns>指定对象的实例</returns>
    public static object ToObject(this string json, Type type)
    {
        return JsonSerializer.Deserialize(json, type, SystemTextJsonStringSerializer.Options);
    }
    
    /// <summary>
    ///     判断是否为json字符串
    /// </summary>
    /// <param name="input">字符串</param>
    /// <returns></returns>
    public static bool IsJson(this string input)
    {
        try
        {
            JsonDocument.Parse(input);
            return true;
        }
        catch
        {
            return false;
        }
    }

}