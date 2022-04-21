using Findx.Data;
using Findx.Extensions;
using Findx.Locks;
using Findx.Security;
using Findx.Utils;
using Findx.AspNetCore.Extensions;
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

            ILock rlock = GetLock(context);
            var key = GetLockKey(context);
            var expir = Time.ToTimeSpan(Interval);

            var getLock = await rlock.AcquireAsync(key, timeUntilExpires: expir, renew: true);
            if (getLock.IsLocked())
            {
                try
                {
                    await next();
                }
                finally
                {
                    await getLock.ReleaseAsync();
                }
            }
            else
            {
                context.Result = new JsonResult(CommonResult.Fail("4019", "重复提交,请稍后再试"));
            }
        }


        /// <summary>
        /// 获取锁服务
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private ILock GetLock(ActionExecutingContext context)
        {
            var provider = context.HttpContext.RequestServices.GetService<ILockProvider>();
            if (IsDistributed)
                return provider.Get(Locks.LockType.Distributed);
            return provider.Get(Locks.LockType.Local);
        }

        /// <summary>
        /// 获取锁资源key
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetLockKey(ActionExecutingContext context)
        {
            var currentUser = context.HttpContext.RequestServices.GetCurrentUser();

            var userId = string.Empty;

            if (Type == LockType.User && currentUser.Identity.IsAuthenticated)
                userId = $"{currentUser.Identity.GetUserId()}_";

            if (Type == LockType.IP)
                userId = $"{context.HttpContext.GetClientIp()}_";

            return Key.IsNullOrWhiteSpace() ? $"ADR:{userId}{context.HttpContext.Request.Path}" : $"ADR:{userId}{Key}";
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
        /// 用户级别锁,同一IP只能同时发起一个请求
        /// </summary>
        IP = 1,

        /// <summary>
        /// 全局锁，该操作同时只有一个用户请求被执行
        /// </summary>
        Global = 2
    }
}
