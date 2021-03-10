using Findx;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                              .UseKestrel(options =>
                              {
                                  IApplicationInstanceInfo applicationInstanceInfo = options.ApplicationServices.GetService<IApplicationInstanceInfo>();
                                  // Console.WriteLine(applicationInstanceInfo.Port);
                                  options.ListenAnyIP(applicationInstanceInfo.Port);
                              });
                });
    }
}
