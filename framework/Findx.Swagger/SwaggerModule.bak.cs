// using System;
// using System.ComponentModel;
// using System.Linq;
// using Findx.AspNetCore;
// using Findx.Extensions;
// using Findx.Modularity;
//
// using Microsoft.AspNetCore.Builder;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.OpenApi.Any;
// using Microsoft.OpenApi.Models;
// #if NET10_0_OR_GREATER
// using Findx.Swagger.Transformers;
// using System.Threading.Tasks;
// #else
// using System;
// using System.Collections.Generic;
// using System.IO;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc.Controllers;
// using Swashbuckle.AspNetCore.SwaggerGen;
// using Findx.Swagger.Filters;
// #endif
//
// namespace Findx.Swagger;
//
// /// <summary>
// ///     Findx-Swagger文档模块
// /// </summary>
// [Description("Findx-Swagger文档模块")]
// public class SwaggerModule : WebApplicationModuleBase
// {
//     /// <summary>
//     ///     配置
//     /// </summary>
//     private readonly SwaggerOptions _swaggerOptions = new();
//
//     /// <summary>
//     ///     等级
//     /// </summary>
//     public override ModuleLevel Level => ModuleLevel.Application;
//
//     /// <summary>
//     ///     排序
//     /// </summary>
//     public override int Order => 40;
//
//     /// <summary>
//     ///     配置服务
//     /// </summary>
//     /// <param name="services"></param>
//     /// <returns></returns>
//     public override IServiceCollection ConfigureServices(IServiceCollection services)
//     {
//         // 配置服务
//         var configuration = services.GetConfiguration();
//         configuration.GetSection("Findx:Swagger").Bind(_swaggerOptions);
//         
//         if (!_swaggerOptions.Enabled) return services;
//
//         #if NET10_0_OR_GREATER
//             if (_swaggerOptions?.Endpoints?.Count > 0)
//             {
//                 foreach (var endpoint in _swaggerOptions.Endpoints)
//                 {
//                     services.AddOpenApi(endpoint.Version, options => {
//                         options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
//                         options.AddDocumentTransformer((document, _, _) =>
//                         {
//                             document.Info.Title = endpoint.Title ?? document.Info.Title;
//                             document.Servers = endpoint.Servers?.Select(x => new OpenApiServer { Url = x.Url, Description = x.Description }).ToArray();
//                             return Task.CompletedTask;
//                         });
//                         // options.AddSchemaTransformer((schema, context, _) =>
//                         // {
//                         //     if (context.JsonTypeInfo.Type.IsEnum)
//                         //     {
//                         //         schema.Enum.Clear();
//                         //         foreach (var name in Enum.GetNames(context.JsonTypeInfo.Type))
//                         //         {
//                         //             var enumValue = Enum.Parse(context.JsonTypeInfo.Type, name);
//                         //             // Todo 如果经常使用,可以进行一些性能优化
//                         //             schema.Enum.Add(new OpenApiString($"{name}({enumValue.GetType().GetField(name).GetDescription()})={enumValue.CastTo<int>()}"));
//                         //         }
//                         //     }
//                         //     return Task.CompletedTask;
//                         // });
//                     });
//                 }
//             }
//         #else
//             services.AddEndpointsApiExplorer();
//             services.AddSwaggerGen(options =>
//             {
//                 // 添加文档
//                 AddDocs(_swaggerOptions, options);
//
//                 // 添加自定义配置
//                 AddCustomOptions(_swaggerOptions, options);
//
//                 // 添加注释
//                 AddXmlComments(options);
//
//                 // 添加过滤器
//                 AddDocumentFilter(options);
//
//                 // 添加权限
//                 AddSecurity(options);
//
//                 // 标签分组
//                 // AddActionTag(options);
//             });
//         #endif
//
//         return services;
//     }
//
//     public override void UseModule(WebApplication app)
//     {
//         if (!_swaggerOptions.Enabled)
//             return;
//
//         #if NET10_0_OR_GREATER
//             app.MapOpenApi();
//             app.MapOpenApi("/swagger/{documentName}/swagger.json");
//         #else
//             app.UseSwagger();
//         #endif
//         
//         app.UseSwaggerUI(options =>
//         {
//             if (_swaggerOptions.Endpoints?.Count > 0)
//                 foreach (var endpoint in _swaggerOptions.Endpoints)
//                     options.SwaggerEndpoint(endpoint.Url, endpoint.Title);
//
//             // 接口地址复制功能
//             options.HeadContent = @"
//                         <script type='text/javascript'>
//                             window.addEventListener('load', function () {
//                                 setTimeout(() => {
//                                     let createElement = window.ui.React.createElement
//                                     ui.React.createElement = function () {
//                                         let array = Array.from(arguments)
//                                         if (array.length == 3) {
//                                             if (array[0] == 'span' && !array[1]) {
//                                                 array[1] = { contentEditable: true }
//                                             }
//                                         }
//                                         let ele = createElement(...array)
//                                         return ele
//                                     }
//                                 })
//                             })
//                         </script>";
//
//             options.DocExpansion(_swaggerOptions.DocExpansion);
//
//             if (_swaggerOptions.HideSchemas)
//                 options.DefaultModelsExpandDepth(-1);
//         });
//         
//         base.UseModule(app);
//     }
//
//     #if !NET10_0_OR_GREATER
//     /// <summary>
//     ///     添加文档
//     /// </summary>
//     /// <param name="customOptions"></param>
//     /// <param name="options"></param>
//     private static void AddDocs(SwaggerOptions customOptions, SwaggerGenOptions options)
//     {
//         if (customOptions?.Endpoints?.Count > 0)
//             foreach (var endpoint in customOptions.Endpoints)
//                 options.SwaggerDoc($"{endpoint.Version}",
//                     new OpenApiInfo { Title = endpoint.Title, Version = endpoint.Version });
//     }
//
//     /// <summary>
//     ///     添加权限
//     /// </summary>
//     /// <param name="options"></param>
//     private static void AddSecurity(SwaggerGenOptions options)
//     {
//         options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//         {
//             In = ParameterLocation.Header,
//             Type = SecuritySchemeType.ApiKey,
//             Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
//             Name = "Authorization",
//             BearerFormat = "JWT",
//             Scheme = "Bearer"
//         });
//
//         options.AddSecurityRequirement(new OpenApiSecurityRequirement
//         {
//             {
//                 new OpenApiSecurityScheme
//                 {
//                     Reference = new OpenApiReference
//                     {
//                         Type = ReferenceType.SecurityScheme,
//                         Id = "Bearer"
//                     }
//                 },
//                 Array.Empty<string>()
//             }
//         });
//     }
//
//     /// <summary>
//     ///     添加文档过滤器
//     /// </summary>
//     /// <param name="options"></param>
//     // ReSharper disable once UnusedMember.Local
//     private static void AddDocumentFilter(SwaggerGenOptions options)
//     {
//         // 枚举注释
//         options.SchemaFilter<EnumSchemaFilter>();
//         // 标签重排
//         options.DocumentFilter<TagReorderDocumentFilter>();
//     }
//
//     /// <summary>
//     ///     添加标签
//     /// </summary>
//     /// <param name="options"></param>
//     // ReSharper disable once UnusedMember.Local
//     private static void AddActionTag(SwaggerGenOptions options)
//     {
//         // 自定义维护标签数据
//         options.TagActionsBy(description =>
//         {
//             var tagAttribute = description.ActionDescriptor.EndpointMetadata.OfType<TagsAttribute>().FirstOrDefault();
//             if (tagAttribute != null) return tagAttribute.Tags.ToList();
//
//             return new List<string> { description.ActionDescriptor.As<ControllerActionDescriptor>().ControllerName };
//         });
//     }
//
//     /// <summary>
//     ///     添加Xml注释
//     /// </summary>
//     /// <param name="options"></param>
//     private static void AddXmlComments(SwaggerGenOptions options)
//     {
//         foreach (var file in Directory.GetFiles(AppContext.BaseDirectory, "*.xml"))
//             options.IncludeXmlComments(file);
//     }
//
//     /// <summary>
//     ///     添加自定义配置
//     /// </summary>
//     /// <param name="customOptions"></param>
//     /// <param name="options"></param>
//     private static void AddCustomOptions(SwaggerOptions customOptions, SwaggerGenOptions options)
//     {
//         // 参数描述小驼峰
//         if (customOptions is { AllParametersInCamelCase: true })
//             options.DescribeAllParametersInCamelCase();
//
//         // 自定义架构编号
//         options.CustomSchemaIds(x => x.ToString());
//     }
//     #endif
// }