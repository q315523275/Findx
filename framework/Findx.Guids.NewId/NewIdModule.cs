using System.ComponentModel;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Findx.Guids.NewId
{
    [Description("Findx-NewId模块")]
    public class NewIdModule : StartupModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;

        public override int Order => 140;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.Replace(new ServiceDescriptor(typeof(IGuidGenerator), typeof(Guids.SequentialGuidGenerator), ServiceLifetime.Singleton));
            return services;
        }
    }
}