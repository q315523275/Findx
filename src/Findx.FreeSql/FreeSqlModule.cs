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
                FreeSqlConnectionConfig DbConnection = option.Value;
                IFreeSql freeSql = new FreeSqlBuilder().UseConnectionString(DbConnection.DataType, DbConnection.ConnectionString).Build();
                freeSqlClient.TryAdd(option.Key, freeSql);
                services.AddSingleton(freeSql);
                if (FreeSqlOptions.Debug)
                {
                    // Deubg 开启AOP打印
                    freeSql.Aop.CurdAfter += (s, e) => {
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
                    };
                }
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
