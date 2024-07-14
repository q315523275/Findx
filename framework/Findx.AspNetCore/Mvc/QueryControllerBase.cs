using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Data;
using Findx.Extensions;
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
    where TQueryParameter : class, IPager, new()
    where TKey : IEquatable<TKey>;

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
    where TQueryParameter : class, IPager, new()
    where TKey : IEquatable<TKey>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConcurrentDictionary<Type, Dictionary<PropertyInfo, QueryFieldAttribute>> QueryFieldDict = new();
    
    /// <summary>
    ///     构建分页查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected virtual Expression<Func<TModel, bool>> CreatePageWhereExpression(TQueryParameter request)
    {
        var filters = new List<FilterCondition>();
        var reqType = request.GetType();
        
        // 属性标记查询字段
        var queryFields = QueryFieldDict.GetOrAdd(reqType, () =>
        {
            var fieldDict = new Dictionary<PropertyInfo, QueryFieldAttribute>();
            var queryFieldProperties = reqType.GetProperties().Where(x => x.HasAttribute<QueryFieldAttribute>());
            foreach (var propertyInfo in queryFieldProperties)
            {
                fieldDict[propertyInfo] = propertyInfo.GetAttribute<QueryFieldAttribute>();
            }
            return fieldDict;
        });
        
        // 标记字段计算
        foreach (var fieldInfo in queryFields)
        {
            var value = PropertyValueGetter<TQueryParameter>.GetPropertyValue<string>(request, fieldInfo.Key.Name);
            if (value.IsNotNullOrWhiteSpace())
                filters.Add(new FilterCondition { Field = fieldInfo.Value?.Name?? fieldInfo.Key.Name, Operator = fieldInfo.Value?.FilterOperate?? FilterOperate.Equal, Value = value });
        }
        
        return filters.Any() ? LambdaExpressionParser.ParseConditions<TModel>(new FilterGroup { Filters = filters, Logic = FilterOperate.And }) : null;
    }

    /// <summary>
    ///     构建分页查询条件
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    protected virtual IEnumerable<OrderByParameter<TModel>> CreatePageOrderExpression(TQueryParameter request)
    {
        var orderExp = SortConditionBuilder.New<TModel>();

        if (request is SortCondition sortCondition && sortCondition.SortField.IsNotNullOrWhiteSpace())
            orderExp.Order(sortCondition.SortField, sortCondition.SortDirection);
        
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