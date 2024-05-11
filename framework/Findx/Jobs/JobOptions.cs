namespace Findx.Jobs;

/// <summary>
///     作业参数
/// </summary>
public class JobOptions : IOptions<JobOptions>
{
	/// <summary>
	///     是否启用
	/// </summary>
	public bool Enabled { set; get; }

	/// <summary>
	///     调度延迟时间
	///     <para>单位:秒</para>
	/// </summary>
	public int ScheduleDelay { set; get; } = 5;

	/// <summary>
	///		任务超期策略
	/// </summary>
	public JobMisfireStrategy MisfireStrategy { set; get; } = JobMisfireStrategy.DoNothing;

	/// <summary>
	///		调度模式
	/// </summary>
	public ScheduleType ScheduleType { set; get; } = ScheduleType.Simple;

	/// <summary>
	///     单次最大调度作业数
	/// </summary>
	public int MaxFetchJobCount { set; get; } = 100;

	/// <summary>
	///		服务端池大小
	/// </summary>
	public int ServerPoolSize { set; get; } = 1;

	/// <summary>
	///		客户端池大小
	/// </summary>
	public int ClientPoolSize { set; get; } = 5;
	
	/// <summary>
	///     option
	/// </summary>
	public JobOptions Value => this;
}