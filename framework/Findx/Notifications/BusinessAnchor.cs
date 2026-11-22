namespace Findx.Notifications;

/// <summary>
///     业务锚点：把一条通知/待办关联回业务单据
///     <para>审批、通知、消息中心都需要这套字段，各自散着写就会出现"审批存 BusinessId、
///     通知存 BusinessKey"这类对不上的情况，统一放在核心包里当公共语言</para>
/// </summary>
public class BusinessAnchor
{
    /// <summary>
    ///     业务类型标识，如 pms.order
    /// </summary>
    public string BusinessType { get; set; }

    /// <summary>
    ///     业务主键
    /// </summary>
    public string BusinessId { get; set; }

    /// <summary>
    ///     业务单号，如 CGDD2611150001
    /// </summary>
    public string BusinessCode { get; set; }

    /// <summary>
    ///     业务标题（一般是往来单位名、项目名等便于识别的信息）
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     打开业务单据的地址，前端据此跳转
    /// </summary>
    public string Url { get; set; }
}
