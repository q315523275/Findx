using System.Threading.Tasks;

namespace Findx.Sms;

/// <summary>
///     短信发送器
/// </summary>
public interface ISmsSender
{
    /// <summary>
    ///     发送
    /// </summary>
    /// <param name="sms"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task SendAsync(SmsMessage sms, CancellationToken token = default);
}