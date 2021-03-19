using Findx.AspNetCore.Extensions;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Findx.WebHost
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFindx().AddModules();
            services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All); });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseFindx();

            app.UseRouting().UseEndpoints(endpoints => { endpoints.MapControllersWithAreaRoute(); });
        }
    }
}
