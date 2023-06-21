namespace Findx.DependencyInjection;

/// <summary>
/// 服务别名接口
/// </summary>
public interface IServiceNameAware
{
    /// <summary>
    /// 服务名称
    /// </summary>
    string Name { get; }
}