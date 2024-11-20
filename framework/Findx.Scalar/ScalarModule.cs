using System.ComponentModel;
using Findx.AspNetCore;
using Findx.Extensions;
using Findx.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace Findx.Scalar;

/// <summary>
///     
/// </summary>
[Description("Findx-Scalar文档模块")]
public class ScalarModule: MinimalModuleBase
{
    /// <summary>
    ///     等级
    /// </summary>
    public override ModuleLevel Level => ModuleLevel.Application;

    /// <summary>
    ///     排序
    /// </summary>
    public override int Order => 40;
    
    /// <summary>
    ///     配置
    /// </summary>
    private readonly ScalarOptionsX _optionsX = new();

    /// <summary>
    ///     配置服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApi(opt =>
        {
            opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            
            // opt.AddDocumentTransformer((document, context, _) =>
            // {
            //     document.Info.Title = "Todo API";
            //     document.Info.Version = "v1";
            //     return Task.CompletedTask;
            // });
            
            // opt.AddDocumentTransformer((document, context, _) =>
            // {
            //     document.Info.Title = "Todo API EleAdmin";
            //     document.Info.Version = "eleAdmin";
            //     return Task.CompletedTask;
            // });
        });
        return services;
    }
    
    public override void UseModule(WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
        
        // app.MapScalarApiReference(options =>
        // {
        //     options.ProxyUrl = "213213";
        //     options.Authentication = new ScalarAuthenticationOptions()
        //     {
        //         ApiKey = new ApiKeyOptions { Token = "Jwt" },
        //         PreferredSecurityScheme = "Bearer"
        //     };
        //     options.Servers = new List<ScalarServer>()
        //     {
        //         new("/", "测试"), new("/test", "测试2")
        //     };
        //     options.Theme = ScalarTheme.Alternate;
        //     options.DarkMode = false;
        //     options.HideDownloadButton = true;
        //     options.WithApiKeyAuthentication(op1 => { op1.Token = "Jwt"; });
        //     options.WithDefaultOpenAllTags(true);
        //     options.WithTitle("eleadmin");
        // }); // scalar/v1
        
        base.UseModule(app);
    }
}