using Findx.Caching;
using Findx.Data;
using Findx.Extensions;
using Findx.Module.EleAdmin.Shared.Enum;
using Findx.Module.EleAdmin.Shared.Models;
using Findx.Module.EleAdmin.Shared.ServiceDefaults;
using Findx.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.Module.EleAdmin.Mvc.Filters;

/// <summary>
///     EleAdminPlus全局过滤器
/// </summary>
public class EleAdminGlobalAttribute: ActionFilterAttribute
{
    /// <summary>
    ///     过滤器执行
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var filters = context.Filters;

        var ipAddressLimit = filters.OfType<IpAddressLimiterAttribute>().Any();
        var dataScopeLimit = filters.OfType<DataScopeLimiterAttribute>().Any();

        var workContext = context.HttpContext.RequestServices.GetService<IWorkContext>();
        var currentUser = context.HttpContext.RequestServices.GetService<ICurrentUser>();
        
        if (currentUser is { IsAuthenticated: true } && (ipAddressLimit || dataScopeLimit))
        {
            var user = workContext.GetCurrentUser();

            // 用户角色信息
            var roles = await AllRoleListAsync(context);
            var userRoleIds = currentUser.FindClaims(ClaimTypes.RoleIds).SelectMany(m => m.Value.Split(',', StringSplitOptions.RemoveEmptyEntries)).Select(Guid.Parse);
            var userRoleList = roles.Where(x => userRoleIds.Contains(x.Id));
            
            //  启用数据范围限定
            if (dataScopeLimit)
            {
                // 默认数据范围
                workContext.SetDataScope(DataScope.Oneself);
                // 根据角色控制数据范围
                if (userRoleList.Any())
                {
                    // 设置权限最高数据范围
                    var maxRoleInfo = userRoleList.OrderBy(x => x.DataScope).First();
                    workContext.SetDataScope(maxRoleInfo.DataScope);
                    
                    // 自定义机构范围
                    if (workContext.DataScope == DataScope.Custom)
                    {
                        // 所有自定义范围集合
                        var ids = userRoleList.Where(x => x.DataScope == DataScope.Custom).SelectMany(x => x.OrgJson?.ToObject<List<Guid>>());
                        workContext.SetOrgIds(ids);
                    }

                    // 本部门及所有下属部门
                    if (workContext.DataScope == DataScope.Subs && user.OrgId.HasValue)
                    {
                        var orgList = await AllOrgListAsync(context);
                        var allSubsIds = GetTargetDepartmentAndSubOrgList(orgList, user.OrgId.Value).Select(x => x.Id);
                        workContext.SetOrgIds(allSubsIds);
                    }
                }
                // 默认本机构
                if (workContext.DataScope is DataScope.Oneself or DataScope.Department)
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    workContext.SetOrgIds([user.OrgId.Value]);
                }
            }
        }
        await next();
    }

    /// <summary>
    ///     所有机构清单
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<List<SysOrgInfo>> AllOrgListAsync(ActionExecutingContext context)
    {
        var cacheFactory = context.HttpContext.RequestServices.GetService<ICacheFactory>();
        var cache = cacheFactory.Create(CacheType.DefaultMemory);
        var currentUser = context.HttpContext.RequestServices.GetService<ICurrentUser>();
        var cacheKey = $"EleAdmin:Org:{currentUser.TenantId}";
        
        var orgList = await cache.GetAsync<List<SysOrgInfo>>(cacheKey, context.HttpContext.RequestAborted);
        if (orgList != null && orgList.Any())
            return orgList;
        
        var orgRepo = context.HttpContext.RequestServices.GetService<IRepository<SysOrgInfo, Guid>>();
        orgList = await orgRepo.SelectAsync(selectExpression: x => new SysOrgInfo { Id = x.Id, Name = x.Name, ParentId = x.ParentId });
        await cache.AddAsync(cacheKey, orgList, context.HttpContext.RequestAborted);

        return orgList;
    }
    
    /// <summary>
    ///     所有角色清单
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<List<SysRoleInfo>> AllRoleListAsync(ActionExecutingContext context)
    {
        var cacheFactory = context.HttpContext.RequestServices.GetService<ICacheFactory>();
        var cache = cacheFactory.Create(CacheType.DefaultMemory);
        var currentUser = context.HttpContext.RequestServices.GetService<ICurrentUser>();
        var cacheKey = $"EleAdmin:Role:{currentUser.TenantId}";
        
        var roleList = await cache.GetAsync<List<SysRoleInfo>>(cacheKey, context.HttpContext.RequestAborted);
        if (roleList != null && roleList.Any())
            return roleList;
        
        var roleRepo = context.HttpContext.RequestServices.GetService<IRepository<SysRoleInfo, Guid>>();
        roleList = await roleRepo.SelectAsync(selectExpression: x => new SysRoleInfo { Id = x.Id, DataScope = x.DataScope, OrgJson = x.OrgJson });
        await cache.AddAsync(cacheKey, roleList, context.HttpContext.RequestAborted);

        return roleList;
    }

    /// <summary>
    ///     本机构及所有子集机构集合
    /// </summary>
    /// <param name="organizations"></param>
    /// <param name="targetDepartmentId"></param>
    /// <returns></returns>
    static List<SysOrgInfo> GetTargetDepartmentAndSubOrgList(List<SysOrgInfo> organizations, Guid targetDepartmentId)
    {
        var result = new List<SysOrgInfo>();
        var queue = new Queue<SysOrgInfo>();

        foreach (var org in organizations)
        {
            if (org.Id == targetDepartmentId)
            {
                result.Add(org);
                queue.Enqueue(org);
                break;
            }
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var org in organizations)
            {
                if (org.ParentId == current.Id)
                {
                    result.Add(org);
                    queue.Enqueue(org);
                }
            }
        }

        return result;
    }
}