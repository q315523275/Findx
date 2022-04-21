using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore.Extensions
{
    /// <summary>
    /// WebHost扩展
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 使用Findx框架启动
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <param name="webHostBuilder"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseFindxStartup<TStartup>(this IWebHostBuilder webHostBuilder) where TStartup : class
        {
            webHostBuilder.UseStartup<TStartup>();
            webHostBuilder.UseKestrel(options =>
            {
                IApplicationContext applicationInstanceInfo = options.ApplicationServices.GetService<IApplicationContext>();
                options.ListenAnyIP(applicationInstanceInfo.Port);
            });
            return webHostBuilder;
        }
    }
}
