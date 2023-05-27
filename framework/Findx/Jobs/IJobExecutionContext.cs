namespace Findx.Jobs;

/// <summary>
///     定义一个作业上下文信息
/// </summary>
public interface IJobExecutionContext
{
	/// <summary>
	///     服务提供器
	/// </summary>
	IServiceProvider ServiceProvider { get; }

	/// <summary>
	///     作业编号
	/// </summary>
	long JobId { get; }

	/// <summary>
	///     作业名称
	/// </summary>
	string JobName { get; }

	/// <summary>
	///     作业执行编号
	/// </summary>
	long ExecutionId { get; }

	/// <summary>
	///     作业类全名称
	/// </summary>
	string FullName { get; }

	/// <summary>
	///     作业参数
	/// </summary>
	IDictionary<string, string> Parameter { get; }

	/// <summary>
	///     作业分片索引
	/// </summary>
	int ShardIndex { get; }

	/// <summary>
	///     作业分片总数
	/// </summary>
	int ShardTotal { get; }
}