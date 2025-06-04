using System.ComponentModel;
using Findx.Extensions;

namespace Findx.Utilities;

/// <summary>
///     枚举工具类
/// </summary>
/// <typeparam name="T"></typeparam>
public static class EnumUtility<T> where T : Enum
{
    private static readonly ConcurrentDictionary<T, EnumMetadata> Cache = new();

    static EnumUtility()
    {
        InitializeCache();
    }

    private static void InitializeCache()
    {
        var type = typeof(T);
        foreach (T value in Enum.GetValues(type))
        {
            var memberInfo = type.GetMember(value.ToString())[0];
            var descriptionAttribute = memberInfo.GetAttribute<DescriptionAttribute>();
            
            Cache[value] = new EnumMetadata(value.ToString(), value.CastTo<int>(), descriptionAttribute?.Description ?? value.ToString());
        }
    }

    /// <summary>
    ///     获取枚举名称
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetName(T value) => Cache[value].Name;
    
    /// <summary>
    ///     获取枚举数值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int GetNumericValue(T value) => Cache[value].NumericValue;
    
    /// <summary>
    ///     获取枚举描述
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetDescription(T value) => Cache[value].Description;

    /// <summary>
    ///     根据枚举名称转换
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T Parse(string name) => (T)Enum.Parse(typeof(T), name, ignoreCase: true);

    /// <summary>
    ///     根据枚举数值转换
    /// </summary>
    /// <param name="numericValue"></param>
    /// <returns></returns>
    public static T FromNumericValue(int numericValue) => (T)Enum.ToObject(typeof(T), numericValue);

    /// <summary>
    ///     枚举元数据
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="NumericValue"></param>
    /// <param name="Description"></param>
    private record EnumMetadata(string Name, int NumericValue, string Description);
}
