using System.ComponentModel;
using System.Linq.Expressions;
using Findx.AspNetCore.Mvc;
using Findx.Common;
using Findx.Data;
using Findx.Expressions;
using Findx.Extensions;
using Findx.Module.EleAdminPlus.Shared.Models;
using Findx.Module.EleAdminPlus.Shared.ServiceDefaults;
using Findx.Module.EleAdminPlus.WebAppApi.Dtos.UserFile;
using Findx.Module.EleAdminPlus.WebAppApi.Vos.UserFile;
using Findx.NewId;
using Findx.Setting;
using Findx.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.Module.EleAdminPlus.WebAppApi.Controller;

/// <summary>
///     系统-用户文件
/// </summary>
[Area("system")]
[Route("api/[area]/user-file")]
[Authorize]
[ApiExplorerSettings(GroupName = "eleAdminPlus"), Tags("系统-用户文件"), Description("系统-用户文件")]
public class SysUserFileController : QueryControllerBase<SysUserFileInfo, UserFileSimplifyVo, UserFilePageQueryDto, long>
{
    private readonly IKeyGenerator<long> _keyGenerator;
    private readonly IWorkContext _workContext;
    private readonly string _folderHost;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="keyGenerator"></param>
    /// <param name="workContext"></param>
    /// <param name="applicationContext"></param>
    /// <param name="settingProvider"></param>
    public SysUserFileController(IKeyGenerator<long> keyGenerator, IWorkContext workContext, IApplicationContext applicationContext, ISettingProvider settingProvider)
    {
        _keyGenerator = keyGenerator;
        _workContext = workContext;
        _folderHost = settingProvider.GetValue<string>("Findx:Storage:Folder:Host")
                      ?? $"//{HostUtility.ResolveHostAddress(HostUtility.ResolveHostName())}:{applicationContext.Port}";
    }

    /// <summary>
    ///     当前登录用户id
    /// </summary>
    private long LoginUserId => _workContext.ContextUser?.UserId ?? 0;

    /// <summary>
    ///     构建查询条件（限定为当前登录用户的文件）
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    protected override Expression<Func<SysUserFileInfo, bool>> CreateWhereExpression(UserFilePageQueryDto req)
    {
        var baseExp = base.CreateWhereExpression(req);
        var userId = LoginUserId;
        var exp = PredicateBuilder.New<SysUserFileInfo>().And(x => x.UserId == userId);
        return baseExp == null ? exp.Build() : exp.And(baseExp).Build();
    }

    /// <summary>
    ///     构建排序（文件夹优先）
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    protected override IEnumerable<SortCondition<SysUserFileInfo>> CreateOrderExpression(UserFilePageQueryDto req)
    {
        var orderExp = SortConditionBuilder.New<SysUserFileInfo>();

        if (req.SortField.IsNotNullOrWhiteSpace())
            orderExp.Order(req.SortField, req.SortDirection);

        orderExp.OrderByDescending(x => x.IsDirectory);
        orderExp.OrderByDescending(x => x.Id);

        return orderExp.Build();
    }

    /// <summary>
    ///     分页查询
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<CommonResult<PageResult<List<UserFileSimplifyVo>>>> PageAsync(UserFilePageQueryDto req, CancellationToken cancellationToken = default)
    {
        var res = await base.PageAsync(req, cancellationToken);
        FillFileUrls(res.Data?.Rows);
        return res;
    }

    /// <summary>
    ///     列表查询
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<CommonResult<List<UserFileSimplifyVo>>> ListAsync(UserFilePageQueryDto req, CancellationToken cancellationToken = default)
    {
        var res = await base.ListAsync(req, cancellationToken);
        FillFileUrls(res.Data);
        return res;
    }

