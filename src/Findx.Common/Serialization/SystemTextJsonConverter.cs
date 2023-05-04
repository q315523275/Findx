using System.Text.Json.Serialization;

namespace Findx.Serialization;

/// <summary>
///     DateTime时间格式转换器
/// </summary>
public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    /// <summary>
    ///     读取时间
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null or JsonTokenType.None) return default;

        if (reader.TokenType == JsonTokenType.String)
            return DateTime.TryParse(reader.GetString(), out var dateTime) ? dateTime : default;

        return reader.GetDateTime();
    }

    /// <summary>
    ///     写入字符串
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value is { Hour: 0, Minute: 0, Second: 0 }
            ? value.ToString("yyyy-MM-dd")
            : value.ToString("yyyy-MM-dd HH:mm:ss"));
    }
}

/// <summary>
///     DateTime时间格式转换器
/// </summary>
public class DateTimeNullableJsonConverter : JsonConverter<DateTime?>
{
    /// <summary>
    ///     读取时间
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null or JsonTokenType.None) return default;

        if (reader.TokenType == JsonTokenType.String)
            return DateTime.TryParse(reader.GetString(), out var dateTime) ? dateTime : default;

        return reader.GetDateTime();
    }

    /// <summary>
    ///     写入字符
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value is { Hour: 0, Minute: 0, Second: 0 }
                ? value.Value.ToString("yyyy-MM-dd")
                : value.Value.ToString("yyyy-MM-dd HH:mm:ss"));
        else
            writer.WriteNullValue();
    }
}

/// <summary>
///     长整型字符串转换器
/// </summary>
public class LongStringJsonConverter : JsonConverter<long>
{
    /// <summary>
    ///     读取json
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null or JsonTokenType.None) return default;

        if (reader.TokenType == JsonTokenType.String)
            return long.TryParse(reader.GetString(), out var longValue) ? longValue : default;

        return reader.GetInt64();
    }

    /// <summary>
    ///     写入json
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

/// <summary>
///     Decimal转换器
///     <para>处理字符类型转换</para>
/// </summary>
public class DecimalNullableJsonConverter : JsonConverter<decimal?>
{
    /// <summary>
    ///     读取json
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null or JsonTokenType.None) return default;

        if (reader.TokenType == JsonTokenType.String)
            return decimal.TryParse(reader.GetString(), out var decimalValue) ? decimalValue : default;

        return reader.GetDecimal();
    }

    /// <summary>
    ///     写入json
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(value.Value);
        else
            writer.WriteNullValue();
    }
}