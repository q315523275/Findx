using Findx.AspNetCore;
using Findx.Extensions;
using Findx.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Findx.Swagger
{
    [Description("Findx-Swagger文档模块")]
    public class SwaggerModule : AspNetCoreModuleBase
    {
        public override ModuleLevel Level => ModuleLevel.Application;
        public override int Order => 20;
        private SwaggerOptions _swaggerOptions = new();
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            var configuration = services.GetConfiguration();
            configuration.Bind("Findx:Swagger", _swaggerOptions);
            if (!_swaggerOptions.Enabled)
                return services;

            services.AddMvcCore().AddApiExplorer();
            services.AddSwaggerGen(options =>
            {
                if (_swaggerOptions?.Endpoints?.Count > 0)
                {
                    foreach (var endpoint in _swaggerOptions.Endpoints)
                    {
                        options.SwaggerDoc($"{endpoint.Version}", new OpenApiInfo() { Title = endpoint.Title, Version = endpoint.Version });
                    }
                    options.DocInclusionPredicate((version, desc) =>
                    {
                        if (!desc.TryGetMethodInfo(out MethodInfo method))
                        {
                            return false;
                        }
                        // 文档分组
                        var versions = method.DeclaringType.GetAttributes<ApiExplorerSettingsAttribute>().Select(m => m.GroupName);
                        if (version.ToLower() == "v1" && !versions.Any())
                        {
                            return true;
                        }
                        return versions.Any(m => m.ToString() == version);
                    });
                }
                // 参数描述小驼峰
                if (_swaggerOptions.AllParametersInCamelCase) options.DescribeAllParametersInCamelCase();
                options.CustomSchemaIds(x => x.FullName);
                var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml");
                foreach(var file in files)
                {
                    options.IncludeXmlComments(file, true);
                }
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
                    Name = "Authorization",
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }, Array.Empty<string>()
                    }
                });
                options.DocumentFilter<IgnoreApiFilter>();
            });

            return services;
        }

        public override void UseModule(IApplicationBuilder app)
        {
            if (!_swaggerOptions.Enabled)
                return;

            app.UseSwagger().UseSwaggerUI(options =>
            {
                if (_swaggerOptions.Endpoints?.Count > 0)
                {
                    foreach (var endpoint in _swaggerOptions.Endpoints)
                    {
                        options.SwaggerEndpoint(endpoint.Url, endpoint.Title);
                    }
                }
            });

            base.UseModule(app);
        }
    }
}
