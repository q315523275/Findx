using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Extensions;
using Findx.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Unicode;
namespace Findx.Module.WebHost
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFindx().AddModules();

            services.AddControllers(options => options.Filters.Add(typeof(FindxGlobalAttribute)))
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                        options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
                        options.JsonSerializerOptions.Converters.Add(new DateTimeNullableJsonConverter());
                        options.JsonSerializerOptions.Converters.Add(new LongConverter());
                    });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowed(_ => true)
                );
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseJsonExceptionHandler();
            else
                app.UseJsonExceptionHandler();

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions { RequestPath = "/storage", FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "storage")) });

            app.UseCors("CorsPolicy");

            app.UseRouting();

            app.UseFindx();

            app.UseEndpoints(endpoints => { endpoints.MapControllersWithAreaRoute(); });
        }
    }
}
