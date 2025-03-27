using System;
using System.Collections.Generic;
using System.ComponentModel;
using Findx.Common;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.Extensions;
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
            // FreeSql构建开始
            var fsql = new FreeSqlBuilder().UseConnectionString(item.Value.DbType, item.Value.ConnectionString)
                                           .UseAutoSyncStructure(item.Value.UseAutoSyncStructure)
                                           .UseNameConvert(item.Value.NameConvertType)
                                           .Build();

            // 开启租户隔离
            if (item.Value.MultiTenant)
                fsql.GlobalFilter.ApplyIf<ITenant>("Tenant", () => TenantManager.Current.IsNotNullOrWhiteSpace(), it => it.TenantId == TenantManager.Current);
            
            // 开启逻辑删除
            if (item.Value.SoftDeletable)
                fsql.GlobalFilter.Apply<ISoftDeletable>("SoftDeletable", it => it.IsDeleted == false);

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
    private static void Aop_CurdBefore_AuditEntity(CurdBeforeEventArgs e, FreeSqlConnectionConfig option)
    {
        if ((!option.AuditEntity && !option.AuditSqlRaw) || e.EntityType.GetEntityExtensionAttribute().DisableAuditing || e.CurdType == CurdType.Select) return;

        var auditEntityReport = ServiceLocator.GetService<IAuditEntityReport>();
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
    ///     AOP实体审计
    /// </summary>
    /// <param name="option"></param>
    /// <param name="e"></param>
    private static void Aop_CurdAfter_AuditSqlRaw(CurdAfterEventArgs e, FreeSqlConnectionConfig option)
    {
        if ((!option.AuditEntity && !option.AuditSqlRaw) || e.EntityType.GetEntityExtensionAttribute().DisableAuditing || e.CurdType == CurdType.Select) return;

        var auditEntityReport = ServiceLocator.GetService<IAuditEntityReport>();
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
                    auditSqlRawEntry.DbParameters.Add(new AuditSqlRawParameterEntry
                    {
                        DbType = pa.DbType.ToString(),
                        SourceColumn = pa.SourceColumn,
                        ParameterName = pa.ParameterName,
                        Value = pa.Value?.ToString()
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
            if (e.Column.CsName.Equals(option.MultiTenantFieldName, StringComparison.OrdinalIgnoreCase) && e.Column.CsType == typeof(Guid?) && e.Value == null)
            {
                e.Value = TenantManager.Current;
            }
        }
    }

    /// <summary>
    ///     AOP审计值
    /// </summary>
    /// <param name="option"></param>
    /// <param name="e"></param>
    /// <param name="fsql"></param>
    private static void Aop_AuditValue_AuditProperty(AuditValueEventArgs e, FreeSqlConnectionConfig option, IFreeSql fsql)
    {
        if (!option.AuditEntity) return;
        
        var auditEntityReport = ServiceLocator.GetService<IAuditEntityReport>();
        Console.WriteLine(auditEntityReport != null);
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
                EntityTypeName = e.Column.Table.CsName,
                DisplayName = e.Column.Comment,
                PropertyName = e.Property?.Name?? e.Column.CsName,
                PropertyTypeFullName = e.Property?.PropertyType.FullName?? e.Column.CsType.FullName,
                NewValue = e.Value?.ToString()
            };
            auditEntityReport.AuditEntityProperty(auditEntityPropertyEntry);
        }
    }
        
    /// <summary>
    ///     启用模块
    /// </summary>
    /// <param name="app"></param>
    public override void UseModule(IServiceProvider app)
    {
        if (FreeSqlOptions.Enabled) base.UseModule(app);
    }
}