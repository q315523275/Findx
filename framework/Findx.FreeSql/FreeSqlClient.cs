using System;
using System.Collections.Generic;

namespace Findx.FreeSql;

/// <summary>
///     FreeSql
/// </summary>
public class FreeSqlClient : Dictionary<string, IFreeSql>, IDisposable
{
    private readonly FreeSqlOptions _options;

    /// <summary>
    ///     标识 -> 主数据源标识
    ///     <para>构建期一次性写入，运行期只读，用于 O(1) 取主连接名；不再加锁，不参与热更新</para>
    /// </summary>
    private readonly Dictionary<string, string> _primaryMap = new(StringComparer.Ordinal);

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="options"></param>
    public FreeSqlClient(FreeSqlOptions options)
    {
        _options = options;
    }

    /// <summary>
    ///     获取默认
    /// </summary>
    /// <returns></returns>
    public IFreeSql GetDefault()
    {
        return this[_options.Primary];
    }

    /// <summary>
    ///     注册数据源
    ///     <para>写入实例字典的同时记录别名归属，后续取主连接名直接查字典，不必再靠 ReferenceEquals 反查配置</para>
    /// </summary>
    /// <param name="primary">主数据源标识</param>
    /// <param name="fsql">FreeSql 实例</param>
    /// <param name="sharingKeys">共享该实例的别名标识（DataSourceSharing）</param>
    public void RegisterSource(string primary, IFreeSql fsql, IEnumerable<string> sharingKeys = null)
    {
        if (string.IsNullOrWhiteSpace(primary) || fsql == null) return;

        //  先到先得：与实例字典的 TryAdd 语义保持一致，同名标识不会被后注册的数据源抢占
        if (TryAdd(primary, fsql)) _primaryMap[primary] = primary;

        if (sharingKeys == null) return;
        foreach (var key in sharingKeys)
        {
            if (string.IsNullOrWhiteSpace(key)) continue;
            if (TryAdd(key, fsql)) _primaryMap[key] = primary;
        }
    }

    /// <summary>
    ///     获取主连接名
    ///     <para>DataSourceSharing 的别名归到它的主数据源标识；未登记的一律原样返回（等价于不归一化）</para>
    /// </summary>
    /// <param name="dataSource">数据源标识</param>
    /// <returns></returns>
    public string GetPrimaryName(string dataSource)
    {
        if (string.IsNullOrWhiteSpace(dataSource)) return _options.Primary;

        return _primaryMap.TryGetValue(dataSource, out var primary) ? primary : dataSource;
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        foreach (var fsql in Values) fsql?.Dispose();

        Clear();
        _primaryMap.Clear();
    }
}
