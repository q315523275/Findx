using System.Threading.Tasks;
using Findx.Data;
using Findx.Extensions;
using Findx.Linq;

namespace Findx.Utilities;

/// <summary>
///     Csv数据转换工具类 - 简化版
/// </summary>
public static class CsvUtility
{
    /// <summary>
    ///     读取Csv数据
    /// </summary>
    /// <typeparam name="T">属性顺序需与csv列顺序一致</typeparam>
    /// <param name="path"></param>
    /// <param name="skipFirstLine"></param>
    /// <param name="csvDelimiter"></param>
    /// <returns></returns>
    public static IEnumerable<T> ReadCsv<T>(string path, bool skipFirstLine = true, string csvDelimiter = ",") where T : class, new()
    {
        var entityType = typeof(T);
        var properties = SingletonDictionary<Type, PropertyInfo[]>.Instance.GetOrAdd(entityType, () => entityType.GetProperties());
        var csvDelimiters = csvDelimiter.ToCharArray();
        using var reader = new StreamReader(path);
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line == null) continue;
            
            var values = line.Split(csvDelimiters);
            if (properties.Length != values.Length) continue;
            
            if (skipFirstLine)
            {
                skipFirstLine = false;
            }
            else
            {
                var model = new T();
                for (var i = 0; i < values.Length; i++)
                    PropertyValueSetter<T>.SetPropertyValueObject(entityType, model, properties[i].Name, values[i].CastTo(properties[i].PropertyType));
                yield return model;
            }
        }
    }
    
    /// <summary>
    ///     读取Csv数据
    /// </summary>
    /// <typeparam name="T">属性顺序需与csv列顺序一致</typeparam>
    /// <param name="path"></param>
    /// <param name="skipFirstLine"></param>
    /// <param name="csvDelimiter"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<T> ReadCsvAsync<T>(string path, bool skipFirstLine = true, string csvDelimiter = ",") where T : class, new()
    {
        var entityType = typeof(T);
        var properties = SingletonDictionary<Type, PropertyInfo[]>.Instance.GetOrAdd(entityType, () => entityType.GetProperties());
        var csvDelimiters = csvDelimiter.ToCharArray();
        using var reader = new StreamReader(path);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line == null) continue;
            
            var values = line.Split(csvDelimiters);
            if (properties.Length != values.Length) continue;
            
            if (skipFirstLine)
            {
                skipFirstLine = false;
            }
            else
            {
                var model = new T();
                for (var i = 0; i < values.Length; i++)
                    PropertyValueSetter<T>.SetPropertyValueObject(entityType, model, properties[i].Name, values[i].CastTo(properties[i].PropertyType));
                yield return model;
            }
        }
    }

    /// <summary>
    ///     导出Csv数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="path">cs文件路径</param>
    /// <param name="includeHeader"></param>
    /// <param name="csvDelimiter"></param>
    /// <param name="rewrite"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ExportCsvAsync<T>(IEnumerable<T> data, string path, bool includeHeader = true, string csvDelimiter = ",", bool rewrite = false, CancellationToken cancellationToken = default) where T: class
    {
        if (FileUtility.Exists(path) && !rewrite) throw new Exception($"Csv文件“{path}”已存在");
        
        var allLineText = BuildAllLineText(data, includeHeader, csvDelimiter);
        
        if (rewrite) FileUtility.DeleteIfExists(path);
            
        await File.AppendAllLinesAsync(path, allLineText, cancellationToken);
    }

    /// <summary>
    ///     构建所有行文本数据集合
    /// </summary>
    /// <param name="data"></param>
    /// <param name="includeHeader"></param>
    /// <param name="csvDelimiter"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static IEnumerable<string> BuildAllLineText<T>(IEnumerable<T> data, bool includeHeader = true, string csvDelimiter = ",") where T: class
    {
        var entityType = typeof(T);
        var properties = SingletonDictionary<Type, PropertyInfo[]>.Instance.GetOrAdd(entityType, () => entityType.GetProperties());

        if (includeHeader)
            yield return properties.Select(x => x.Name).JoinAsString(csvDelimiter);

        var propertyDynamicGetter = new PropertyDynamicGetter<T>();
        var csvDelimiterLen = csvDelimiter.Length;
        
        foreach (var item in data)
        {
            using var psb = Pool.StringBuilder.Get(out var sb);
            var values = properties.Select(pi => new { Value = propertyDynamicGetter.GetPropertyValue(item, pi.Name) });
            foreach (var val in values)
            {
                if (val.Value != null)
                {
                    var escapeVal = val.Value.ToString();
                    // ReSharper disable once PossibleNullReferenceException
                    if (escapeVal.IndexOf(',') != -1) 
                        escapeVal = $"\"{escapeVal}\"";
                    
                    if (escapeVal.IndexOf('\r', StringComparison.OrdinalIgnoreCase) != -1)
                        escapeVal = escapeVal.Replace("\r", " ", StringComparison.OrdinalIgnoreCase);
                    
                    if (escapeVal.IndexOf('\n', StringComparison.OrdinalIgnoreCase) != -1)
                        escapeVal = escapeVal.Replace("\n", " ", StringComparison.OrdinalIgnoreCase);

                    sb.Append(escapeVal).Append(csvDelimiter);
                }
                else
                {
                    sb.Append(string.Empty).Append(csvDelimiter);
                }
            }
            sb.Remove(sb.Length - csvDelimiterLen, csvDelimiterLen);
            yield return sb.ToString();
        }
    }
}