using DinkToPdf;
using DinkToPdf.Contracts;
using Findx.Modularity;
using Findx.Pdf;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Findx.DinkToPdf
{
    [Description("Findx-Pdf模块")]
    public class DinkToPdfModule : FindxModule
    {
        public override ModuleLevel Level => ModuleLevel.Framework;

        public override int Order => 30;

        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddSingleton<IPdfConverter, DinkToPdfConverter>();

            return services;
        }

    }
}