    /// <summary>
    ///     添加用户文件
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("add")]
    [Description("新增")]
    public async Task<CommonResult> AddAsync([FromBody] UserFileSaveDto req, CancellationToken cancellationToken = default)
    {
        Check.NotNull(req, nameof(req));

        var userId = LoginUserId;
        var parentId = req.ParentId ?? 0;
        var repo = GetRepository<SysUserFileInfo, long>();

        //  文件夹同名校验
        if (req.IsDirectory == 1)
        {
            var name = req.Name;
            if (await repo.ExistAsync(x => x.Name == name && x.ParentId == parentId && x.UserId == userId, cancellationToken))
                return CommonResult.Fail("user.file.exists", "文件夹名称已存在");
        }

        var model = new SysUserFileInfo
        {
            Id = _keyGenerator.Create(),
            UserId = userId,
            Name = req.Name,
            IsDirectory = req.IsDirectory,
            ParentId = parentId,
            Path = req.Path,
            Length = req.Length,
            ContentType = req.ContentType
        };

        model.CheckCreatedTime();
        model.CheckCreationAudited<SysUserFileInfo, long>(HttpContext.User);
        model.CheckTenant(HttpContext.User);

        var res = await repo.InsertAsync(model, cancellationToken);
        return res > 0 ? CommonResult.Success("添加成功") : CommonResult.Fail("db.add.error", "添加失败");
    }

    /// <summary>
    ///     修改用户文件
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("edit")]
    [Description("编辑")]
    public async Task<CommonResult> UpdateAsync([FromBody] UserFileEditDto req, CancellationToken cancellationToken = default)
    {
        Check.NotNull(req, nameof(req));

        var userId = LoginUserId;
        var repo = GetRepository<SysUserFileInfo, long>();

        var old = await repo.GetAsync(req.Id, cancellationToken);
        if (old == null || old.UserId != userId)
            return CommonResult.Fail("db.edit.error", "修改失败");

        var newName = req.Name.IsNullOrWhiteSpace() ? null : req.Name;
        var newParentId = req.ParentId;

        // 未提供任何可修改字段
        if (newName == null && newParentId == null)
            return CommonResult.Fail("db.edit.error", "修改失败");

        // 文件夹同名校验
        if (old.IsDirectory == 1)
        {
            var checkName = newName ?? old.Name;
            var checkParent = newParentId ?? old.ParentId;
            var oldId = old.Id;
            if (await repo.ExistAsync(x => x.Name == checkName && x.ParentId == checkParent && x.UserId == userId && x.Id != oldId, cancellationToken))
                return CommonResult.Fail("user.file.exists", "文件夹名称已存在");
        }

        var updater = _workContext.ContextUser?.Username ?? string.Empty;
        var now = DateTime.Now;
        var columns = new List<Expression<Func<SysUserFileInfo, bool>>>
        {
            x => x.LastUpdatedTime == now,
            x => x.LastUpdaterId == userId,
            x => x.LastUpdater == updater
        };
        if (newName != null) columns.Add(x => x.Name == newName);
        if (newParentId != null) columns.Add(x => x.ParentId == newParentId.Value);

        var res = await repo.UpdateColumnsAsync(columns, x => x.Id == req.Id && x.UserId == userId, cancellationToken);
        return res > 0 ? CommonResult.Success("修改成功") : CommonResult.Fail("db.edit.error", "修改失败");
    }

    /// <summary>
    ///     删除用户文件（支持批量）
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("delete")]
    [Description("删除")]
    public async Task<CommonResult> RemoveAsync([FromBody] List<long> req, CancellationToken cancellationToken = default)
    {
        Check.NotNull(req, nameof(req));

        if (req.Count == 0)
            return CommonResult.Fail("delete.not.count", "不存在删除数据");

        var userId = LoginUserId;
        var repo = GetRepository<SysUserFileInfo, long>();

        var total = await repo.DeleteAsync(x => req.Contains(x.Id) && x.UserId == userId, cancellationToken);
        return total > 0 ? CommonResult.Success("删除成功") : CommonResult.Fail("db.delete.error", "删除失败");
    }

    /// <summary>
    ///     填充文件访问地址、下载地址、缩略图地址
    /// </summary>
    /// <param name="records"></param>
    private void FillFileUrls(List<UserFileSimplifyVo> records)
    {
        if (records == null) return;

        foreach (var record in records)
        {
            if (record.Path.IsNullOrWhiteSpace()) continue;

            record.Url = Path.Combine(_folderHost, record.Path).NormalizePath();
            record.DownloadUrl = record.Url;

            var extension = Path.GetExtension(!record.Name.IsNullOrWhiteSpace() ? record.Name : record.Path);
            if (FileUtility.IsImage(extension))
                record.Thumbnail = record.Url;
        }
    }
}
