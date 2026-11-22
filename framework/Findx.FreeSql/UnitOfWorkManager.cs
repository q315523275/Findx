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
    ///     获取默认数据源标识
    /// </summary>
    /// <returns></returns>
    protected override string GetDefaultPrimary()
    {
        return _options.CurrentValue.Primary;
    }

    /// <summary>
    ///     归一化数据源标识
    ///     <para>DataSourceSharing 会让多个键指向同一个 IFreeSql，此时必须归到实际数据源键，否则会各建一个工作单元（各开连接、各开事务），跨数据源的写入就不是原子的</para>
    ///     <para>别名->主数据源名 的映射在数据源构建期就已登记，此处只做一次字典查询，不再遍历配置、不再访问 IOptionsMonitor</para>
    /// </summary>
    /// <param name="primary"></param>
    /// <returns></returns>
    protected override string NormalizePrimary(string primary)
    {
        return string.IsNullOrWhiteSpace(primary) ? GetDefaultPrimary() : _clients.GetPrimaryName(primary);
    }
}