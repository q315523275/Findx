using Findx.Modularity;
using Microsoft.AspNetCore.Builder;

namespace Findx.AspNetCore;

/// <summary>
///     AspNetCore模块基类
/// </summary>
public abstract class MinimalModuleBase : StartupModule
{
    /// <summary>
    ///     应用AspNetCore的服务业务
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    public virtual void UseModule(WebApplication app)
    {
        base.UseModule(app.Services);
    }
}