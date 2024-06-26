﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Data;
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
    where TRequest : IRequest, new()
    where TQueryParameter : PageBase, new()
    where TKey : IEquatable<TKey>
    where TUserKey : struct;

/// <summary>
///     增删改查通用控制器基类
/// </summary>
/// <typeparam name="TModel">实体</typeparam>
/// <typeparam name="TDto">返回Dto</typeparam>
/// <typeparam name="TRequest">新增/编辑Dto</typeparam>
/// <typeparam name="TQueryParameter">查询Dto</typeparam>
/// <typeparam name="TKey">实体主键</typeparam>
/// <typeparam name="TUserKey">用户主键</typeparam>
public abstract class CrudControllerBase<TModel, TDto, TRequest, TQueryParameter, TKey, TUserKey> : CrudControllerBase<
    TModel, TDto, TDto, TRequest, TRequest, TQueryParameter, TKey, TUserKey>
    where TModel : EntityBase<TKey>, new()
    where TDto : IResponse, new()
    where TRequest : IRequest, new()
    where TQueryParameter : PageBase, new()
    where TKey : IEquatable<TKey>
    where TUserKey : struct;

/// <summary>
///     增删改查通用控制器基类
/// </summary>
/// <typeparam name="TModel">实体</typeparam>
/// <typeparam name="TDto">返回Dto</typeparam>
/// <typeparam name="TCreateRequest">新增Dto</typeparam>
/// <typeparam name="TUpdateRequest">编辑Dto</typeparam>
/// <typeparam name="TQueryParameter">查询Dto</typeparam>
/// <typeparam name="TKey">实体主键</typeparam>
/// <typeparam name="TUserKey">用户主键</typeparam>
public abstract class
    CrudControllerBase<TModel, TDto, TCreateRequest, TUpdateRequest, TQueryParameter, TKey, TUserKey> :
        CrudControllerBase<TModel, TDto, TDto, TCreateRequest, TUpdateRequest, TQueryParameter, TKey, TUserKey>
    where TModel : EntityBase<TKey>, new()
    where TDto : IResponse, new()
    where TCreateRequest : IRequest, new()
    where TUpdateRequest : IRequest, new()
    where TQueryParameter : PageBase, new()
    where TKey : IEquatable<TKey>
    where TUserKey : struct;

