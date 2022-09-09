using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Findx.Serialization
{
    /// <summary>
    /// DateTime时间格式转换器
    /// </summary>
    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (DateTime.TryParse(reader.GetString(), out DateTime date))
                {
                    return date;
                }
            }
            return reader.GetDateTime();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            if (value.Hour == 0 && value.Minute == 0 && value.Second == 0)
            {
                writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
            }
            else
            {
                writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
    }

    /// <summary>
    /// DateTime时间格式转换器
    /// </summary>
    public class DateTimeNullableJsonConverter : JsonConverter<DateTime?>
    {
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                if (DateTime.TryParse(reader.GetString(), out DateTime date))
                {
                    return date;
                }
            }
            return reader.GetDateTime();
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value?.Hour == 0 && value?.Minute == 0 && value?.Second == 0)
            {
                writer.WriteStringValue(value?.ToString("yyyy-MM-dd"));
            }
            else
            {
                writer.WriteStringValue(value?.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
    }

    /// <summary>
    /// 长整型字符串转换器
    /// </summary>
    public class LongStringJsonConverter : JsonConverter<long>
    {
        /// <summary>
        /// 读取json
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return long.Parse(reader.GetString());
        }

        /// <summary>
        /// 写入json
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
    /// Decimal转换器
    /// <para>处理字符类型转换</para>
    /// </summary>
    public class DecimalNullableJsonConverter : JsonConverter<decimal?>
    {
        /// <summary>
        /// 读取json
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }
            if (reader.TokenType == JsonTokenType.String)
            {
                if (Decimal.TryParse(reader.GetString(), out decimal v))
                {
                    return v;
                }
                else
                {
                    return default;
                }
            }
            return reader.GetDecimal();
        }

        /// <summary>
        /// 写入json
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}
