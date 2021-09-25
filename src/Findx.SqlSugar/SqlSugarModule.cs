using Findx.Data;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Findx.SqlSugar
{
    [Description("Findx-SqlSugar组件模块")]
    public class SqlSugarModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Application;

        public override int Order => 20;

        private SqlSugarOptions SqlSugarOptions { set; get; }

        private List<ConnectionConfig> ConnectionConfigs { set; get; }

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            IConfiguration configuration = services.GetConfiguration();
            var section = configuration.GetSection("Findx:SqlSugar");
            services.Configure<SqlSugarOptions>(section);
            SqlSugarOptions = section.Get<SqlSugarOptions>();
            if (SqlSugarOptions == null || !SqlSugarOptions.Enabled)
                return services;

            ConnectionConfigs = new List<ConnectionConfig>();

            var primaryList = SqlSugarOptions.DataSource?.Keys;
            foreach (var primary in primaryList)
            {
                SqlSugarOptions.DataSource[primary].ConfigId = primary;
                SqlSugarOptions.DataSource[primary].InitKeyType = InitKeyType.Attribute;
                ConnectionConfigs.Add(SqlSugarOptions.DataSource[primary]);
            }

            // 添加SqlSugar服务
            services.TryAddScoped(provider =>
            {
                // Sugar构建
                var sugar = new SqlSugarClient(ConnectionConfigs);
                // AOP
                sugar.Aop.OnLogExecuted = (sql, param) =>
                {
                    // 打印SQL日志
                    if (SqlSugarOptions.PrintSQL)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Creating a new SqlSession");
                        sb.AppendLine("==>  Preparing:" + sql);
                        if (param != null && param.Length > 0)
                        {
                            sb.AppendLine("==>  Parameters:" + param?.Length);
                            foreach (var pa in param)
                            {
                                sb.AppendLine("==>  Column:" + pa?.ParameterName + "  Row:" + pa?.Value?.ToString());
                            }
                        }
                        sb.Append($"==>  ExecuteTime:{sugar.Ado.SqlExecutionTime.TotalMilliseconds:0.000}ms");
                        ServiceLocator.GetService<ILogger<SqlSugarModule>>()?.LogInformation(sb.ToString());
                    }
                    // 开启慢SQL记录
                    if (SqlSugarOptions.OutageDetection && sugar.Ado.SqlExecutionTime.TotalMilliseconds > (SqlSugarOptions.OutageDetectionInterval * 1000))
                    {
                        ServiceLocator.GetService<ILogger<SqlSugarModule>>()?.LogInformation($"SqlSugar触发慢日志:执行sql({sql})耗时({sugar.Ado.SqlExecutionTime.TotalMilliseconds:0.000})毫秒");
                    }
                };
                // 开启逻辑删除
                if (SqlSugarOptions.SoftDeletable)
                {
                    // 全局过滤器, FilterName = null,
                    sugar.QueryFilter.Add(new SqlFilterItem { FilterName = "SoftDeletable", FilterValue = it => { return new SqlFilterResult { Sql = $" {SqlSugarOptions.SoftDeletableField} = {SqlSugarOptions.SoftNotDeletableValue} " }; }, IsJoinQuery = false });
                }

                return sugar;
            });

            // 添加仓储实现
            ServiceDescriptor descriptor = new ServiceDescriptor(typeof(IRepository<>), typeof(SqlSugarRepository<>), ServiceLifetime.Scoped);
            services.Replace(descriptor);

            ServiceDescriptor descriptor2 = new ServiceDescriptor(typeof(IUnitOfWorkManager), typeof(SugarUnitOfWorkManager), ServiceLifetime.Scoped);
            services.Replace(descriptor2);

            return services;
        }

        public override void UseModule(IServiceProvider provider)
        {
            if (SqlSugarOptions == null)
                return;

            IOptionsMonitor<SqlSugarOptions> optionsMonitor = provider.GetService<IOptionsMonitor<SqlSugarOptions>>();
            optionsMonitor?.OnChange(options =>
            {
                SqlSugarOptions = options;
                var primaryList = options.DataSource?.Keys;
                foreach (var primary in primaryList)
                {
                    options.DataSource[primary].ConfigId = primary;
                    ConnectionConfigs.Add(options.DataSource[primary]);
                }
            });
            base.UseModule(provider);
        }
    }
}
