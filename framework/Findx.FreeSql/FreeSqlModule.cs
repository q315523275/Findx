using System;
using System.ComponentModel;
using System.Text;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Modularity;
using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Findx.FreeSql;

/// <summary>
///     Findx-FreeSql模块
/// </summary>
[Description("Findx-FreeSql模块")]
public class FreeSqlModule : StartupModule
{
    /// <summary>
    ///     模块等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Framework;

    /// <summary>
    ///     模块排序
    /// </summary>
    public override int Order => 50;

    /// <summary>
    ///     Option
    /// </summary>
    private FreeSqlOptions FreeSqlOptions { set; get; }

    /// <summary>
    ///     配置服务注册
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // 配置服务
        var configuration = services.GetConfiguration();
        var section = configuration.GetSection("Findx:FreeSql");
        services.Configure<FreeSqlOptions>(section);
            
        FreeSqlOptions = new FreeSqlOptions();
        section.Bind(FreeSqlOptions);
        if (!FreeSqlOptions.Enabled) return services;

        // 构建FreeSql
        var freeSqlClient = services.GetOrAddSingletonInstance(() => new FreeSqlClient());
        foreach (var item in FreeSqlOptions.DataSource)
        {
            // FreeSQL构建开始
            var freeSql = new FreeSqlBuilder().UseConnectionString(item.Value.DbType, item.Value.ConnectionString)
                .UseAutoSyncStructure(item.Value.UseAutoSyncStructure)
                .UseNameConvert(item.Value.NameConvertType)
                .Build();

            // 开启逻辑删除
            if (item.Value.SoftDeletable)
                freeSql.GlobalFilter.Apply<ISoftDeletable>("SoftDeletable", it => it.IsDeleted == false);

            // 开启租户隔离
            if (item.Value.MultiTenant)
                freeSql.GlobalFilter.ApplyIf<ITenant>("Tenant", () => TenantManager.Current != Guid.Empty,
                    it => it.TenantId == TenantManager.Current);

            // AOP
            freeSql.Aop.CurdAfter += (_, e) =>
            {
                // 开启SQL打印
                if (item.Value.PrintSql)
                {
                    var sb = new StringBuilder("Creating a new SqlSession");
                    sb.AppendLine("==>  Preparing:" + e.Sql);
                    if (e.DbParms.Length > 0)
                    {
                        sb.AppendLine("==>  Parameters:" + e.DbParms?.Length);
                        if (e.DbParms != null)
                            foreach (var pa in e.DbParms)
                                sb.AppendLine("==>  Column:" + pa?.ParameterName + "  Row:" + pa?.Value);
                    }

                    sb.Append($"==>  ExecuteTime:{e.ElapsedMilliseconds:0.000}ms");
                    var logger = ServiceLocator.GetService<ILogger<FreeSqlModule>>();
                    // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                    logger?.LogInformation(sb.ToString());
                }

                // 开启慢SQL记录
                if (item.Value.OutageDetection && e.ElapsedMilliseconds > item.Value.OutageDetectionInterval * 1000)
                    // 推送慢sql事件
                    ServiceLocator.GetService<IApplicationContext>()
                        ?.PublishEvent(new SqlExecutionSlowEvent { ElapsedMilliseconds = e.ElapsedMilliseconds, SqlRaw = e.Sql });
            };

            freeSql.Aop.AuditValue += (_, e) =>
            {
                // 租户自动赋值
                if (item.Value.MultiTenant && TenantManager.Current != Guid.Empty
                                           && e.Property.PropertyType == typeof(Guid?)
                                           && e.Property.Name == item.Value.MultiTenantFieldName)
                    e.Value = TenantManager.Current;
            };

            // 注入
            freeSqlClient.TryAdd(item.Key, freeSql);

            // 数据源共享
            if (item.Value.DataSourceSharing is { Count: > 0 })
                foreach (var sourceKey in item.Value.DataSourceSharing)
                    freeSqlClient.TryAdd(sourceKey, freeSql);

            // 注册单独主IFreeSql
            if (item.Key == FreeSqlOptions.Primary)
                services.AddSingleton(freeSql);
        }

        // 添加仓储实现
        var repositoryWithTypedId = new ServiceDescriptor(typeof(IRepository<,>), typeof(RepositoryWithTypedId<,>), ServiceLifetime.Scoped);
        services.Replace(repositoryWithTypedId);
            
        var repository = new ServiceDescriptor(typeof(IRepository<>), typeof(Repository<>), ServiceLifetime.Scoped);
        services.Replace(repository);

        var unitOfWorkManager = new ServiceDescriptor(typeof(IUnitOfWorkManager), typeof(UnitOfWorkManager), ServiceLifetime.Scoped);
        services.Replace(unitOfWorkManager);

        // Entity属性字典初始化
        SingletonDictionary<Type, EntityExtensionAttribute>.Instance.ThrowIfNull();

        return services;
    }
        
    /// <summary>
    /// 启用模块
    /// </summary>
    /// <param name="app"></param>
    public override void UseModule(IServiceProvider app)
    {
        if (FreeSqlOptions.Enabled) base.UseModule(app);
    }
}