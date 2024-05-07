using System.Runtime.InteropServices;
using Findx.Extensions;

namespace Findx.Utilities;

/// <summary>
///     常用公共辅助操作类
/// </summary>
public static class CommonUtility
{
    /// <summary>
    ///     换行符
    /// </summary>
    public static string Line => Environment.NewLine;

    /// <summary>
    ///     是否Linux操作系统
    /// </summary>
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <summary>
    ///     是否Windows操作系统
    /// </summary>
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    ///     是否苹果操作系统
    /// </summary>
    public static bool IsOsx => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    /// <summary>
    ///     当前操作系统
    /// </summary>
    public static string System => IsWindows ? "Windows" : IsLinux ? "Linux" : IsOsx ? "OSX" : string.Empty;

    /// <summary>
    ///     获取类型
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    public static Type GetType<T>()
    {
        return GetType(typeof(T));
    }

    /// <summary>
    ///     获取类型
    /// </summary>
    /// <param name="type">类型</param>
    public static Type GetType(Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }

    /// <summary>
    ///     按行读取文件内容并组合成字典返回
    /// </summary>
    /// <param name="file"></param>
    /// <param name="separate"></param>
    /// <returns></returns>
    public static IDictionary<string, string> ReadInfo(string file, char separate = ':')
    {
        if (file.IsNullOrEmpty() || !File.Exists(file)) return null;

        var dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(file);
        while (!reader.EndOfStream)
        {
            // 按行读取
            var line = reader.ReadLine();
            if (line == null) continue;
            // 分割
            var p = line.IndexOf(separate);
            if (p <= 0) continue;
            var key = line[..p].Trim();
            var value = line[(p + 1)..].Trim();
            dic[key] = value;
        }

        return dic;
    }

    /// <summary>
    ///     读取文件内容
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryRead(string fileName, out string value)
    {
        value = null;

        if (!File.Exists(fileName)) return false;

        try
        {
            value = File.ReadAllText(fileName).Trim();
            if (value.IsNullOrEmpty()) return false;
        }
        catch
        {
            return false;
        }

        return true;
    }
}