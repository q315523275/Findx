namespace Findx.Threading;

/// <summary>
///     取消令牌提供程序
/// </summary>
public interface ICancellationTokenProvider
{
    /// <summary>
    ///     令牌Token
    /// </summary>
    CancellationToken Token { get; }
}