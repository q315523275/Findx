using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Findx.AspNetCore.Extensions;
namespace Findx.Module.WebHost
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
                    webBuilder.UseFindxStartup<Startup>();
                });
    }
}
