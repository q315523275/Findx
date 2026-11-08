namespace Findx.Data;

/// <summary>
///     审计完整性校验结果
/// </summary>
public class AuditIntegrityValidationResult
{
    /// <summary>
    ///     是否通过
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    ///     断点审计记录Id（0 表示无断点）
    /// </summary>
    public long BrokenAtId { get; set; }

    /// <summary>
    ///     断点说明
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     参与校验的记录总数
    /// </summary>
    public int TotalCount { get; set; }
}
