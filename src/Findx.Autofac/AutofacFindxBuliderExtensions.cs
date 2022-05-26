using Autofac;
using Findx.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Autofac
{
    public static class AutofacAbpApplicationCreationOptionsExtensions
	{

        public static void UseAutofac(this IFindxBuilder build)
        {
            build.Services.AddAutofacServiceProviderFactory();
        }

        public static AutofacServiceProviderFactory AddAutofacServiceProviderFactory(this IServiceCollection services)
        {
            return services.AddAutofacServiceProviderFactory(new ContainerBuilder());
        }

        public static AutofacServiceProviderFactory AddAutofacServiceProviderFactory(this IServiceCollection services, ContainerBuilder containerBuilder)
        {
            var factory = new AutofacServiceProviderFactory(containerBuilder);

            services.AddSingleton(containerBuilder);
            services.AddSingleton((IServiceProviderFactory<ContainerBuilder>)factory);

            return factory;
        }
    }
}

