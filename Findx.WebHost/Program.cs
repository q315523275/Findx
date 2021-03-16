using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Findx.WebHost
{
    public static class Program
    {
        public static IWebHostBuilder UseFindxStartup(this IWebHostBuilder webHostBuilder)
        {
            webHostBuilder.UseKestrel(options =>
            {
                IApplicationInstanceInfo applicationInstanceInfo = options.ApplicationServices.GetService<IApplicationInstanceInfo>();
                options.ListenAnyIP(applicationInstanceInfo.Port);
            });
            return webHostBuilder;
        }

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseFindxStartup();
                });
    }
}
