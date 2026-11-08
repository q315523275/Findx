using System.Threading.Tasks;

namespace Findx.Sms;

/// <summary>
/// </summary>
public class NullSmsSender : ISmsSender
{
    private readonly ILogger<NullSmsSender> _logger;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="logger"></param>
    public NullSmsSender(ILogger<NullSmsSender> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     发送
    /// </summary>
    /// <param name="sms"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task SendAsync(SmsMessage sms, CancellationToken token = default)
    {
        //  Null 对象模式：不实际发送短信，仅记录日志，避免未配置短信通道时直接抛异常中断业务
        _logger.LogDebug("NullSmsSender 不实际发送短信，接收号码：{Phone}，内容：{Text}", sms.Phone, sms.Text);
        return Task.CompletedTask;
    }
}