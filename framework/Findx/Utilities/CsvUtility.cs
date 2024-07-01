using System.Globalization;
using Findx.Extensions;

namespace Findx.Utilities;

/// <summary>
///     Csv数据转换工具类 - 简单版
/// </summary>
public static class CsvUtility
{
    /// <summary>
    ///     读取Csv数据
    /// </summary>
    /// <typeparam name="T">属性顺序需与csv列顺序一致</typeparam>
    /// <param name="stream"></param>
    /// <param name="skipFirstLine"></param>
    /// <param name="csvDelimiter"></param>
    /// <returns></returns>
    public static IList<T> ReadCsvStream<T>(Stream stream, bool skipFirstLine = true, string csvDelimiter = ",") where T : new()
    {
        // 可以增加映射实现自动匹配属性名,无需现在必须顺序一致
        // Todo 使用Emit或者ExpressionTree进行性能优化
        
        var records = new List<T>();
        var item = new T();
        var properties = item.GetType().GetProperties();
        var csvDelimiters = csvDelimiter.ToCharArray();
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line == null) continue;
            var values = line.Split(csvDelimiters);
            if (skipFirstLine)
            {
                skipFirstLine = false;
            }
            else
            {
                for (var i = 0; i < values.Length; i++)
                {
                    properties[i].SetValue(item, Convert.ChangeType(values[i], properties[i].PropertyType, CultureInfo.CurrentCulture), null);
                }
                records.Add(item);
            }
        }

        return records;
    }

    /// <summary>
    ///     导出Csv数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="includeHeader"></param>
    /// <param name="csvDelimiter"></param>
    /// <returns></returns>
    public static string ExportCsv<T>(IList<T> data, bool includeHeader = true, string csvDelimiter = ",")
    {
        // Todo 使用Emit或者ExpressionTree进行性能优化
        
        var type = data.GetType();
        Type itemType;

        if (type.GetGenericArguments().Length > 0)
            itemType = type.GetGenericArguments()[0];
        else
            itemType = type.GetElementType();

        var csvDelimiterLen = csvDelimiter.Length;
        
        using var stringWriter = new StringWriter();
        if (includeHeader)
            stringWriter.WriteLine(string.Join<string>(csvDelimiter, itemType.GetProperties().Select(x => x.Name)));
        
        using var psb = Pool.StringBuilder.Get(out var sb);
        foreach (var obj in data)
        {
            var values = obj.GetType().GetProperties().Select(pi => new { Value = pi.GetValue(obj, null) });
            foreach (var val in values)
            {
                if (val.Value != null)
                {
                    var escapeVal = val.Value.ToString();
                    // ReSharper disable once PossibleNullReferenceException
                    if (escapeVal.Contains(',')) 
                        escapeVal = string.Concat("\"", escapeVal, "\"");

                    if (escapeVal.Contains('\r', StringComparison.OrdinalIgnoreCase))
                        escapeVal = escapeVal.ReplaceFirst("\r", " ", StringComparison.OrdinalIgnoreCase);

                    if (escapeVal.Contains('\n', StringComparison.OrdinalIgnoreCase))
                        escapeVal = escapeVal.ReplaceFirst("\n", " ", StringComparison.OrdinalIgnoreCase);

                    sb.Append(escapeVal).Append(csvDelimiter);
                }
                else
                {
                    sb.Append(string.Empty).Append(csvDelimiter);
                }
            }

            sb.Remove(sb.Length - csvDelimiterLen, csvDelimiterLen);
            stringWriter.WriteLine(sb.ToString());
            sb.Clear();
        }
        return stringWriter.ToString();
    }
}