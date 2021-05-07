using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore.Extensions
{
    /// <summary>
    /// WebHost扩展
    /// </summary>
    public static partial class Extensions
    {
        public static IWebHostBuilder UseFindxStartup<TStartup>(this IWebHostBuilder webHostBuilder) where TStartup : class
        {
            webHostBuilder.UseStartup<TStartup>();
            webHostBuilder.UseKestrel(options =>
            {
                IApplicationInstanceInfo applicationInstanceInfo = options.ApplicationServices.GetService<IApplicationInstanceInfo>();
                options.ListenAnyIP(applicationInstanceInfo.Port);
            });
            return webHostBuilder;
        }
    }
}
