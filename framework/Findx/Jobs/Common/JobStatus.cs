namespace Findx.Jobs.Common;

/// <summary>
///     任务状态枚举
/// </summary>
public enum JobStatus
{
	/// <summary>
	///     正常
	/// </summary>
	Normal = 1,

	/// <summary>
	///     停止
	/// </summary>
	Stop = 2,

	/// <summary>
	///     死信隔离：连续失败达到上限后停止调度，等待人工放行
	/// </summary>
	Dead = 3
}