namespace Findx.Jobs.Client;

/// <summary>
///     定义一个作业属性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class JobAttribute : Attribute
{
	/// <summary>
	///     作业名称
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	///     作业描述
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	///     间隔时间
	/// </summary>
	public string Interval { get; set; }

	/// <summary>
	///     Cron表达式
	/// </summary>
	public string Cron { get; set; }

	/// <summary>
	///     是否允许并行
	/// </summary>
	public bool IsParallel { get; set; }

	/// <summary>
	///     最大并行数量
	/// </summary>
	public int IterationLimit { get; set; } = -1;
}