using Findx.Common;

namespace Findx.Sms;

/// <summary>
///     短信消息
/// </summary>
public class SmsMessage
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="phone"></param>
    /// <param name="text"></param>
    public SmsMessage(string phone, string text)
    {
        Check.NotNull(phone, nameof(phone));
        Check.NotNull(text, nameof(text));

        Phone = phone;
        Text = text;

        Parameter = new Dictionary<string, object>();
    }

    /// <summary>
    ///     接收手机号
    /// </summary>
    public string Phone { get; }

    /// <summary>
    ///     短信文本
    /// </summary>
    public string Text { get; }

    /// <summary>
    ///     短信参数
    /// </summary>
    public IDictionary<string, object> Parameter { get; }
}