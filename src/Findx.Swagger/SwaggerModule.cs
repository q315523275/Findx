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

namespace Findx.Swagger
{
    /// <summary>
    /// Findx-Swagger文档模块
    /// </summary>
    [Description("Findx-Swagger文档模块")]
    public class SwaggerModule : AspNetCoreModuleBase
    {
        /// <summary>
        /// 等级
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Application;
        
        /// <summary>
        /// 排序
        /// </summary>
        public override int Order => 20;
        
        /// <summary>
        /// 配置
        /// </summary>
        private readonly SwaggerOptions _swaggerOptions = new();
        
        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            // 配置服务
            var configuration = services.GetConfiguration();
            configuration.Bind("Findx:Swagger", _swaggerOptions);
            if (!_swaggerOptions.Enabled)
                return services;

            services.AddEndpointsApiExplorer();
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
                        if (!desc.TryGetMethodInfo(out var method))
                        {
                            return false;
                        }
                        // 文档分组
                        var versions = method.DeclaringType.GetAttributes<ApiExplorerSettingsAttribute>().Select(m => m.GroupName);
                        // ReSharper disable once PossibleMultipleEnumeration
                        if (version.ToLower() == "v1" && !versions.Any())
                        {
                            return true;
                        }
                        // ReSharper disable once PossibleMultipleEnumeration
                        return versions.Any(m => m != null && m.ToString() == version);
                    });
                }
                // 参数描述小驼峰
                if (_swaggerOptions is { AllParametersInCamelCase: true }) 
                    options.DescribeAllParametersInCamelCase();
                // 自定义架构编号
                options.CustomSchemaIds(x => x.FullName);
                // 装载注释
                foreach(var file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml"))
                {
                    options.IncludeXmlComments(file, true);
                }
                // 权限token
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
                // 加载过滤器
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
                // 接口地址复制功能
                options.HeadContent = @"
                    <script type='text/javascript'>
                    window.addEventListener('load', function () {
                        setTimeout(() => {
                            let createElement = window.ui.React.createElement
                            ui.React.createElement = function () {
                                let array = Array.from(arguments)
                                if (array.length == 3) {
                                    if (array[0] == 'span' && !array[1]) {
                                        array[1] = { contentEditable: true }
                                    }
                                }

                                let ele = createElement(...array)
                                return ele
                            }
                        })
                    })
                    </script>";
                options.DocExpansion(_swaggerOptions.DocExpansion);
                if (_swaggerOptions.HideSchemas)
                    options.DefaultModelsExpandDepth(-1);
            });

            base.UseModule(app);
        }
    }
}
