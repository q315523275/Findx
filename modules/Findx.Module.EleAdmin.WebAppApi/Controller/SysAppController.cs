using System.ComponentModel;
using Findx.AspNetCore.Mvc;
using Findx.Common;
using Findx.Data;
using Findx.Exceptions;
using Findx.Module.EleAdmin.Dtos;
using Findx.Module.EleAdmin.Dtos.App;
using Findx.Module.EleAdmin.Shared.Models;
using Findx.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdmin.Controller;

/// <summary>
///     系统-应用服务
/// </summary>
[Area("system")]
[Route("api/[area]/app")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdmin"), Tags("系统-应用"), Description("系统-应用")]
public class SysAppController : CrudControllerBase<SysAppInfo, AppSaveDto, AppQueryDto, Guid, Guid>
{
    /// <summary>
    ///     新增前校验
    /// </summary>
    /// <param name="model"></param>
    /// <param name="saveDto"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="FindxException"></exception>
    protected override async Task AddBeforeAsync(SysAppInfo model, AppSaveDto saveDto, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysAppInfo>();
        var isExist = await repo.ExistAsync(x => x.Name == model.Name || x.Code == model.Code, cancellationToken);
        if (isExist) throw new FindxException("500", "已存在同名或同编码应用");
    }

    /// <summary>
    ///     编辑前校验
    /// </summary>
    /// <param name="model"></param>
    /// <param name="saveDto"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="FindxException"></exception>
    protected override async Task EditBeforeAsync(SysAppInfo model, AppSaveDto saveDto, CancellationToken cancellationToken = default)
    {
        var repo = GetRepository<SysAppInfo>();
        var isExist = await repo.ExistAsync(x => (x.Name == model.Name || x.Code == model.Code) && x.Id != model.Id, cancellationToken);
        if (isExist) throw new FindxException("500", "已存在同名或同编码应用");
        var old = await repo.FirstAsync(x => x.Id == saveDto.Id, cancellationToken);
        if (old.Code != saveDto.Code)
        {
            var repoMenu = GetRepository<SysMenuInfo>();
            await repoMenu.UpdateColumnsAsync(x => new SysMenuInfo { ApplicationCode = saveDto.Code, ApplicationName = saveDto.Name }, x => x.ApplicationCode == old.Code, cancellationToken);

            var repoRole = GetRepository<SysRoleInfo>();
            await repoRole.UpdateColumnsAsync(x => new SysRoleInfo { ApplicationCode = saveDto.Code, ApplicationName = saveDto.Name }, x => x.ApplicationCode == old.Code, cancellationToken);
        }
    }

    /// <summary>
    ///     删除记录
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<CommonResult> DeleteAsync([FromBody] List<Guid> request, CancellationToken cancellationToken = default)
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
            var info = await repo.GetAsync(item, cancellationToken);

            if (await repoMenu.ExistAsync(u => u.ApplicationCode == info.Code, cancellationToken))
                return CommonResult.Fail("delete.not.count", "该应用下有菜单禁止删除");

            if (await repo.DeleteAsync(item, cancellationToken) > 0)
                total++;
        }

        return CommonResult.Success($"共删除{total}条数据,失败{request.Count - total}条");
    }
}