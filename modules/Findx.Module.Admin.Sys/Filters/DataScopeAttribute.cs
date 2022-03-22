using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Findx.Security;
using Findx.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Findx.Module.Admin.Sys.Service;

namespace Findx.Module.Admin.Sys.Filters
{
    public class DataScopeAttribute : ActionFilterAttribute
	{
		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (context.HttpContext.User.Identity.IsAuthenticated)
			{
				var currentUser = context.HttpContext.RequestServices.GetService<ICurrentUser>();
				var userId = currentUser.UserId.To<long>();
				var userService = context.HttpContext.RequestServices.GetService<ISysUserService>();
				var dataScope = await userService.GetUserDataScopeIdList(userId);
				context.HttpContext.Items.Add("dataScope", dataScope);
			}
			await next();
		}
	}
}

