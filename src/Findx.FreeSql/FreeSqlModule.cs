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
    public class FreeSqlModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Application;
        public override int Order => 100;

        private FreeSqlOptions FreeSqlOptions { set; get; }

        /// <summary>
        /// 配置服务注册
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            IConfiguration configuration = services.GetConfiguration();
            var section = configuration.GetSection("Findx:FreeSql");
            services.Configure<FreeSqlOptions>(section);
            FreeSqlOptions = section.Get<FreeSqlOptions>();
            if (FreeSqlOptions == null || !FreeSqlOptions.Enabled)
                return services;

            var freeSqlClient = services.GetOrAddSingletonInstance(() => new FreeSqlClient());

            foreach (var option in FreeSqlOptions.DataSource)
            {
                // FreeSQL构建开始
                FreeSqlConnectionConfig DbConnection = option.Value;
                IFreeSql freeSql = new FreeSqlBuilder().UseConnectionString(DbConnection.DataType, DbConnection.ConnectionString).Build();
                // 开启逻辑删除
                if (FreeSqlOptions.SoftDeletable)
                {
                    freeSql.GlobalFilter.Apply<ISoftDeletable>("SoftDeletable", it => it.IsDeleted == false);
                }
                // AOP
                freeSql.Aop.CurdAfter += (s, e) =>
                {
                    // 开启SQL打印
                    if (FreeSqlOptions.PrintSQL)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Creating a new SqlSession");
                        sb.AppendLine("==>  Preparing:" + e.Sql);
                        if (e != null && e.DbParms.Length > 0)
                        {
                            sb.AppendLine("==>  Parameters:" + e.DbParms?.Length);
                            foreach (var pa in e.DbParms)
                            {
                                sb.AppendLine("==>  Column:" + pa?.ParameterName + "  Row:" + pa?.Value?.ToString());
                            }
                        }
                        sb.Append($"==>  ExecuteTime:{e.ElapsedMilliseconds:0.000}ms");
                        ServiceLocator.GetService<ILogger<FreeSqlModule>>()?.LogInformation(sb.ToString());
                    }
                    // 开启慢SQL记录
                    if (FreeSqlOptions.OutageDetection && e.ElapsedMilliseconds > (FreeSqlOptions.OutageDetectionInterval * 1000))
                    {
                        ServiceLocator.GetService<ILogger<FreeSqlModule>>()?.LogInformation($"FreeSql触发慢日志:执行sql({e.Sql})耗时({e.ElapsedMilliseconds:0.000})毫秒");
                    }
                };
                // 注入
                freeSqlClient.TryAdd(option.Key, freeSql);
                if (option.Key == FreeSqlOptions.Primary)
                    services.AddSingleton(freeSql);
            }

            // 添加仓储实现
            ServiceDescriptor descriptor = new ServiceDescriptor(typeof(IRepository<>), typeof(FreeSqlRepository<>), ServiceLifetime.Scoped);
            services.Replace(descriptor);

            ServiceDescriptor descriptor2 = new ServiceDescriptor(typeof(IUnitOfWorkManager), typeof(FreeSqlUnitOfWorkManager), ServiceLifetime.Scoped);
            services.Replace(descriptor2);

            return services;
        }
    }
}
