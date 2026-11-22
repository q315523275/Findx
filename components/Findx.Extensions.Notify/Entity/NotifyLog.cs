namespace Findx.Extensions.Notify.Entity;

/// <summary>
///     通知发送日志表。异步入队时在业务线程零 IO，由消费线程发送完成后一次性落库
/// </summary>
[Table(Name = "notify_logs")]
[Description("通知发送日志")]
[EntityExtension(DataSource = "Notify", DisableAuditing = true)]
public class NotifyLog : FullAuditedBase<long, long>
{
    /// <summary>
    ///     通道
    /// </summary>
    public NotifyChannel Channel { get; set; }

    /// <summary>
    ///     标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     正文（展示用，过长截断；完整内容见 Payload）
    /// </summary>
    [Column(StringLength = 2000)]
    public string Content { get; set; }

    /// <summary>
    ///     接收方（多个逗号拼接）
    /// </summary>
    [Column(StringLength = 500)]
    public string Receivers { get; set; }

    /// <summary>
    ///     模板编号
    /// </summary>
    [Column(StringLength = 100)]
    public string TemplateId { get; set; }

    /// <summary>
    ///     发送状态
    /// </summary>
    public NotifyLogStatus Status { get; set; }

    /// <summary>
    ///     是否成功（冗余列，便于按结果筛选）
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    ///     失败原因
    /// </summary>
    [Column(StringLength = 500)]
    public string ErrorMessage { get; set; }

    /// <summary>
    ///     业务关联键（如单据号、流程实例号），便于按业务查通知
    /// </summary>
    [Column(StringLength = 100)]
    public string BusinessKey { get; set; }

    /// <summary>
    ///     发送节点名（多实例部署时定位是哪个节点发的）
    /// </summary>
    [Column(StringLength = 100)]
    public string NodeName { get; set; }

    /// <summary>
    ///     完整消息快照（JSON，可据此重放）
    /// </summary>
    [Column(StringLength = -1)]
    public string Payload { get; set; }

    /// <summary>
    ///     发送时间
    /// </summary>
    public DateTime? SendTime { get; set; }

    /// <summary>
    ///     发送耗时（毫秒）
    /// </summary>
    public long ElapsedMs { get; set; }

    /// <summary>
    ///     由消息构造日志
    ///     <para>主键由调用方用 IKeyGenerator 生成后传入，本方法不做任何 IO</para>
    /// </summary>
    /// <param name="message">通知消息</param>
    /// <param name="nodeName">发送节点名</param>
    /// <param name="id">主键</param>
    public static NotifyLog From(NotifyMessage message, string nodeName, long id)
    {
        var log = new NotifyLog
        {
            Channel = message.Channel,
            Title = Truncate(message.Title, 200),
            Content = Truncate(message.Content, 2000),
            Receivers = message.To is { Length: > 0 } ? Truncate(string.Join(",", message.To), 500) : null,
            TemplateId = Truncate(message.TemplateId, 100),
            BusinessKey = Truncate(message.BusinessKey, 100),
            NodeName = Truncate(nodeName, 100),
            Payload = message.ToJson(),
            Status = NotifyLogStatus.Queued,
            CreatedTime = DateTime.Now,
            //  入队即带主键：QueueAsync 直接把 Id 返回给业务用于后续反查，无需等落库
            Id = id
        };
        return log;
    }

    /// <summary>
    ///     回填发送结果
    /// </summary>
    public void Apply(NotifyResult result, DateTime sendTime, long elapsedMs)
    {
        Success = result.Success;
        Status = result.Success ? NotifyLogStatus.Success : NotifyLogStatus.Failed;
        ErrorMessage = result.Success ? null : Truncate(result.Message, 500);
        SendTime = sendTime;
        ElapsedMs = elapsedMs;
        LastUpdatedTime = sendTime;
    }

    /// <summary>
    ///     由日志还原消息（优先用 Payload 完整快照）
    /// </summary>
    public NotifyMessage ToMessage()
    {
        if (!string.IsNullOrWhiteSpace(Payload))
        {
            try
            {
                var message = Payload.ToObject<NotifyMessage>();
                if (message != null) return message;
            }
            catch
            {
                //  Payload 损坏时回退到列字段重建，保证消息仍可发出
            }
        }

        return new NotifyMessage
        {
            Channel = Channel,
            Title = Title,
            Content = Content,
            TemplateId = TemplateId,
            To = Receivers?.Split(',')
        };
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength) return value;
        return value[..maxLength];
    }
}
