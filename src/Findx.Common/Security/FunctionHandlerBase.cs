using System;
using System.Collections.Generic;
using System.Linq;
using Findx.Data;
using Findx.Extensions;
using Microsoft.Extensions.Logging;

namespace Findx.Security
{
	/// <summary>
	/// 功能信息处理基类
	/// </summary>
	public abstract class FunctionHandlerBase<TFunction> : IFunctionHandler
		where TFunction : class, IEntity<Guid>, IFunction, new()
	{
		private readonly List<TFunction> _functions = new();
		private readonly IServiceProvider _serviceProvider;
		private readonly IFunctionStore<TFunction> _store;

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="serviceProvider"></param>
		/// <param name="store"></param>
		/// <param name="logger"></param>
		protected FunctionHandlerBase(IServiceProvider serviceProvider, IFunctionStore<TFunction> store, ILogger logger)
		{
			_serviceProvider = serviceProvider;
			_store = store;
			Logger = logger;
		}
		
		/// <summary>
		/// 获取 日志记录对象
		/// </summary>
		private ILogger Logger { get; }

		/// <summary>
		/// 从程序集中获取功能信息（如MVC的Controller-Action）
		/// </summary>
		public void Initialize()
		{
			var functions = GetFunctions();
			_serviceProvider.ExecuteScopedWork(provider =>
			{
			   _store.SyncToDatabase(functions);
			});
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
			_serviceProvider.ExecuteScopedWork(provider =>
			{
				_functions.Clear();
				_functions.AddRange(_store.GetFromDatabase());
				Logger.LogInformation($"刷新功能信息缓存，从数据库获取到 {_functions.Count} 个功能信息");
			});
		}

		/// <summary>
		/// 清空功能信息缓存
		/// </summary>
		public void ClearCache()
		{
			_functions.Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fromCache"></param>
		/// <returns></returns>
		protected abstract List<TFunction> GetFunctions(bool fromCache = true);
	}
}

