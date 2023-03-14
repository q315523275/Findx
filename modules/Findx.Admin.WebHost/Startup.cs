using Findx.AspNetCore.Extensions;
using Findx.AspNetCore.Mvc.Filters;
using Findx.Extensions;
using Findx.Serialization;
using Microsoft.Extensions.FileProviders;
using System.Text.Json;
using Findx.Admin.WebHost.WebShell;
using Findx.WebSocketCore;
using Microsoft.AspNetCore.WebSockets;

namespace Findx.Module.WebHost
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFindx().AddModules();

            services.AddControllers()
                    .AddMvcFilter<FindxGlobalAttribute>()
                    .AddJsonOptions(options =>
                    {
                        // options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs, UnicodeRanges.CjkSymbolsandPunctuation);
                        options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
                        options.JsonSerializerOptions.Converters.Add(new DateTimeNullableJsonConverter());
                        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    });

            services.AddEndpointsApiExplorer();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder => builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowed(_ => true)
                );
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            services.AddWebSockets(x =>
            {
                x.KeepAliveInterval = TimeSpan.FromMinutes(1);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseJsonExceptionHandler();

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions { RequestPath = "/storage", FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "storage")) });

            app.UseCors("CorsPolicy");

            app.UseResponseCompression();

            app.UseRouting();

            app.UseWebSockets();
            app.MapWebSocketManager("/ws", app.ApplicationServices.GetRequiredService<WebSocketHandler>());

            app.UseFindx();

            app.UseEndpoints(endpoints => { endpoints.MapControllersWithAreaRoute(); });
        }
    }
}
