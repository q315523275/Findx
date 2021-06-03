using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Extensions;
using Findx.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Findx.Module.WebHost
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFindx().AddModules();

            services.AddControllers(options => options.Filters.Add(typeof(ValidationModelAttribute)))
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                        options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                    });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseJsonExceptionHandler();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseFindx();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllersWithAreaRoute();
            });
        }
    }
}
