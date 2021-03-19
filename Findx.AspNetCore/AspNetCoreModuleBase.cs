using Findx.Modularity;
using Microsoft.AspNetCore.Builder;

namespace Findx.AspNetCore
{
    public abstract class AspNetCoreModuleBase : FindxModule
    {
        /// <summary>
        /// 应用AspNetCore的服务业务
        /// </summary>
        /// <param name="app">应用程序构建器</param>
        public virtual void UseModule(IApplicationBuilder app) => base.UseModule(app.ApplicationServices);
    }
}
