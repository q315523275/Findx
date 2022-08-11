using System;
using System.ComponentModel;
using Findx.Data;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Modularity;
using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Findx.FreeSql
{
    /// <summary>
    /// Findx-FreeSql模块
    /// </summary>
    [Description("Findx-FreeSql模块")]
    public class FreeSqlModule : FindxModule
    {
        /// <summary>
        /// 模块等级
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Framework;
        
        /// <summary>
        /// 模块排序
        /// </summary>
        public override int Order => 100;

        /// <summary>
        /// Option
        /// </summary>
        private FreeSqlOptions FreeSqlOptions { set; get; }

        /// <summary>
        /// 配置服务注册
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            var configuration = services.GetConfiguration();
            var section = configuration.GetSection("Findx:FreeSql");
            services.Configure<FreeSqlOptions>(section);
            FreeSqlOptions = section.Get<FreeSqlOptions>();
            if (!(FreeSqlOptions is { Enabled: true }))
                return services;

            var freeSqlClient = services.GetOrAddSingletonInstance(() => new FreeSqlClient());

            foreach (var item in FreeSqlOptions.DataSource)
            {
                // FreeSQL构建开始
                var freeSql = new FreeSqlBuilder().UseConnectionString(item.Value.DbType, item.Value.ConnectionString).UseAutoSyncStructure(FreeSqlOptions.UseAutoSyncStructure).Build();
                // 开启逻辑删除
                if (FreeSqlOptions.SoftDeletable)
                {
                    freeSql.GlobalFilter.Apply<ISoftDeletable>("SoftDeletable", it => it.IsDeleted == false);
                }
                // 开启租户隔离
                if (FreeSqlOptions.MultiTenant)
                {
                    freeSql.GlobalFilter.ApplyIf<ITenant>("Tenant", () => Tenant.TenantId.Value != Guid.Empty, it => it.TenantId == Tenant.TenantId.Value);
                }
                // AOP
                freeSql.Aop.CurdAfter += (s, e) =>
                {
                    // 开启SQL打印
                    if (FreeSqlOptions.PrintSql)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Creating a new SqlSession");
                        sb.AppendLine("==>  Preparing:" + e.Sql);
                        if (e.DbParms.Length > 0)
                        {
                            sb.AppendLine("==>  Parameters:" + e.DbParms?.Length);
                            if (e.DbParms != null)
                                foreach (var pa in e.DbParms)
                                {
                                    sb.AppendLine("==>  Column:" + pa?.ParameterName + "  Row:" +
                                                  pa?.Value);
                                }
                        }
                        sb.Append($"==>  ExecuteTime:{e.ElapsedMilliseconds:0.000}ms");
                        var logger = ServiceLocator.GetService<ILogger<FreeSqlModule>>();
                        logger?.LogInformation(sb.ToString());
                    }
                    // 开启慢SQL记录
                    if (FreeSqlOptions.OutageDetection && e.ElapsedMilliseconds > (FreeSqlOptions.OutageDetectionInterval * 1000))
                    {
                        ServiceLocator.GetService<ILogger<FreeSqlModule>>()?.LogInformation($"FreeSql触发慢日志:执行sql({e.Sql})耗时({e.ElapsedMilliseconds:0.000})毫秒");
                    }
                };
                // 注入
                freeSqlClient.TryAdd(item.Key, freeSql);
                if (item.Key == FreeSqlOptions.Primary)
                    services.AddSingleton(freeSql);
            }

            // 添加仓储实现
            var descriptor = new ServiceDescriptor(typeof(IRepository<>), typeof(FreeSqlRepository<>), ServiceLifetime.Scoped);
            services.Replace(descriptor);

            var descriptor2 = new ServiceDescriptor(typeof(IUnitOfWorkManager), typeof(FreeSqlUnitOfWorkManager), ServiceLifetime.Scoped);
            services.Replace(descriptor2);

            return services;
        }
    }
}
