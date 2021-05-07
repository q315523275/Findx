using System;
using System.Collections.Generic;

namespace Findx.DependencyInjection
{
    public interface IScopedServiceResolver
    {
        bool ResolveEnabled { get; }
        T GetService<T>();
        object GetService(Type serviceType);
        IEnumerable<T> GetServices<T>();
        IEnumerable<object> GetServices(Type serviceType);
    }
}
