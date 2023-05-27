namespace Findx.Threading;

/// <summary>
///     取消Token信号提供器
/// </summary>
public class NullCancellationTokenProvider : ICancellationTokenProvider
{
    /// <summary>
    ///     Token信号
    /// </summary>
    public CancellationToken Token => CancellationToken.None;
}