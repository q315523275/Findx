using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Data;
using Findx.Linq;
using Findx.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace Findx.AspNetCore.Mvc;

/// <summary>
///     通用查询控制器基类
/// </summary>
/// <typeparam name="TModel">实体</typeparam>
/// <typeparam name="TDto">返回Dto</typeparam>
/// <typeparam name="TQueryParameter">查询Dto</typeparam>
/// <typeparam name="TKey">实体主键</typeparam>
public abstract class QueryControllerBase<TModel, TDto, TQueryParameter, TKey> : QueryControllerBase<TModel, TDto, TDto,
    TQueryParameter, TKey>
    where TModel : EntityBase<TKey>, new()
    where TDto : IResponse, new()
    where TQueryParameter : PageBase, new()
    where TKey : IEquatable<TKey>
{
}

/// <summary>
///     通用查询控制器基类
/// </summary>
/// <typeparam name="TModel">实体</typeparam>
/// <typeparam name="TListDto">返回列表Dto</typeparam>
/// <typeparam name="TDetailDto">返回详情Dto</typeparam>
/// <typeparam name="TQueryParameter">查询Dto</typeparam>
/// <typeparam name="TKey">实体主键</typeparam>
public abstract class QueryControllerBase<TModel, TListDto, TDetailDto, TQueryParameter, TKey> : ApiControllerBase
    where TModel : EntityBase<TKey>, new()
    where TListDto : IResponse, new()
    where TDetailDto : IResponse, new()
    where TQueryParameter : PageBase, new()
    where TKey : IEquatable<TKey>
{
    /// <summary>
    ///     构建分页查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected virtual Expression<Func<TModel, bool>> CreatePageWhereExpression(TQueryParameter request)
    {
        return null;
    }

    /// <summary>
    ///     构建分页查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected virtual IEnumerable<OrderByParameter<TModel>> CreatePageOrderExpression(TQueryParameter request)
    {
        var orderExp = DataSortBuilder.New<TModel>();
        
        if (typeof(TModel).IsAssignableTo(typeof(ISort)))
            orderExp.OrderBy(it => (it as ISort).Sort);
        
        orderExp.OrderByDescending(it => it.Id);
        
        return orderExp.Build();
    }

    /// <summary>
    ///     查询数据
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("page")]
    [Description("分页")]
    public virtual async Task<CommonResult<PageResult<List<TListDto>>>> PageAsync([FromQuery] TQueryParameter request, CancellationToken cancellationToken = default)
    {
        Check.NotNull(request, nameof(request));

        var repo = GetRepository<TModel, TKey>();

        Check.NotNull(repo, nameof(repo));

        var whereExpression = CreatePageWhereExpression(request);
        var orderByExpression = CreatePageOrderExpression(request);

        var result = await repo.PagedAsync<TListDto>(request.PageNo, request.PageSize, whereExpression, orderParameters: orderByExpression, cancellationToken: cancellationToken);

        return CommonResult.Success(result);
    }

    /// <summary>
    ///     查询列表数据
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("list")]
    [Description("列表")]
    public virtual async Task<CommonResult<List<TListDto>>> ListAsync([FromQuery] TQueryParameter request, CancellationToken cancellationToken = default)
    {
        Check.NotNull(request, nameof(request));
        // 默认条数提升到99条
        if (request.PageSize == 20) 
            request.PageSize = 99;
        var repo = GetRepository<TModel, TKey>();
        Check.NotNull(repo, nameof(repo));

        var whereExpression = CreatePageWhereExpression(request);
        var orderByExpression = CreatePageOrderExpression(request);

        var list = await repo.TopAsync<TListDto>(request.PageSize, whereExpression, orderParameters: orderByExpression, cancellationToken: cancellationToken);

        return CommonResult.Success(list);
    }

    /// <summary>
    ///     查询单条数据
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("detail")]
    [Description("详情")]
    public virtual async Task<CommonResult<TDetailDto>> Detail(TKey id, CancellationToken cancellationToken = default)
    {
        Check.NotNull(id, nameof(id));
        var repo = GetRepository<TModel, TKey>();
        Check.NotNull(repo, nameof(repo));

        var model = await repo.GetAsync(id, cancellationToken);
        var result = ToDetailDto(model);
        await DetailAfterAsync(model, result);

        return CommonResult.Success(result);
    }

    /// <summary>
    ///     转换多条数据查询结果
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    protected virtual List<TListDto> ToListDto(List<TModel> model)
    {
        return model.MapTo<List<TListDto>>();
    }

    /// <summary>
    ///     转换单条数据查询结果
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    protected virtual TDetailDto ToDetailDto(TModel model)
    {
        return model.MapTo<TDetailDto>();
    }

    /// <summary>
    ///     单条数据查询后操作
    /// </summary>
    /// <param name="model"></param>
    /// <param name="dto"></param>
    protected virtual Task DetailAfterAsync(TModel model, TDetailDto dto)
    {
        return Task.CompletedTask;
    }
}