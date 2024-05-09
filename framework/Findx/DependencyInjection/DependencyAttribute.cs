namespace Findx.DependencyInjection;

/// <summary>
///     依赖注入注解
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
// ReSharper disable once ClassNeverInstantiated.Global
public class DependencyAttribute : Attribute
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="lifetime">生命周期</param>
    public DependencyAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }

    /// <summary>
    ///     生命周期
    /// </summary>
    public ServiceLifetime Lifetime { get; }

    /// <summary>
    ///     不存在则注册
    /// </summary>
    public bool TryRegister { get; set; }

    /// <summary>
    ///     是否替换已存在服务
    /// </summary>
    public bool ReplaceServices { get; set; }

    /// <summary>
    ///     注册自身
    /// </summary>
    public bool AddSelf { get; set; }
}