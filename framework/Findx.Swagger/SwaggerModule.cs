using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Findx.AspNetCore;
using Findx.Extensions;
using Findx.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Findx.Swagger;

/// <summary>
///     Findx-Swagger文档模块
/// </summary>
[Description("Findx-Swagger文档模块")]
public class SwaggerModule : AspNetCoreModuleBase
{
    /// <summary>
    ///     配置
    /// </summary>
    private readonly SwaggerOptions _swaggerOptions = new();

    /// <summary>
    ///     等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Application;

    /// <summary>
    ///     排序
    /// </summary>
    public override int Order => 40;

    /// <summary>
    ///     配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // 配置服务
        var configuration = services.GetConfiguration();
        configuration.GetSection("Findx:Swagger").Bind(_swaggerOptions);
        if (!_swaggerOptions.Enabled)
            return services;

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            // 添加文档
            AddDocs(_swaggerOptions, options);

            // 添加自定义配置
            AddCustomOptions(_swaggerOptions, options);

            // 添加注释
            AddXmlComments(options);

            // 添加过滤器
            AddDocumentFilter(options);

            // 添加权限
            AddSecurity(options);

            // 标签分组
            // AddActionTag(options);
        });

        return services;
    }

    public override void UseModule(IApplicationBuilder app)
    {
        if (!_swaggerOptions.Enabled)
            return;

        app.UseSwagger()
            .UseSwaggerUI(options =>
            {
                if (_swaggerOptions.Endpoints?.Count > 0)
                    foreach (var endpoint in _swaggerOptions.Endpoints)
                        options.SwaggerEndpoint(endpoint.Url, endpoint.Title);

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

    /// <summary>
    ///     添加文档
    /// </summary>
    /// <param name="customOptions"></param>
    /// <param name="options"></param>
    private static void AddDocs(SwaggerOptions customOptions, SwaggerGenOptions options)
    {
        if (customOptions?.Endpoints?.Count > 0)
            foreach (var endpoint in customOptions.Endpoints)
                options.SwaggerDoc($"{endpoint.Version}",
                    new OpenApiInfo { Title = endpoint.Title, Version = endpoint.Version });
    }

    /// <summary>
    ///     添加权限
    /// </summary>
    /// <param name="options"></param>
    private static void AddSecurity(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
            Name = "Authorization",
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }

    /// <summary>
    ///     添加文档过滤器
    /// </summary>
    /// <param name="options"></param>
    private static void AddDocumentFilter(SwaggerGenOptions options)
    {
        options.DocumentFilter<IgnoreApiDocumentFilter>();
        // options.DocumentFilter<TagReorderDocumentFilter>();
    }

    /// <summary>
    ///     添加标签
    /// </summary>
    /// <param name="options"></param>
    private static void AddActionTag(SwaggerGenOptions options)
    {
        options.TagActionsBy(description =>
        {
            var tagAttribute = description.ActionDescriptor.EndpointMetadata.OfType<TagsAttribute>().FirstOrDefault();
            if (tagAttribute != null) return tagAttribute.Tags.ToList();

            return new List<string>
                { description.ActionDescriptor.CastTo<ControllerActionDescriptor>().ControllerName };
        });
    }

    /// <summary>
    ///     添加Xml注释
    /// </summary>
    /// <param name="options"></param>
    private static void AddXmlComments(SwaggerGenOptions options)
    {
        foreach (var file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml"))
            options.IncludeXmlComments(file);
    }

    /// <summary>
    ///     添加自定义配置
    /// </summary>
    /// <param name="customOptions"></param>
    /// <param name="options"></param>
    private static void AddCustomOptions(SwaggerOptions customOptions, SwaggerGenOptions options)
    {
        // 参数描述小驼峰
        if (customOptions is { AllParametersInCamelCase: true })
            options.DescribeAllParametersInCamelCase();

        // 自定义架构编号
        options.CustomSchemaIds(x => x.ToString());
    }
}