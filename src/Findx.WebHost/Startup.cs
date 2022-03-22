using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Data;
using Findx.Extensions;
using Findx.WebHost.RabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Findx.WebHost
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFindx().AddModules();

            // HttpClient Polly策略
            services.AddHttpClient("policy")
                    .AddFallbackPolicy(CommonResult.Fail(), 200)
                    .AddCircuitBreakerPolicy(5, "5s")
                    .AddRetryPolicy(1)
                    .AddTimeoutPolicy(1);

            services.AddHostedService<EventBusWorker>();

            services.AddControllers(options => options.Filters.Add(typeof(ValidationModelAttribute)))
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                    });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseJsonExceptionHandler();

            app.UseRouting();

            app.UseFindx();

            app.UseEndpoints(endpoints => { endpoints.MapControllersWithAreaRoute(); });
        }
    }
}
