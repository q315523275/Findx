using Findx.Aspects;
using System;

namespace Findx.DynamicProxy
{
    internal sealed class CastleProxyFactory : IProxyFactory
    {
        private readonly IServiceProvider _serviceProvider;


        public object CreateProxy(Type serviceType, object[] arguments)
        {
            Check.NotNull(serviceType, nameof(serviceType));
            throw new NotImplementedException();
        }

        public object CreateProxy(Type serviceType, Type implementType, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        public object CreateProxyWithTarget(Type serviceType, object implement, object[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}