/// <summary>
///     增删改查通用控制器基类
/// </summary>
/// <typeparam name="TModel">实体</typeparam>
/// <typeparam name="TListDto">返回列表Dto</typeparam>
/// <typeparam name="TDetailDto">返回详情Dto</typeparam>
/// <typeparam name="TCreateRequest">新增Dto</typeparam>
/// <typeparam name="TUpdateRequest">编辑Dto</typeparam>
/// <typeparam name="TQueryParameter">查询Dto</typeparam>
/// <typeparam name="TKey">实体主键</typeparam>
/// <typeparam name="TUserKey">用户字段</typeparam>
public abstract class CrudControllerBase<TModel, TListDto, TDetailDto, TCreateRequest, TUpdateRequest, TQueryParameter,
        TKey, TUserKey>
    : QueryControllerBase<TModel, TListDto, TDetailDto, TQueryParameter, TKey>
    where TModel : EntityBase<TKey>, new()
    where TListDto : IResponse, new()
    where TDetailDto : IResponse, new()
    where TCreateRequest : IRequest, new()
    where TUpdateRequest : IRequest, new()
    where TQueryParameter : PageBase, new()
    where TKey : IEquatable<TKey>
    where TUserKey : struct
{
    /// <summary>
    ///     创建参数转换为实体
    /// </summary>
    /// <param name="request">创建参数</param>
    protected virtual TModel ToModelFromCreateRequest(TCreateRequest request)
    {
        return request.MapTo<TModel>();
    }

    /// <summary>
    ///     修改参数转换为实体
    /// </summary>
    /// <param name="request">修改参数</param>
    protected virtual TModel ToModelFromUpdateRequest(TUpdateRequest request)
    {
        return request.MapTo<TModel>();
    }

    /// <summary>
    ///     创建前操作
    /// </summary>
    /// <param name="model">实体</param>
    /// <param name="request">入参</param>
    /// <returns></returns>
    protected virtual Task AddBeforeAsync(TModel model, TCreateRequest request)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     创建后操作
    /// </summary>
    /// <param name="model">实体</param>
    /// <param name="request">入参</param>
    /// <param name="result">添加结果</param>
    protected virtual Task AddAfterAsync(TModel model, TCreateRequest request, int result)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     修改前操作
    /// </summary>
    /// <param name="model">修改参数</param>
    /// <param name="request">入参</param>
    protected virtual Task EditBeforeAsync(TModel model, TUpdateRequest request)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     修改后操作
    /// </summary>
    /// <param name="model">修改参数</param>
    /// <param name="request">入参</param>
    /// <param name="result">处理结果</param>
    protected virtual Task EditAfterAsync(TModel model, TUpdateRequest request, int result)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     删除前前操作
    /// </summary>
    /// <param name="req">id集合</param>
    protected virtual Task DeleteBeforeAsync(List<TKey> req)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     删除后操作
    /// </summary>
    /// <param name="req">id集合</param>
    /// <param name="total">删除成功条数</param>
    protected virtual Task DeleteAfterAsync(List<TKey> req, int total)
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
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("add")]
    [Description("新增")]
    public virtual async Task<CommonResult> AddAsync([FromBody] TCreateRequest request, CancellationToken cancellationToken = default)
    {
        Check.NotNull(request, nameof(request));

        Repo = GetRepository<TModel, TKey>();
        var principal = GetService<IPrincipal>();
        
        Check.NotNull(Repo, nameof(Repo));
        
        var unitOfManager = GetService<IUnitOfWorkManager>();
        UnitOfWork = await unitOfManager.GetConnUnitOfWorkAsync(false, false, Repo.GetDataSource(), cancellationToken);
        Repo.UnitOfWork = UnitOfWork;
        
        var model = ToModelFromCreateRequest(request);

        Check.NotNull(model, nameof(model));

        model.CheckCreatedTime(); // 判断设置创建时间
        model.CheckCreationAudited<TModel, TUserKey>(principal); // 判断设置创建人
        model.CheckTenant(principal); // 判断设置租户值
        model.SetEmptyKey(); // 判断设置ID值

        await AddBeforeAsync(model, request);
        var res = await Repo.InsertAsync(model, cancellationToken);
        await AddAfterAsync(model, request, res);

        return res > 0 ? CommonResult.Success() : CommonResult.Fail("db.add.error", "数据创建失败");
    }

    /// <summary>
    ///     修改数据
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("edit")]
    [Description("编辑")]
    public virtual async Task<CommonResult> EditAsync([FromBody] TUpdateRequest request, CancellationToken cancellationToken = default)
    {
        Check.NotNull(request, nameof(request));

        Repo = GetRepository<TModel, TKey>();
        var principal = GetService<IPrincipal>();

        Check.NotNull(Repo, nameof(Repo));
        
        var unitOfManager = GetService<IUnitOfWorkManager>();
        UnitOfWork = await unitOfManager.GetConnUnitOfWorkAsync(false, false, Repo.GetDataSource(), cancellationToken);
        Repo.UnitOfWork = UnitOfWork;

        var model = ToModelFromUpdateRequest(request);

        Check.NotNull(model, nameof(model));

        model.CheckUpdateTime(); // 判断设置修改时间
        model.CheckUpdateAudited<TModel, TUserKey>(principal); // 判断设置修改人

        await EditBeforeAsync(model, request);
        var res = await Repo.UpdateAsync(model, ignoreNullColumns: true, cancellationToken: cancellationToken);
        await EditAfterAsync(model, request, res);

        return res > 0 ? CommonResult.Success() : CommonResult.Fail("db.edit.error", "数据更新失败");
    }

    /// <summary>
    ///     删除数据
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("delete")]
    [Description("删除")]
    public virtual async Task<CommonResult> DeleteAsync([FromBody] [MinLength(1)] List<TKey> request, CancellationToken cancellationToken = default)
    {
        Check.NotNull(request, nameof(request));
        if (request.Count == 0)
            return CommonResult.Fail("delete.not.count", "不存在删除数据");

        Repo = GetRepository<TModel, TKey>();

        Check.NotNull(Repo, nameof(Repo));

        var unitOfManager = GetService<IUnitOfWorkManager>();
        var dataSource = Repo.GetDataSource();
        UnitOfWork = await unitOfManager.GetConnUnitOfWorkAsync(false, false, dataSource, cancellationToken);
        Repo.UnitOfWork = UnitOfWork;
        
        await DeleteBeforeAsync(request);
        var total = await Repo.DeleteAsync(x => request.Contains(x.Id), cancellationToken);
        await DeleteAfterAsync(request, total);

        return CommonResult.Success($"共删除{total}条数据,失败{request.Count - total}条");
    }
}