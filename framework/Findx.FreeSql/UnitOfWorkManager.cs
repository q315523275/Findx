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
}