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
                    return date;
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
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default;

            if (reader.TokenType == JsonTokenType.String)
            {
                if (DateTime.TryParse(reader.GetString() ?? string.Empty, out DateTime date))
                    return date;
            }
            return reader.GetDateTime();
        }

        /// <summary>
        /// 
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
    /// 长整型转换器
    /// </summary>
    public class LongConverter : JsonConverter<long>
    {
        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
