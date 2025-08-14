using Findx.Common;
using Findx.Common.DisposableEx;

namespace Findx.Tracing;

/// <summary>
///     默认跟踪标识提供程序
/// </summary>
public class DefaultCorrelationIdProvider: ICorrelationIdProvider
{
    private static readonly AsyncLocal<string> ValueAccessor = new();

    private string CorrelationId => ValueAccessor.Value;
    
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
        ValueAccessor.Value = correlationId;
        return new ActionDisposable(() =>
        {
            ValueAccessor.Value = parent;
        });
    }
}