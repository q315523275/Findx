namespace Findx.Messaging;

/// <summary>
///     应用事件属性
/// </summary>
public class ApplicationEventProperty
{
    /// <summary>
    ///     工作单元提交后推送
    /// </summary>
    public bool AfterUnitOfWorkCommit { set; get; }
}