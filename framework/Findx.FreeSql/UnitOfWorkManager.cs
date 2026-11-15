using System;
using Findx.Common;
using Findx.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Findx.FreeSql;

/// <summary>
///     工作单元管理器
/// </summary>
public class UnitOfWorkManager : UnitOfWorkManagerBase
{
    private readonly FreeSqlClient _clients;
    private readonly IOptionsMonitor<FreeSqlOptions> _options;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="scopedDictionary"></param>
    public UnitOfWorkManager(IServiceProvider serviceProvider, ScopedDictionary scopedDictionary) : base(scopedDictionary)
    {
        _serviceProvider = serviceProvider;
        _clients = serviceProvider.GetRequiredService<FreeSqlClient>();
        _options = serviceProvider.GetRequiredService<IOptionsMonitor<FreeSqlOptions>>();
    }

    /// <summary>
    ///     获取工作单元
    /// </summary>
    /// <param name="dbPrimary"></param>
    /// <returns></returns>
    protected override IUnitOfWork CreateConnUnitOfWork(string dbPrimary)
    {
        dbPrimary ??= _options.CurrentValue.Primary;

        _clients.TryGetValue(dbPrimary, out var fsql);

        Check.NotNull(fsql, nameof(fsql));

        return new UnitOfWork(_serviceProvider, fsql);
    }

    /// <summary>
    ///     获取默认数据库链接标识
    /// </summary>
    /// <returns></returns>
    protected override string GetDataSourcePrimary()
    {
        return _options.CurrentValue.Primary;
    }

    /// <summary>
    ///     归一化数据源标识
    ///     <para>DataSourceSharing 会让多个键指向同一个 IFreeSql，此时必须归到实际数据源键，否则会各建一个工作单元（各开连接、各开事务），跨数据源的写入就不是原子的</para>
    /// </summary>
    /// <param name="primary"></param>
    /// <returns></returns>
    protected override string NormalizePrimary(string primary)
    {
        if (string.IsNullOrWhiteSpace(primary)) return GetDataSourcePrimary();
        if (!_clients.TryGetValue(primary, out var fsql)) return primary;

        //  以配置里的真实数据源为基准，共享同一实例的别名归一到它
        foreach (var item in _options.CurrentValue.DataSource)
        {
            if (_clients.TryGetValue(item.Key, out var candidate) && ReferenceEquals(candidate, fsql))
                return item.Key;
        }

        return primary;
    }
}