using Findx.Common.DisposableEx;

namespace Findx.Tracing;

/// <summary>
///     默认跟踪标识提供程序
/// </summary>
/// <remarks>
///     内部基于 AsyncLocal 保存上下文，须以单例（Singleton）方式注册到 DI 容器（见 FindxCoreModule）；
///     若注册为 Scoped/Transient，不同实例将各自持有独立上下文，跟踪标识将无法跨组件传播。
/// </remarks>
public class DefaultCorrelationIdProvider: ICorrelationIdProvider
{
    private readonly AsyncLocal<string> _valueAccessor = new();

    private string CorrelationId => _valueAccessor.Value;
    
    /// <summary>
    ///     获取跟踪标识
    /// </summary>
    /// <returns></returns>
    public string Get() => CorrelationId;

    /// <summary>
    ///      变更跟踪标识
    /// </summary>
    /// <param name="correlationId"></param>
    public virtual IDisposable Change(string correlationId)
    {
        var parent = CorrelationId;
        _valueAccessor.Value = correlationId;
        return new ActionDisposable(() =>
        {
            _valueAccessor.Value = parent;
        });
    }
}