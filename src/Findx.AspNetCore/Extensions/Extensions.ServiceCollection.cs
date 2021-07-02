using Findx.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore.Extensions
{
    public static partial class Extensions
    {
        /// <summary>
        /// 添加Razor和读取Razor绑定后内容
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddRazorPageAndRenderer(this IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddTransient<IRazorViewRenderer, RazorViewRenderer>();

            return services;
        }
    }
}
