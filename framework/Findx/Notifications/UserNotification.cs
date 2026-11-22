using System.Collections.Generic;

namespace Findx.Notifications;

/// <summary>
///     用户通知
///     <para>只描述"发给谁、说什么、关联哪张单据"三件事，通道差异（模板、@某人、小程序页面等）
///     一律不进来，否则这个契约又会被某个厂商绑死</para>
/// </summary>
public class UserNotification
{
    /// <summary>
    ///     接收方。用户标识，语义由实现解释：用户编号、账号、邮箱或 openid
    /// </summary>
    public string[] Receivers { get; set; }

    /// <summary>
    ///     标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     正文
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    ///     是否按 markdown 排版（支持的通道才会生效）
    /// </summary>
    public bool IsMarkdown { get; set; }

    /// <summary>
    ///     关联的业务单据，接收方据此跳转处理
    /// </summary>
    public BusinessAnchor Business { get; set; }

    /// <summary>
    ///     模板变量。键为实现方约定的占位字段名
    /// </summary>
    public IDictionary<string, object> Data { get; set; }
}
