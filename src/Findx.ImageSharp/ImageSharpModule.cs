using Findx.Drawing;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;

namespace Findx.ImageSharp
{
    [Description("Findx-图像处理组件模块")]
    public class ImageSharpModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;
        public override int Order => 100;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IImageProcessor, ImageProcessor>();

            return services;
        }

        public override void UseModule(IServiceProvider provider)
        {

            base.UseModule(provider);
        }
    }
}
