using Findx.Data;
using Findx.Logging;

namespace Findx.Security
{
	/// <summary>
	/// 功能信息处理基类
	/// </summary>
	public abstract class FunctionHandlerBase<TFunction> : IFunctionHandler
		where TFunction : class, IEntity<Guid>, IFunction, new()
	{
		private readonly List<TFunction> _functions = new();
		private readonly IFunctionStore<TFunction> _store;
		private readonly StartupLogger _startupLogger;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="store"></param>
		/// <param name="startupLogger"></param>
		protected FunctionHandlerBase(IFunctionStore<TFunction> store, StartupLogger startupLogger)
		{
			_store = store;
			_startupLogger = startupLogger;
		}
		
		/// <summary>
		/// 从程序集中获取功能信息（如MVC的Controller-Action）
		/// </summary>
		public void Initialize()
		{
			var functions = GetFunctions();
			_store.SyncToDatabase(functions);
			RefreshCache();
		}

		/// <summary>
		/// 查找指定条件的功能信息
		/// </summary>
		/// <param name="area">区域</param>
		/// <param name="controller">控制器</param>
		/// <param name="action">功能方法</param>
		/// <returns>功能信息</returns>
		public IFunction GetFunction(string area, string controller, string action)
		{
			if (_functions.Count == 0)
			{
				RefreshCache();
			}

			return _functions.FirstOrDefault(m =>
						string.Equals(m.Area, area, StringComparison.OrdinalIgnoreCase)
						&& string.Equals(m.Controller, controller, StringComparison.OrdinalIgnoreCase)
						&& string.Equals(m.Action, action, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// 刷新功能信息缓存
		/// </summary>
		public void RefreshCache()
		{
			_functions.Clear();
			_functions.AddRange(_store.GetFromDatabase());
			_startupLogger.LogInformation($"刷新功能信息缓存，从数据库获取到 {_functions.Count} 个功能信息", GetType().Name);
		}

		/// <summary>
		/// 清空功能信息缓存
		/// </summary>
		public void ClearCache()
		{
			_functions.Clear();
		}

		/// <summary>
		/// 获取所有功能
		/// </summary>
		/// <param name="fromCache"></param>
		/// <returns></returns>
		protected abstract List<TFunction> GetFunctions(bool fromCache = true);
	}
}

