using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Data;
using Findx.Extensions;
using Findx.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace Findx.AspNetCore.Mvc;

/// <summary>
///     增删改查通用控制器基类
/// </summary>
/// <typeparam name="TModel">实体</typeparam>
/// <typeparam name="TRequest">新增/编辑Dto</typeparam>
/// <typeparam name="TQueryParameter">查询Dto</typeparam>
/// <typeparam name="TKey">实体主键</typeparam>
/// <typeparam name="TUserKey">用户主键</typeparam>
public abstract class CrudControllerBase<TModel, TRequest, TQueryParameter, TKey, TUserKey> : CrudControllerBase<TModel,
    TModel, TModel, TRequest, TRequest, TQueryParameter, TKey, TUserKey>
    where TModel : EntityBase<TKey>, IResponse, new()
    where TRequest : IRequest<TKey>, new()
    where TQueryParameter : class, IPager, new()
    where TKey : IEquatable<TKey>
    where TUserKey : struct;

/// <summary>
///     增删改查通用控制器基类
/// </summary>
/// <typeparam name="TModel">实体</typeparam>
/// <typeparam name="TVo">返回Dto</typeparam>
/// <typeparam name="TRequest">新增/编辑Dto</typeparam>
/// <typeparam name="TQueryParameter">查询Dto</typeparam>
/// <typeparam name="TKey">实体主键</typeparam>
/// <typeparam name="TUserKey">用户主键</typeparam>
public abstract class CrudControllerBase<TModel, TVo, TRequest, TQueryParameter, TKey, TUserKey> : CrudControllerBase<
    TModel, TVo, TVo, TRequest, TRequest, TQueryParameter, TKey, TUserKey>
    where TModel : EntityBase<TKey>, new()
    where TVo : IResponse, new()
    where TRequest : IRequest<TKey>, new()
    where TQueryParameter : class, IPager, new()
    where TKey : IEquatable<TKey>
    where TUserKey : struct;

/// <summary>
///     增删改查通用控制器基类
/// </summary>
/// <typeparam name="TModel">实体</typeparam>
/// <typeparam name="TVo">返回Dto</typeparam>
/// <typeparam name="TCreateRequest">新增Dto</typeparam>
/// <typeparam name="TUpdateRequest">编辑Dto</typeparam>
/// <typeparam name="TQueryParameter">查询Dto</typeparam>
/// <typeparam name="TKey">实体主键</typeparam>
/// <typeparam name="TUserKey">用户主键</typeparam>
public abstract class
    CrudControllerBase<TModel, TVo, TCreateRequest, TUpdateRequest, TQueryParameter, TKey, TUserKey> :
        CrudControllerBase<TModel, TVo, TVo, TCreateRequest, TUpdateRequest, TQueryParameter, TKey, TUserKey>
    where TModel : EntityBase<TKey>, new()
    where TVo : IResponse, new()
    where TCreateRequest : IRequest, new()
    where TUpdateRequest : IRequest<TKey>, new()
    where TQueryParameter : class, IPager, new()
    where TKey : IEquatable<TKey>
    where TUserKey : struct;

