using Findx.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore.Mvc.Filters
{
	/// <summary>
	/// 自动提交工作单元事务
	/// </summary>
	public class UnitOfWorkAttribute : ActionFilterAttribute
	{
		/// <summary>
		/// 数据连接标识,不传默认使用主连接
		/// </summary>
		public string DbKey { get; set; }
		
		/// <summary>
		/// Called before the action executes, after model binding is complete.
		/// </summary>
		/// <param name="context"></param>
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			// 初始化工作单元
			context.HttpContext.RequestServices.GetService<IUnitOfWorkManager>()
				?.GetConnUnitOfWork(true, DbKey)
				?.BeginOrUseTransaction();
		}

		/// <summary>
		/// Called after the action executes, before the action result.
		/// </summary>
		/// <param name="context"></param>
		public override void OnActionExecuted(ActionExecutedContext context)
		{
		    var unitOfWork = context.HttpContext.RequestServices.GetService<IUnitOfWorkManager>()?.GetConnUnitOfWork(false, DbKey);

		    if (context.Result is JsonResult result1)
			{
				if (result1.Value is CommonResult ajax)
				{
					if (ajax.IsSuccess())
					{
						unitOfWork?.Commit();
					}
				}
			}
			else if (context.Result is ObjectResult result2)
			{
				if (result2.Value is CommonResult ajax)
				{
					if (ajax.IsSuccess())
					{
						unitOfWork?.Commit();
					}
				}
			}
			else if (context.HttpContext.Response.StatusCode < 400)
			{
				unitOfWork?.Commit();
			}
		}
	}
}

