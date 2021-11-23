using Findx.Data;
using Findx.Extensions;
using Findx.Locks;
using Findx.Security;
using Findx.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Findx.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// 防止重复提交过滤器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AntiDuplicateRequestAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 业务标识
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 是否分布式
        /// </summary>
        public bool IsDistributed { set; get; }
        /// <summary>
        /// 再次提交时间间隔
        /// </summary>
        public string Interval { get; set; } = "30s";
        /// <summary>
        /// 锁类型
        /// </summary>
        public LockType Type { get; set; } = LockType.User;
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Check.NotNull(context, nameof(context));

            ILock localLock = GetLock(context);
            var key = GetLockKey(context);
            var val = GetLockValue(context);
            var expir = Time.ToTimeSpan(Interval);
            if (await localLock.LockTakeAsync(key, val, expir))
            {
                try
                {
                    await next();
                }
                finally
                {
                    await localLock.LockReleaseAsync(key, val);
                }
            }
            else
            {
                context.Result = new JsonResult(CommonResult.Fail("4019", "重复提交,请稍后再试"));
            }
        }
        private ILock GetLock(ActionExecutingContext context)
        {
            if (IsDistributed)
                return context.HttpContext.RequestServices.GetService<IDistributedLock>();
            return context.HttpContext.RequestServices.GetService<ILock>();
        }
        private string GetLockKey(ActionExecutingContext context)
        {
            var currentUser = context.HttpContext.RequestServices.GetCurrentUser();
            var userId = string.Empty;
            if (Type == LockType.User && currentUser.Identity.IsAuthenticated)
                userId = $"{currentUser.Identity.GetUserId()}_";
            return Key.IsNullOrWhiteSpace() ? $"ADR:{userId}{context.HttpContext.Request.Path}" : $"ADR:{userId}{Key}";
        }
        private string GetLockValue(ActionExecutingContext context)
        {
            var currentUser = context.HttpContext.RequestServices.GetCurrentUser();
            var value = string.Empty;
            if (Type == LockType.User && currentUser.Identity.IsAuthenticated)
                value = $"{currentUser.Identity.GetUserId()}";
            return value.IsNullOrWhiteSpace() ? "findx_global_lock" : value;
        }
    }
    /// <summary>
    /// 锁类型
    /// </summary>
    public enum LockType
    {
        /// <summary>
        /// 用户级别锁,同一用户只能同时发起一个请求
        /// </summary>
        User = 0,

        /// <summary>
        /// 全局锁，该操作同时只有一个用户请求被执行
        /// </summary>
        Global = 1
    }
}
