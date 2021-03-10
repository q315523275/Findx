using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
namespace Findx.FreeSql
{
    public class FreeSqlModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;
        public override int Order => 100;
        /// <summary>
        /// 配置服务注册
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {

            // new FreeSqlBuilder().UseConnectionString(DataType.Dameng, "").Build()

            return services;
        }
    }
}
