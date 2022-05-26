using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Findx.Autofac
{
    public static class AutofacHostBuilderExtensions
	{
        public static IHostBuilder UseAutofac(this IHostBuilder hostBuilder)
        {
            var containerBuilder = new ContainerBuilder();

            return hostBuilder.ConfigureServices((_, services) => { services.AddSingleton(containerBuilder); }).UseServiceProviderFactory(new AutofacServiceProviderFactory(containerBuilder));
        }
    }
}
