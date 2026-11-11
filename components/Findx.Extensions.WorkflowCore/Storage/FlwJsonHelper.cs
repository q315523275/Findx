using System.Text.Encodings.Web;
using System.Text.Json;

namespace Findx.Extensions.WorkflowCore.Storage;

/// <summary>
///     流程 JSON 序列化工具：变量以 Dictionary&lt;string, object&gt; 存储，读回时把 JsonElement 归一为 CLR 值
/// </summary>
public static class FlwJsonHelper
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    /// <summary>
    ///     序列化
    /// </summary>
    public static string Serialize(object value)
    {
        return value == null ? null : JsonSerializer.Serialize(value, Options);
    }

    /// <summary>
    ///     反序列化为 CLR 字典（JsonElement 递归归一）
    /// </summary>
    public static Dictionary<string, object> DeserializeVariables(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, object>();

        Dictionary<string, JsonElement> source;
        try
        {
            source = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, Options);
        }
        catch (JsonException)
        {
            return new Dictionary<string, object>();
        }

        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        if (source == null) return result;
        foreach (var item in source) result[item.Key] = ToClrValue(item.Value);
        return result;
    }

    /// <summary>
    ///     JsonElement 转 CLR 值
    /// </summary>
    public static object ToClrValue(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var map = new Dictionary<string, object>(StringComparer.Ordinal);
                foreach (var property in element.EnumerateObject()) map[property.Name] = ToClrValue(property.Value);
                return map;
            case JsonValueKind.Array:
                return element.EnumerateArray().Select(ToClrValue).ToList();
            case JsonValueKind.String:
                var text = element.GetString();
                //  时间字符串保持字符串形态，避免条件比较时被误转数值
                return text;
            case JsonValueKind.Number:
                return element.TryGetDecimal(out var number) ? number : (object)element.GetDouble();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            default:
                return null;
        }
    }
}
