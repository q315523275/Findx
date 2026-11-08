using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Findx.Common;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Logging;
using Findx.Modularity;
using FreeSql;
using FreeSql.Aop;
using FreeSql.Extensions.EntityUtil;
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
    ///     应用根服务容器（UseModule 时缓存），供 AOP 回调解析单例服务，避免每次 CRUD 走 ServiceLocator 的双层查找
    /// </summary>
    private IServiceProvider _serviceProvider;

    /// <summary>
    ///     审计报告服务（单例，首次解析后缓存，不再重复从容器解析）
    /// </summary>
    private IAuditEntityReport _auditEntityReport;

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
        var freeSqlClient = services.GetOrAddSingletonInstance(() => new FreeSqlClient(FreeSqlOptions));
        foreach (var item in FreeSqlOptions.DataSource)
        {
            //  PrintSql 是调试开关：每条 SQL 都会拼接 StringBuilder 并输出日志，生产误开会带来明显 CPU/IO 开销
            if (item.Value.PrintSql)
            {
                services.GetOrAddSingletonInstance(() => new StartupLogger())
                        .Log(LogLevel.Warning, $"数据源[{item.Key}]已开启 PrintSql，生产环境建议关闭", nameof(FreeSqlModule));
            }

            // FreeSql构建开始
            var fsql = new FreeSqlBuilder().UseConnectionString(item.Value.DbType, item.Value.ConnectionString)
                                           .UseAutoSyncStructure(item.Value.UseAutoSyncStructure)
                                           .UseNameConvert(item.Value.NameConvertType)
                                           .Build();

            // 开启租户隔离
            if (item.Value.MultiTenant)
            {
                switch (item.Value.TenantFieldType)
                {
                    case TenantFieldType.Guid:
                        fsql.GlobalFilter.ApplyOnlyIf<ITenant<Guid>>("Tenant", () => TenantManager<Guid>.Current != Guid.Empty, it => it.TenantId == TenantManager<Guid>.Current);
                        break;
                    case TenantFieldType.Int:
                        fsql.GlobalFilter.ApplyOnlyIf<ITenant<int>>("Tenant", () => TenantManager<int>.Current > 0, it => it.TenantId == TenantManager<int>.Current);
                        break;
                    case TenantFieldType.Long:
                        fsql.GlobalFilter.ApplyOnlyIf<ITenant<long>>("Tenant", () => TenantManager<long>.Current > 0, it => it.TenantId == TenantManager<long>.Current);
                        break;
                    case TenantFieldType.String:
                    default:
                        fsql.GlobalFilter.ApplyOnlyIf<ITenant>("Tenant", () => TenantManager.Current.IsNotNullOrWhiteSpace(), it => it.TenantId == TenantManager.Current);
                        break;
                }
            }
            
            // 开启逻辑删除
            if (item.Value.SoftDeletable)
                fsql.GlobalFilter.ApplyOnly<ISoftDeletable>("SoftDeletable", it => it.IsDeleted == false);

            // AOP
            fsql.Aop.CurdBefore += (_, e) => Aop_CurdBefore(e, item.Value);
            fsql.Aop.CurdAfter += (_, e) => Aop_CurdAfter(e, item.Value);
            fsql.Aop.AuditValue += (_, e) => Aop_AuditValue(e, item.Value, fsql);

            // 注入
            freeSqlClient.TryAdd(item.Key, fsql);

            // 数据源共享
            if (item.Value.DataSourceSharing is { Count: > 0 }) 
                foreach (var sourceKey in item.Value.DataSourceSharing) 
                    freeSqlClient.TryAdd(sourceKey, fsql);

            // 注册单独主IFreeSql
            if (item.Key == FreeSqlOptions.Primary)
                services.AddSingleton(fsql);
        }

        // 添加仓储实现
        var repositoryWithTypedId = new ServiceDescriptor(typeof(IRepository<,>), typeof(RepositoryWithTypedId<,>), ServiceLifetime.Scoped);
        services.Replace(repositoryWithTypedId);
            
        var repository = new ServiceDescriptor(typeof(IRepository<>), typeof(Repository<>), ServiceLifetime.Scoped);
        services.Replace(repository);

        var unitOfWorkManager = new ServiceDescriptor(typeof(IUnitOfWorkManager), typeof(UnitOfWorkManager), ServiceLifetime.Scoped);
        services.Replace(unitOfWorkManager);

        var auditEntityReport = new ServiceDescriptor(typeof(IAuditEntityReport), typeof(FreeSqlAuditEntityReport), ServiceLifetime.Singleton);
        services.Replace(auditEntityReport);

        // Entity属性字典初始化
        SingletonDictionary<Type, EntityExtensionAttribute>.Instance.ThrowIfNull();
        
        return services;
    }
    
    /// <summary>
    ///     Aop_CurdBefore
    /// </summary>
    private void Aop_CurdBefore(CurdBeforeEventArgs e, FreeSqlConnectionConfig option)
    {
        // 实体审计
        Aop_CurdBefore_AuditEntity(e, option);
    }
    
    /// <summary>
    ///     Aop_CurdAfter
    /// </summary>
    private void Aop_CurdAfter(CurdAfterEventArgs e, FreeSqlConnectionConfig option)
    {
        // 开启SQL打印
        Aop_CurdAfter_PrintSql(e, option);
        
        // 耗时检测
        Aop_CurdAfter_Outage_Detection(e, option);
        
        // Sql审计
        Aop_CurdAfter_AuditSqlRaw(e, option);
    }

    /// <summary>
    ///     AOP打印执行sql
    /// </summary>
    /// <param name="option"></param>
    /// <param name="e"></param>
    private static void Aop_CurdAfter_PrintSql(CurdAfterEventArgs e, FreeSqlConnectionConfig option)
    {
        if (!option.PrintSql) return;
        
        // 开启SQL打印
        using var psb = Pool.StringBuilder.Get(out var sb);
        sb.AppendLine("Creating a new SqlSession");
        sb.AppendLine("==>  Preparing:" + e.Sql);
        if (e.DbParms.Length > 0)
        {
            sb.AppendLine("==>  Parameters:" + e.DbParms?.Length);
            if (e.DbParms != null)
            {
                foreach (var pa in e.DbParms)
                {
                    sb.AppendLine("==>  Column:" + pa?.ParameterName + "  Row:" + pa?.Value);
                }
            }
        }
        sb.Append($"==>  ExecuteTime:{e.ElapsedMilliseconds:0.000}ms");
        var logger = ServiceLocator.GetService<ILogger<FreeSqlModule>>();
        // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
        logger?.LogInformation(sb.ToString());
    }

    /// <summary>
    ///     AOP耗时检测
    /// </summary>
    /// <param name="option"></param>
    /// <param name="e"></param>
    private static void Aop_CurdAfter_Outage_Detection(CurdAfterEventArgs e, FreeSqlConnectionConfig option)
    {
        // 开启慢SQL记录
        if (!option.OutageDetection || e.ElapsedMilliseconds <= option.OutageDetectionInterval * 1000) return;
        // 推送慢sql事件
        var eventbus = ServiceLocator.GetService<IApplicationContext>();
        eventbus?.PublishEvent(new SqlExecutionSlowEvent { ElapsedMilliseconds = e.ElapsedMilliseconds, SqlRaw = e.Sql });
    }

    /// <summary>
    ///     AOP实体审计
    /// </summary>
    /// <param name="option"></param>
    /// <param name="e"></param>
    private void Aop_CurdBefore_AuditEntity(CurdBeforeEventArgs e, FreeSqlConnectionConfig option)
    {
        if ((!option.AuditEntity && !option.AuditSqlRaw) || e.EntityType.GetEntityExtensionAttribute().DisableAuditing || e.CurdType == CurdType.Select) return;
        
        var auditEntityReport = ResolveAuditEntityReport();
        if (auditEntityReport != null && option.AuditEntity)
        {
            var auditEntity = new AuditEntityEntry
            {
                EntityTypeName = e.EntityType.Name,
                EntityTypeFullName = e.EntityType.FullName,
                ExecutionTime = DateTime.Now,
            };
            auditEntityReport.AuditEntity(auditEntity);
        }
    }

    /// <summary>
    ///     审计日志中需脱敏的参数名关键字（列名或参数名命中即掩码）
    /// </summary>
    private static readonly string[] SensitiveParameterKeywords =
    {
        "password", "passwd", "pwd", "secret", "token", "credential", "idcard", "id_no",
        "phone", "mobile", "email", "bankcard", "bank_card", "salt", "authorization"
    };

    /// <summary>
    ///     列名/参数名敏感判定缓存：字段名集合有限，命中后避免每次都对 15 个关键字做 Contains
    /// </summary>
    private static readonly ConcurrentDictionary<string, bool> SensitiveParameterCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     敏感判定缓存上限（防御动态 SQL 产生大量非常规参数名导致缓存膨胀）
    /// </summary>
    private const int SensitiveParameterCacheLimit = 4096;

    /// <summary>
    ///     判断列名/参数名是否属于敏感字段
    /// </summary>
    private static bool IsSensitiveParameter(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;

        if (SensitiveParameterCache.TryGetValue(name, out var cached)) return cached;

        var sensitive = false;
        foreach (var keyword in SensitiveParameterKeywords)
        {
            if (name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                sensitive = true;
                break;
            }
        }

        if (SensitiveParameterCache.Count < SensitiveParameterCacheLimit)
            SensitiveParameterCache[name] = sensitive;

        return sensitive;
    }


    /// <summary>
    ///     AOP实体审计
    /// </summary>
    /// <param name="option"></param>
    /// <param name="e"></param>
    private void Aop_CurdAfter_AuditSqlRaw(CurdAfterEventArgs e, FreeSqlConnectionConfig option)
    {
        if ((!option.AuditEntity && !option.AuditSqlRaw) || e.EntityType.GetEntityExtensionAttribute().DisableAuditing || e.CurdType == CurdType.Select) return;
        
        var auditEntityReport = ResolveAuditEntityReport();
        if (auditEntityReport != null && option.AuditSqlRaw)
        {
            var auditSqlRawEntry = new AuditSqlRawEntry
            {
                EntityTypeFullName = e.EntityType.FullName,
                DbTableName = e.Table.DbName,
                SqlRaw = e.Sql,
                ExecutionTime = DateTime.Now,
                ExecutionDuration = e.ElapsedMilliseconds,
                ExecutionResult = e.ExecuteResult?.ToString()
            };
            if (e.DbParms?.Length > 0)
            {
                foreach (var pa in e.DbParms)
                {
                    //  敏感列/参数值脱敏，避免密码、令牌、证件号等明文落入审计日志
                    var masked = IsSensitiveParameter(pa.SourceColumn) || IsSensitiveParameter(pa.ParameterName) ? "***" : pa.Value?.ToString();
                    auditSqlRawEntry.DbParameters.Add(new AuditSqlRawParameterEntry
                    {
                        DbType = pa.DbType.ToString(),
                        SourceColumn = pa.SourceColumn,
                        ParameterName = pa.ParameterName,
                        Value = masked
                    });
                }
            }
            auditEntityReport.AuditSqlRaw(auditSqlRawEntry);
        }
    }
    
    /// <summary>
    ///     Aop_AuditValue
    /// </summary>
    private void Aop_AuditValue(AuditValueEventArgs e, FreeSqlConnectionConfig option, IFreeSql fsql)
    {
        // 租户自动赋值
        Aop_AuditValue_Tenant(e, option);
        
        // 审计值
        Aop_AuditValue_AuditProperty(e, option, fsql);
    }

    /// <summary>
    ///     AOP租户自动赋值
    /// </summary>
    /// <param name="option"></param>
    /// <param name="e"></param>
    private static void Aop_AuditValue_Tenant(AuditValueEventArgs e, FreeSqlConnectionConfig option)
    {
        // 新建数据时,租户自动赋值
        if (option.MultiTenant && e.AuditValueType == AuditValueType.Insert && TenantManager.Current.IsNotNullOrWhiteSpace() && option.MultiTenantFieldName.IsNotNullOrWhiteSpace())
        {
            //  实体属性字段
            if (e.Column.CsName.Equals(option.MultiTenantFieldName, StringComparison.OrdinalIgnoreCase))
            {
                switch (option.TenantFieldType)
                {
                    case TenantFieldType.Guid when e.Value == null:
                        e.Value = TenantManager<Guid>.Current;
                        break;
                    case TenantFieldType.Int when e.Value?.ToString() == "0":
                        e.Value = TenantManager<int>.Current;
                        break;
                    case TenantFieldType.Long when e.Value?.ToString() == "0":
                        e.Value = TenantManager<long>.Current;
                        break;
                    case TenantFieldType.String when e.Value == null:
                    default:
                        e.Value = TenantManager.Current;
                        break;
                }
                
            }
        }
    }

    /// <summary>
    ///     AOP审计值
    /// </summary>
    /// <param name="option"></param>
    /// <param name="e"></param>
    /// <param name="fsql"></param>
    private void Aop_AuditValue_AuditProperty(AuditValueEventArgs e, FreeSqlConnectionConfig option, IFreeSql fsql)
    {
        if (!option.AuditEntity) return;
        
        // Todo 需增加全局过滤,字段值无法识别出实体,记录审计日志数据的时候也会触发此模块
        // 临时解决方案:审计日志数据入库是异步线程执行,通过Http对象进行识别
        
        if (!ServiceLocator.InScoped()) return;
        
        //  该回调按"列"触发（N 个字段即 N 次），使用缓存后的单例服务，避免每次列值都经过 ServiceLocator 双重查找
        var auditEntityReport = ResolveAuditEntityReport();
        if (auditEntityReport != null && e.Value != null && !e.Column.CsName.Equals("Id", StringComparison.OrdinalIgnoreCase))
        {
            string entityId;
            if (e.Object is Dictionary<string, object> dic && dic.TryGetValue("Id", out var id))
                entityId = id.ToString();
            else
                entityId = fsql.GetEntityKeyString(e.Object.GetType(), e.Object, false);

            // 实体值审计
            var auditEntityPropertyEntry = new AuditEntityPropertyEntry
            {
                EntityId = entityId,
                EntityTypeName = e.Column.Table.Type?.Name,
                DisplayName = e.Column.Comment,
                PropertyName = e.Property?.Name?? e.Column.CsName,
                PropertyTypeFullName = e.Property?.PropertyType.FullName?? e.Column.CsType.FullName,
                NewValue = e.Value?.ToString()
            };
            auditEntityReport.AuditEntityProperty(auditEntityPropertyEntry);
        }
    }
        
    /// <summary>
    ///     解析审计报告服务
    ///     <para>IAuditEntityReport 为单例注册，首次解析后缓存，避免每 CRUD/每字段都经过 ServiceLocator 的双层查找</para>
    /// </summary>
    private IAuditEntityReport ResolveAuditEntityReport()
    {
        return _auditEntityReport ??= (_serviceProvider?.GetService<IAuditEntityReport>() ?? ServiceLocator.GetService<IAuditEntityReport>());
    }

    /// <summary>
    ///     启用模块
    /// </summary>
    /// <param name="app"></param>
    public override void UseModule(IServiceProvider app)
    {
        _serviceProvider = app;
        if (FreeSqlOptions.Enabled) base.UseModule(app);
    }
}