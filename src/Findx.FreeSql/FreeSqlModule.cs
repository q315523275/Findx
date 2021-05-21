using Findx.Extensions;
using Findx.Modularity;
using FreeSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Findx.FreeSql
{
    public class FreeSqlModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;
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

            IFreeSqlClient freeSqlClient = services.GetOrAddSingletonInstance<IFreeSqlClient>(() => new FreeSqlClient(FreeSqlOptions));

            foreach (var option in FreeSqlOptions.DataSource)
            {
                FreeSqlConnectionConfig DbConnection = option.Value;
                IFreeSql freeSql = new FreeSqlBuilder().UseConnectionString(DbConnection.DataType, DbConnection.ConnectionString).Build();
                freeSqlClient.Add(option.Key, freeSql);
                services.AddSingleton<IFreeSql>(freeSql);
            }

            return services;
        }
    }
}
