using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Exceptions;
using Findx.Module.EleAdmin.DTO;
using Findx.Module.EleAdmin.Models;
using Findx.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Areas.System.Controller;

/// <summary>
/// 系统-应用服务
/// </summary>
[Area("system")]
[Route("api/[area]/app")]
[Authorize]
[Description("系统-应用")]
[ApiExplorerSettings(GroupName = "eleAdmin")]
public class SysAppController : CrudControllerBase<SysAppInfo, SetAppRequest, QueryAppRequest, Guid, Guid>
{
    /// <summary>
    /// 新增前校验
    /// </summary>
    /// <param name="model"></param>
    /// <param name="request"></param>
    /// <exception cref="FindxException"></exception>
    protected override async Task AddBeforeAsync(SysAppInfo model, SetAppRequest request)
    {
        var repo = GetRepository<SysAppInfo>();

        var isExist = await repo.ExistAsync(x => x.Name == model.Name || x.Code == model.Code);
        if (isExist)
            throw new FindxException("500", "已存在同名或同编码应用");
    }

    /// <summary>
    /// 编辑娇艳
    /// </summary>
    /// <param name="model"></param>
    /// <param name="request"></param>
    /// <exception cref="FindxException"></exception>
    protected override async Task EditBeforeAsync(SysAppInfo model, SetAppRequest request)
    {
        var repo = GetRepository<SysAppInfo>();
        var isExist = await repo.ExistAsync(x => (x.Name == model.Name || x.Code == model.Code) && x.Id != model.Id);
        if (isExist)
            throw new FindxException("500", "已存在同名或同编码应用");
        var old = await repo.FirstAsync(x => x.Id == request.Id);
        if (old.Code != request.Code)
        {
            var repoMenu = GetRepository<SysMenuInfo>();
            await repoMenu.UpdateColumnsAsync(x => new SysMenuInfo { ApplicationCode = request.Code, ApplicationName = request.Name },
                x => x.ApplicationCode == old.Code);
            
            var repoRole = GetRepository<SysRoleInfo>();
            await repoRole.UpdateColumnsAsync(x => new SysRoleInfo { ApplicationCode = request.Code, ApplicationName = request.Name },
                x => x.ApplicationCode == old.Code);
        }
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async Task<CommonResult> DeleteAsync([FromBody] List<Guid> request)
    {
        Check.NotNull(request, nameof(request));
        if (request.Count == 0)
            return CommonResult.Fail("delete.not.count", "不存在删除数据");

        var repo = GetRepository<SysAppInfo>();
        var repoMenu = GetRepository<SysMenuInfo>();
        var currentUser = GetService<ICurrentUser>();

        Check.NotNull(repo, nameof(repo));
        Check.NotNull(currentUser, nameof(currentUser));

        var total = 0;
        foreach (var item in request)
        {
            var info = await repo.GetAsync(item);

            if (await repoMenu.ExistAsync(u => u.ApplicationCode == info.Code))
                return CommonResult.Fail("delete.not.count", "该应用下有菜单禁止删除");

            if (await repo.DeleteAsync(key: item) > 0)
                total++;
        }

        return CommonResult.Success($"共删除{total}条数据,失败{request.Count - total}条");
    }
}