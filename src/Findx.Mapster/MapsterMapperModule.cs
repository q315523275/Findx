using Findx.Mapping;
using Findx.Modularity;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
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
            // 设置不区分大小写
            TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);
            MapperExtensions.SetMapper(provider.GetRequiredService<IMapper>());

            base.UseModule(provider);
        }
    }
}
