using System.Data;
using System.Threading.Tasks;
using Findx.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore.Mvc.Filters;

/// <summary>
///     自动提交工作单元事务
/// </summary>
public class TransactionalAttribute : ActionFilterAttribute
{
	/// <summary>
	///     数据连接标识,不传默认使用主连接
	/// </summary>
	// ReSharper disable once MemberCanBePrivate.Global
	public string DataSource { get; set; } = null;

	/// <summary>
	///		事物级别
	/// </summary>
	public IsolationLevel? IsolationLevel { get; set; } = null;
	
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
		var uow = await	provider.GetService<IUnitOfWorkManager>()?.GetConnUnitOfWorkAsync(true, true, DataSource, cancellationToken);

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
				if (result1.Value is CommonResult ajax)
				{
					if (ajax.IsSuccess())
						await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
					else
						await uow.RollbackAsync(cancellationToken).ConfigureAwait(false);
				}
				else
				{
					await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
				}
				break;
			}
			case ObjectResult result2:
			{
				if (result2.Value is CommonResult ajax)
				{
					if (ajax.IsSuccess())
						await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
					else
						await uow.RollbackAsync(cancellationToken).ConfigureAwait(false);
				}
				else
				{
					await uow.CommitAsync(cancellationToken).ConfigureAwait(false);
				}
				break;
			}
			default:
			{
				if (actionContext.HttpContext.Response.StatusCode < 400)
				{
					await uow.CommitAsync(cancellationToken);
				}
				else
				{
					await uow.RollbackAsync(cancellationToken).ConfigureAwait(false);
				}
				break;
			}
		}
	}
}