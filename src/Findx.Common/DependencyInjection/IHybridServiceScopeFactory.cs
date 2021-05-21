using Microsoft.Extensions.DependencyInjection;

namespace Findx.DependencyInjection
{
    /// <summary>
    /// 作用域工厂,适配当前域
    /// </summary>
    public interface IHybridServiceScopeFactory : IServiceScopeFactory
    {
    }
}
