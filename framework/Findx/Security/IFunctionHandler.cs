using System.Threading.Tasks;

namespace Findx.Security;

/// <summary>
///     定义功能信息处理器
/// </summary>
public interface IFunctionHandler
{
	/// <summary>
	///     从程序集中获取功能信息（如MVC的Controller-Action）
	/// </summary>
	Task InitializeAsync(CancellationToken cancellationToken = default);
	
	/// <summary>
	///     刷新功能信息缓存
	/// </summary>
	Task RefreshCacheAsync(CancellationToken cancellationToken = default);

	/// <summary>
	///     清空功能信息缓存
	/// </summary>
	Task ClearCacheAsync(CancellationToken cancellationToken = default);

	/// <summary>
	///     查找指定条件的功能信息
	/// </summary>
	/// <param name="area">区域</param>
	/// <param name="controller">控制器</param>
	/// <param name="action">功能方法</param>
	/// <param name="method">方法</param>
	/// <returns>功能信息</returns>
	IFunction GetFunction(string area, string controller, string action, string method);
}