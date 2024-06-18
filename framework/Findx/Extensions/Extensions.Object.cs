using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using Findx.Utilities;

namespace Findx.Extensions;

/// <summary>
///     系统扩展 - 对象
/// </summary>
public static partial class Extensions
{
    /// <summary>
    ///     安全转换为字符串，去除两端空格，当值为null时返回""
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string SafeString(this object input)
    {
        return input?.ToString()?.Trim() ?? string.Empty;
    }

    /// <summary>
    ///     安全获取值，当值为null时，不会抛出异常
    /// </summary>
    /// <param name="value">可空值</param>
    public static T SafeValue<T>(this T? value) where T : struct
    {
        return value ?? default;
    }

    /// <summary>
    ///     对象转换
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T As<T>(this object obj) where T : class
    {
        return (T)obj;
    }

    /// <summary>
    ///     使用<see cref="Convert.ChangeType(object, System.Type)" />方法将给定的对象转换为值类型。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T To<T>(this object obj)
    {
        if (obj == null) return default;

        var conversionType = typeof(T);

        if (conversionType.IsNullableType()) conversionType = conversionType.GetUnNullableType();

        if (conversionType.IsEnum) return (T)Enum.Parse(conversionType, obj.ToString() ?? string.Empty);

        if (conversionType == typeof(Guid))
            return (T)TypeDescriptor.GetConverter(conversionType).ConvertFromInvariantString(obj.ToString());

        return (T)Convert.ChangeType(obj, conversionType, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     把值类型转换为指定值类型
    /// </summary>
    /// <param name="value"></param>
    /// <param name="conversionType"></param>
    /// <returns></returns>
    public static object CastTo(this object value, Type conversionType)
    {
        if (value == null) 
            return null;
        
        if (conversionType.IsNullableType()) 
            conversionType = conversionType.GetUnNullableType();
        
        if (conversionType.IsEnum) 
            return Enum.Parse(conversionType, value.ToString() ?? string.Empty);
        
        if (conversionType == typeof(Guid)) 
            return Guid.Parse(value.ToString() ?? string.Empty);
        
        return Convert.ChangeType(value, conversionType);
    }

    /// <summary>
    ///     把值类型转换为指定值类型
    /// </summary>
    /// <typeparam name="T"> 动态类型 </typeparam>
    /// <param name="value"> 要转化的源对象 </param>
    /// <returns> 转化后的指定类型的对象，转化失败引发异常。 </returns>
    public static T CastTo<T>(this object value)
    {
        if (value == null) 
            return default;
        
        if (value.GetType() == typeof(T)) 
            return (T)value;
        
        var result = CastTo(value, typeof(T));
        
        return (T)result;
    }

    /// <summary>
    ///     把值类型转化为指定值类型，转化失败时返回指定的默认值
    /// </summary>
    /// <typeparam name="T"> 动态类型 </typeparam>
    /// <param name="value"> 要转化的源对象 </param>
    /// <param name="defaultValue"> 转化失败返回的指定默认值 </param>
    /// <returns> 转化后的指定类型对象，转化失败时返回指定的默认值 </returns>
    public static T CastTo<T>(this object value, T defaultValue)
    {
        try
        {
            return CastTo<T>(value);
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    /// <summary>
    ///     检查项是否存在于集合中
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
    ///     条件满足
    ///     执行委托
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="condition"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static T If<T>(this T obj, bool condition, Func<T, T> func)
    {
        return condition ? func(obj) : obj;
    }

    /// <summary>
    ///     条件满足
    ///     执行委托
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="condition"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static T If<T>(this T obj, bool condition, Action<T> action)
    {
        if (condition) action(obj);
        return obj;
    }

    /// <summary>
    ///     对象深度拷贝，复制出一个数据一样，但地址不一样的新版本
    ///     如实现了IMapper,请使用Mapper方式进行深拷贝
    /// </summary>
    [Obsolete("Obsolete")]
    public static T DeepClone<T>(this T obj) where T : class
    {
        if (obj == null) return default;
        if (!typeof(T).HasAttribute<SerializableAttribute>())
            throw new NotSupportedException($"当前对象未标记特性“{typeof(SerializableAttribute)}”，无法进行DeepClone操作");
        var formatter = new BinaryFormatter();
        using var ms = new MemoryStream();
        formatter.Serialize(ms, obj);
        ms.Seek(0L, SeekOrigin.Begin);
        return (T)formatter.Deserialize(ms);
    }
    
    /// <summary>
    ///     根据属性名获取属性值
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="t">对象</param>
    /// <param name="name">属性名</param>
    /// <returns>属性的值</returns>
    public static object GetPropertyValue<T>(this T t, string name)
    {
        var getValue = PropertyUtility.EmitGetter<T>(name);
        return getValue(t);
    }
}