using System;
using Autofac;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Findx.Extensions;
namespace Findx.Autofac
{
	public static class AutofacServiceCollectionExtensions
	{
        [NotNull]
        public static ContainerBuilder GetContainerBuilder([NotNull] this IServiceCollection services)
        {
            Check.NotNull(services, nameof(services));

            var builder = services.GetSingletonInstanceOrNull<ContainerBuilder>();
            if (builder == null)
            {
                throw new Exception($"Could not find ContainerBuilder");
            }

            return builder;
        }

        public static IServiceProvider BuildAutofacServiceProvider([NotNull] this IServiceCollection services, Action<ContainerBuilder> builderAction = null)
        {
            return services.BuildServiceProviderFromFactory(builderAction);
        }
    }
}

