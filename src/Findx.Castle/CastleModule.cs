using System;
using Findx.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Castle
{
	public class CastleModule : FindxModule
	{
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddTransient(typeof(CastleAsyncDeterminationInterceptor<>));

            return services;
        }
    }
}

