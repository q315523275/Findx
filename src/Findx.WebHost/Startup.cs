using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc.Filters;
using Findx.EventBus.Abstractions;
using Findx.Extensions;
using Findx.WebHost.EventBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Findx.WebHost
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFindx().AddModules();

            services.AddControllers(options => options.Filters.Add(typeof(ValidationModelAttribute)))
                    .AddJsonOptions(options => { options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All); });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseFindx();

            app.UseEndpoints(endpoints => { endpoints.MapControllersWithAreaRoute(); });

            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<FindxTestEvent, FindxTestEventHander>();
            eventBus.Subscribe<FindxTestEvent, FindxTestEventHanderTwo>();
            eventBus.SubscribeDynamic<FindxTestDynamicEventHandler>("FindxTestRoutingKey");
            eventBus.StartConsuming();
        }
    }
}
