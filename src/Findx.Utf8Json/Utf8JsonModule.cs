using Findx.Extensions;
using Findx.Modularity;
using Findx.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Findx.Utf8Json
{
    [Description("Findx-Utf8Json序列化模块")]
    public class Utf8JsonModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Application;
        public override int Order => 20;
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.Replace<ISerializer, Utf8JsonSerializer>(ServiceLifetime.Singleton);
            services.Replace<IJsonSerializer, Utf8JsonJsonSerializer>(ServiceLifetime.Singleton);

            return services;
        }
    }
}
