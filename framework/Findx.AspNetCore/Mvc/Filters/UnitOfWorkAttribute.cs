using System.Threading.Tasks;
using Findx.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore.Mvc.Filters;

/// <summary>
///     自动提交工作单元事务
/// </summary>
public class UnitOfWorkAttribute : ActionFilterAttribute
{
	/// <summary>
	///     数据连接标识,不传默认使用主连接
	/// </summary>
	public string DbKey { get; set; } = null;
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="context"></param>
	/// <param name="next"></param>
	public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var provider = context.HttpContext.RequestServices;
		var cancellationToken = context.HttpContext.RequestAborted;
		
		// 初始化工作单元
		// ReSharper disable once PossibleNullReferenceException
		var uow = await	provider.GetService<IUnitOfWorkManager>()?.GetConnUnitOfWorkAsync(true, true, DbKey, cancellationToken);

		// 执行业务
		var actionContext = await next();

		if (uow == null) return;
		
		if (actionContext.Exception != null)
		{
			await uow.RollbackAsync(cancellationToken).ConfigureAwait(false);
		}
		else switch (actionContext.Result)
		{
			case JsonResult result1:
			{
				if (result1.Value is CommonResult ajax && ajax.IsSuccess()) 
					await uow.CommitAsync(cancellationToken);
				break;
			}
			case ObjectResult result2:
			{
				if (result2.Value is CommonResult ajax && ajax.IsSuccess()) 
					await uow.CommitAsync(cancellationToken);
				break;
			}
			default:
			{
				if (actionContext.HttpContext.Response.StatusCode < 400)
				{
					await uow.CommitAsync(cancellationToken);
				}

				break;
			}
		}
	}
}