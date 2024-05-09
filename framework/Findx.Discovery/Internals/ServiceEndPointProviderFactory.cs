using Findx.DependencyInjection;
using Findx.Discovery.Abstractions;

namespace Findx.Discovery.Internals;

/// <summary>
///     服务端点提供器工厂   
/// </summary>
public class ServiceEndPointProviderFactory: ServiceFactoryBase<IServiceEndPointProvider>, IServiceEndPointProviderFactory;