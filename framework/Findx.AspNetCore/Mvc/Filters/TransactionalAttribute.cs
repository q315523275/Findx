#nullable enable
using System;
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
	///     工作单元标识
	/// </summary>
	public string? DataSource { get; set; }

	/// <summary>
	///     实体类型
	/// </summary>
	public Type? EntityType { get; set; }

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
		var unitOfWorkManager = provider.GetRequiredService<IUnitOfWorkManager>();
		
		// 初始化工作单元
		IUnitOfWork unitOfWork;
		if (EntityType != null)
			unitOfWork = await unitOfWorkManager.GetEntityUnitOfWorkAsync(EntityType, true, cancellationToken);
		else
			unitOfWork = await unitOfWorkManager.GetUnitOfWorkAsync(DataSource, true, cancellationToken);
		
		// 开启事物
		if (IsolationLevel.HasValue)
			await unitOfWork.BeginOrUseTransactionAsync(IsolationLevel.Value, cancellationToken);
		else
			await unitOfWork.BeginOrUseTransactionAsync(cancellationToken);
		
		// 执行业务
		var actionContext = await next();

		if (actionContext.Exception != null)
		{
			await unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
		}
		else switch (actionContext.Result)
		{
			case JsonResult result1:
			{
				if (result1.Value is CommonResult ajax)
				{
					if (ajax.IsSuccess())
						await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
					else
						await unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
				}
				else
				{
					await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
				}
				break;
			}
			case ObjectResult result2:
			{
				if (result2.Value is CommonResult ajax)
				{
					if (ajax.IsSuccess())
						await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
					else
						await unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
				}
				else
				{
					await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
				}
				break;
			}
			default:
			{
				if (actionContext.HttpContext.Response.StatusCode < 400)
				{
					await unitOfWork.CommitAsync(cancellationToken);
				}
				else
				{
					await unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
				}
				break;
			}
		}
	}
}