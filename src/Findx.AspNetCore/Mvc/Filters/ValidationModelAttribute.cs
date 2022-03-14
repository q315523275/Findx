using Findx.Data;
using Findx.Validations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace Findx.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// 模型验证过滤器
    /// </summary>
    public class ValidationModelAttribute : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                                    .Where(e => e.Value.Errors.Count > 0)
                                    .Select(e => new ErrorMember() { ErrorMemberName = e.Key, ErrorMessage = e.Value.Errors.First().ErrorMessage });

                context.Result = new JsonResult(CommonResult.Fail("4001", "参数校验不通过", errors));
            }
        }

    }
}