/// <summary>
///     增删改查通用控制器基类
/// </summary>
/// <typeparam name="TModel">实体</typeparam>
/// <typeparam name="TListVo">返回列表Dto</typeparam>
/// <typeparam name="TDetailVo">返回详情Dto</typeparam>
/// <typeparam name="TCreateRequest">新增Dto</typeparam>
/// <typeparam name="TUpdateRequest">编辑Dto</typeparam>
/// <typeparam name="TQueryParameter">查询Dto</typeparam>
/// <typeparam name="TKey">实体主键</typeparam>
/// <typeparam name="TUserKey">用户字段</typeparam>
public abstract class CrudControllerBase<TModel, TListVo, TDetailVo, TCreateRequest, TUpdateRequest, TQueryParameter,
        TKey, TUserKey>
    : QueryControllerBase<TModel, TListVo, TDetailVo, TQueryParameter, TKey>
    where TModel : EntityBase<TKey>, new()
    where TListVo : IResponse, new()
    where TDetailVo : IResponse, new()
    where TCreateRequest : IRequest, new()
    where TUpdateRequest : IRequest<TKey>, new()
    where TQueryParameter : class, IPager, new()
    where TKey : IEquatable<TKey>
    where TUserKey : struct
{
    /// <summary>
    ///     创建参数转换为实体
    /// </summary>
    /// <param name="req">创建参数</param>
    protected virtual TModel ToModelFromCreateRequest(TCreateRequest req)
    {
        return req.MapTo<TModel>();
    }

    /// <summary>
    ///     修改参数转换为实体
    /// </summary>
    /// <param name="req">修改参数</param>
    /// <param name="model"></param>
    protected virtual TModel ToModelFromUpdateRequest(TUpdateRequest req, TModel model)
    {
        return req.MapTo(model);
    }

    /// <summary>
    ///     创建前操作
    /// </summary>
    /// <param name="model">实体</param>
    /// <param name="req">入参</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task AddBeforeAsync(TModel model, TCreateRequest req, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     创建后操作
    /// </summary>
    /// <param name="model">实体</param>
    /// <param name="req">入参</param>
    /// <param name="affectedCount">受影响计数</param>
    /// <param name="cancellationToken"></param>
    protected virtual Task AddAfterAsync(TModel model, TCreateRequest req, int affectedCount, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     修改前操作
    /// </summary>
    /// <param name="model">修改参数</param>
    /// <param name="req">入参</param>
    /// <param name="cancellationToken"></param>
    protected virtual Task EditBeforeAsync(TModel model, TUpdateRequest req, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     修改后操作
    /// </summary>
    /// <param name="model">修改参数</param>
    /// <param name="req">入参</param>
    /// <param name="affectedCount">受影响计数</param>
    /// <param name="cancellationToken"></param>
    protected virtual Task EditAfterAsync(TModel model, TUpdateRequest req, int affectedCount, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     删除前前操作
    /// </summary>
    /// <param name="req">id集合</param>
    /// <param name="cancellationToken"></param>
    protected virtual Task DeleteBeforeAsync(List<TKey> req, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     删除后操作
    /// </summary>
    /// <param name="req">id集合</param>
    /// <param name="affectedCount">受影响计数</param>
    /// <param name="cancellationToken"></param>
    protected virtual Task DeleteAfterAsync(List<TKey> req, int affectedCount, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     工作单元
    /// </summary>
    protected IUnitOfWork UnitOfWork { get; set; }
    
    /// <summary>
    ///     添加数据
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("add")]
    [Description("新增")]
    public virtual async Task<CommonResult> AddAsync([FromBody] TCreateRequest req, CancellationToken cancellationToken = default)
    {
        Check.NotNull(req, nameof(req));

        var repo = GetRepository<TModel, TKey>();
        
        var unitOfManager = GetService<IUnitOfWorkManager>();
        UnitOfWork = await unitOfManager.GetEntityUnitOfWorkAsync<TModel>(false, cancellationToken);
        repo.UnitOfWork = UnitOfWork;
        
        var model = ToModelFromCreateRequest(req);

        Check.NotNull(model, nameof(model));

        // 判断设置创建时间
        model.CheckCreatedTime();
        // 判断设置创建人
        model.CheckCreationAudited<TModel, TUserKey>(HttpContext.User);
        // 判断设置租户值
        model.CheckTenant(HttpContext.User);
        // 判断设置部门机构
        model.CheckOrg<TModel, TUserKey>(HttpContext.User);
        // 判断设置拥有者
        model.CheckOwner<TModel, TUserKey>(HttpContext.User);
        // 判断设置ID值
        model.SetEmptyKey(); 

        await AddBeforeAsync(model, req, cancellationToken);
        var res = await repo.InsertAsync(model, cancellationToken);
        await AddAfterAsync(model, req, res, cancellationToken);

        return res > 0 ? CommonResult.Success() : CommonResult.Fail("db.add.error", "数据创建失败");
    }

    /// <summary>
    ///     修改数据
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("edit")]
    [Description("编辑")]
    public virtual async Task<CommonResult> EditAsync([FromBody] TUpdateRequest req, CancellationToken cancellationToken = default)
    {
        Check.NotNull(req, nameof(req));

        var repo = GetRepository<TModel, TKey>();
        
        var unitOfManager = GetService<IUnitOfWorkManager>();
        UnitOfWork = await unitOfManager.GetEntityUnitOfWorkAsync<TModel>(false, cancellationToken);
        repo.UnitOfWork = UnitOfWork;

        var model = await repo.GetAsync(req.Id, cancellationToken);
        if (model == null) 
            return CommonResult.Fail("not.exist", "未能查到相关信息");
        
        model = ToModelFromUpdateRequest(req, model);
        // 判断设置修改时间
        model.CheckUpdateTime();
        // 判断设置修改人
        model.CheckUpdateAudited<TModel, TUserKey>(HttpContext.User);
        
        await EditBeforeAsync(model, req, cancellationToken);
        var res = await repo.SaveAsync(model, cancellationToken: cancellationToken);
        await EditAfterAsync(model, req, res, cancellationToken);

        return res > 0 ? CommonResult.Success() : CommonResult.Fail("db.edit.error", "数据更新失败");
    }

    /// <summary>
    ///     删除数据
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("delete")]
    [Description("删除")]
    public virtual async Task<CommonResult> DeleteAsync([FromBody] [MinLength(1)] List<TKey> req, CancellationToken cancellationToken = default)
    {
        Check.NotNull(req, nameof(req));
        
        if (req.Count == 0) return CommonResult.Fail("delete.not.count", "不存在删除数据");

        var repo = GetRepository<TModel, TKey>();

        var unitOfManager = GetService<IUnitOfWorkManager>();
        UnitOfWork = await unitOfManager.GetEntityUnitOfWorkAsync<TModel>(false, cancellationToken);
        repo.UnitOfWork = UnitOfWork;
        
        await DeleteBeforeAsync(req, cancellationToken);
        var total = await repo.DeleteAsync(x => req.Contains(x.Id), cancellationToken);
        await DeleteAfterAsync(req, total, cancellationToken);

        return CommonResult.Success($"共删除{total}条数据,失败{req.Count - total}条");
    }
}