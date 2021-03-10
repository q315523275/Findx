namespace Findx.DependencyInjection
{
    public interface IScopedServiceResolver
    {
        bool ResolveEnabled { get; }
        T GetService<T>();
    }
}
