using System;
using System.Buffers.Text;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Findx.Common;
using Findx.Extensions;
using Findx.Serialization;

namespace Findx.RabbitMQ;

/// <summary>
///     默认序列化工具
/// </summary>
public class DefaultRabbitMqSerializer : IRabbitMqSerializer
{
    /// <summary>
    ///     序列化
    /// </summary>
    /// <param name="obj"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public byte[] Serialize<T>(T obj)
    {
        Check.NotNull(obj, nameof(obj));
        
        var type= typeof(T);

        // 快速路径：byte[] 直接返回
        if (type == typeof(byte[]))
        {
            return obj.As<byte[]>();
        }

        // 快速路径：字符串处理
        if (type == typeof(string))
        {
            return obj.As<string>().ToBytes();
        }

        // 处理原始数值类型和decimal
        if (IsPrimitiveOrDecimal(type))
        {
            return obj.ToString().ToBytes();
        }

        // 其他类型使用JSON反序列化
        return JsonSerializer.SerializeToUtf8Bytes(obj, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions());
    }

    /// <summary>
    ///     反序列化
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public object Deserialize(ReadOnlyMemory<byte> value, Type type)
    {
        if (type.IsNullableType())
        {
            type = type.GetUnNullableType();
        }

        // 处理字符串类型
        if (type == typeof(string))
        {
            return GetStringFromBytes(value.Span);
        }
        
        // 处理字节数组类型
        if (type == typeof(byte[]))
        {
            return value.ToArray();
        }

        // 处理原始数值类型和decimal
        if (IsPrimitiveOrDecimal(type))
        {
            return HandlePrimitiveFromBytes(value.Span, type);
        }

        // 其他类型使用JSON反序列化
        return JsonSerializer.Deserialize(value.Span, type, SystemTextJsonSerializerOptions.CreateJsonSerializerOptions());
    }
    
    /// <summary>
    ///     转字符串
    /// </summary>
    /// <param name="span"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetStringFromBytes(ReadOnlySpan<byte> span)
    {
        // 小字符串栈分配优化
        if (span.Length <= 256)
        {
            Span<char> chars = stackalloc char[span.Length];
            var count = Encoding.UTF8.GetChars(span, chars);
            return new string(chars[..count]);
        }
        return Encoding.UTF8.GetString(span);
    }
    
    /// <summary>
    ///     值类型转换
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="typeCode"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object ConvertFromString(string value, Type targetType, TypeCode typeCode)
    {
        return typeCode switch
        {
            TypeCode.Decimal => decimal.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture),
            TypeCode.Int64 => long.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture),
            TypeCode.Single => float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture),
            _ => Convert.ChangeType(value, typeCode, CultureInfo.InvariantCulture)
        };
    }
    
    /// <summary>
    ///     值类型判断
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static bool IsPrimitiveOrDecimal(Type type)
    {
        return type.IsPrimitive || type == typeof(decimal);
    }
    
    private static readonly ConcurrentDictionary<Type, TypeCode> TypeCodeCache = new();
    
    /// <summary>
    ///     值类型转换
    /// </summary>
    /// <param name="span"></param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="InvalidCastException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object HandlePrimitiveFromBytes(ReadOnlySpan<byte> span, Type targetType)
    {
        try
        {
            var typeCode = TypeCodeCache.GetOrAdd(targetType, Type.GetTypeCode);

            // 优先使用Utf8Parser直接解析二进制
            return typeCode switch
            {
                // 二进制直接转换
                // TypeCode.Int32 when span.Length >= sizeof(int) => 
                //     BitConverter.ToInt32(span),
                // TypeCode.Double when span.Length >= sizeof(double) => 
                //     BitConverter.ToDouble(span),
                // TypeCode.Boolean when span.Length >= sizeof(bool) => 
                //     BitConverter.ToBoolean(span),

                // Utf8Parser优化路径
                TypeCode.Int64 when Utf8Parser.TryParse(span, out long longVal, out _) => 
                    longVal,
                TypeCode.Decimal when Utf8Parser.TryParse(span, out decimal decimalVal, out _) => 
                    decimalVal,
                TypeCode.Single when Utf8Parser.TryParse(span, out float floatVal, out _) => 
                    floatVal,
                
                // 字符串解析回退
                _ => ConvertFromString(
                    Encoding.UTF8.GetString(span), 
                    targetType, 
                    typeCode)
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to convert {span.Length} bytes to {targetType.Name}. " +
                $"Hex: {BitConverter.ToString(span.ToArray())}", ex);
        }
    }
}