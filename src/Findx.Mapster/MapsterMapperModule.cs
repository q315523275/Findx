using Findx.Mapping;
using Findx.Modularity;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel;

namespace Findx.Mapster
{
    [Description("Findx-对象映射模块")]
    public class MapsterMapperModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;
        public override int Order => 20;
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMapper, MapsterMapper>();
            return services;
        }
        public override void UseModule(IServiceProvider provider)
        {
            var option = provider.GetService<IOptions<MappingOptions>>();

            // 设置不区分大小写
            if (option.Value.IgnoreCase)
                TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);

            // 设置忽略Null值映射
            if (option.Value.IgnoreNullValues)
                TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);

            // 设置指定属性的字段映射
            TypeAdapterConfig.GlobalSettings.Default.IgnoreAttribute(typeof(IgnoreMappingAttribute));

            MapperExtensions.SetMapper(provider.GetRequiredService<IMapper>());

            base.UseModule(provider);
        }
    }
}
