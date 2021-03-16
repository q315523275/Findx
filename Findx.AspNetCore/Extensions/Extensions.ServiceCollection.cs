using Findx.Builders;
using Findx.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore.Extensions
{
    public static partial class Extensions
    {
        public static IServiceCollection AddFindx(this IServiceCollection services)
        {
            // 框架构建
            IFindxBuilder builder = services.GetOrAddSingletonInstance<IFindxBuilder>(() => new FindxBuilder(services));
            // 构建模块
            builder.AddModules();
            // 
            return services;
        }
    }
}
