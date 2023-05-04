using System;
using System.ComponentModel;
using Findx.Mapping;
using Findx.Modularity;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Findx.Mapster
{
    /// <summary>
    ///     Findx-对象映射模块
    /// </summary>
    [Description("Findx-对象映射模块")]
    public class MapsterMapperModule : FindxModule
    {
        /// <summary>
        ///     模块等级
        /// </summary>
        public override ModuleLevel Level => ModuleLevel.Framework;

        /// <summary>
        ///     模块排序
        /// </summary>
        public override int Order => 50;

        /// <summary>
        ///     配置模块服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMapper, MapsterMapper>();
            return services;
        }

        /// <summary>
        ///     启用模块
        /// </summary>
        /// <param name="provider"></param>
        public override void UseModule(IServiceProvider provider)
        {
            var option = provider.GetService<IOptions<MappingOptions>>();

            // 设置不区分大小写
            if (option != null && option.Value.IgnoreCase)
                TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);

            // 设置忽略Null值映射
            if (option != null && option.Value.IgnoreNullValues)
                TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);

            // 设置指定属性的字段映射
            TypeAdapterConfig.GlobalSettings.Default.IgnoreAttribute(typeof(IgnoreMappingAttribute));

            MapperExtensions.SetMapper(provider.GetRequiredService<IMapper>());

            base.UseModule(provider);
        }
    }
}