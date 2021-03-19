using Findx.Data;
using Findx.Extensions;
using Findx.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SqlSugar;
using System;
using System.ComponentModel;
using System.Text;

namespace Findx.SqlSugar
{
    [Description("Findx-SqlSugar组件模块")]
    public class SqlSugarModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;
        public override int Order => 20;
        private SqlSugarOptions SqlSugarOptions { set; get; }

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            IConfiguration configuration = services.GetConfiguration();
            var section = configuration.GetSection("Findx:SqlSugar");
            services.Configure<SqlSugarOptions>(section);
            SqlSugarOptions = section.Get<SqlSugarOptions>();

            // Aop
            Action<string, SugarParameter[]> OnLogExecuting = (sql, param) =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Creating a new SqlSession");
                sb.AppendLine("==>   Preparing:" + sql);
                sb.AppendLine("==>  Parameters:" + param.Length);
                foreach (var pa in param)
                {
                    sb.AppendLine("==>     Column:" + pa.ParameterName + "  Row:" + pa.Value.ToString());
                }
                Console.WriteLine(sb.ToString());
            };
            Action<string, SugarParameter[]> OnLogExecuted = (sql, param) => { };

            // 添加SqlSugar服务
            services.TryAddScoped<SqlSugarClient>(provider =>
            {
                var db = new SqlSugarClient(SqlSugarOptions.DataSource);
                if (SqlSugarOptions.Debug)
                {
                    db.Aop.OnLogExecuting = OnLogExecuting;
                    db.Aop.OnLogExecuted = OnLogExecuted;
                }
                return db;
            });

            // 添加工作单元
            services.AddScoped<IUnitOfWork<SqlSugarClient>, SqlSugarUnitOfWork>();
            services.AddScoped<IUnitOfWork>(provider =>
            {
                // 共用实例
                return provider.GetService<IUnitOfWork<SqlSugarClient>>();
            });
            // 添加仓储实现
            services.AddScoped(typeof(IRepository<>), typeof(SqlSugarRepository<>));
            return services;
        }

        public override void UseModule(IServiceProvider provider)
        {
            IOptionsMonitor<SqlSugarOptions> optionsMonitor = provider.GetService<IOptionsMonitor<SqlSugarOptions>>();
            optionsMonitor?.OnChange(options => SqlSugarOptions = options);

            base.UseModule(provider);
        }
    }
}
