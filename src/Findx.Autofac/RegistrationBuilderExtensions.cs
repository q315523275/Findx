using System;
using System.Linq;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Extras.DynamicProxy;
using Findx.Castle;

namespace Findx.Autofac
{
	public static class RegistrationBuilderExtensions
	{
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> AddInterceptors<TLimit,
            TActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registrationBuilder,
            params Type[] interceptors)
            where TActivatorData : ReflectionActivatorData
        {
            var serviceType = registrationBuilder.RegistrationData.Services.OfType<IServiceWithType>().FirstOrDefault()
                ?.ServiceType;
            if (serviceType == null)
            {
                throw new Exception("Failed to get the specified registration type");
            }

            if (serviceType.IsInterface)
            {
                registrationBuilder = registrationBuilder.EnableInterfaceInterceptors();
            }
            else
            {
                (registrationBuilder as IRegistrationBuilder<TLimit, ConcreteReflectionActivatorData, TRegistrationStyle>)?.EnableClassInterceptors();
            }

            foreach (var interceptor in interceptors)
            {
                registrationBuilder.InterceptedBy(typeof(CastleAsyncDeterminationInterceptor<>).MakeGenericType(interceptor));
            }

            return registrationBuilder;
        }
    }
}

