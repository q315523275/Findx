﻿using Findx.Data;
using Findx.Linq;
using Findx.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Findx.AspNetCore.Mvc
{
    /// <summary>
    /// 通用查询控制器基类
    /// </summary>
    public abstract class QueryControllerBase<TModel, TDto, TQueryParameter, TKey> : QueryControllerBase<TModel, TDto, TDto, TQueryParameter, TKey>
        where TModel : EntityBase<TKey>, new()
        where TDto : IResponse, new()
        where TQueryParameter : IPager, new()
        where TKey : IEquatable<TKey>
    {

    }

        /// <summary>
        /// 通用查询控制器基类
        /// </summary>
        public abstract class QueryControllerBase<TModel, TListDto, TDetailDto, TQueryParameter, TKey> : ApiControllerBase
        where TModel : EntityBase<TKey>, new()
        where TListDto : IResponse, new()
        where TDetailDto : IResponse, new()
        where TQueryParameter : IPager, new()
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// 构建分页查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual Expressionable<TModel> CreatePageWhereExpression(TQueryParameter request)
        {
            return null;
        }

        /// <summary>
        /// 构建分页查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual List<OrderByParameter<TModel>> CreatePageOrderExpression(TQueryParameter request)
        {
            var orderExp = ExpressionBuilder.CreateOrder<TModel>();
            if (typeof(TModel).IsAssignableTo(typeof(ISort)))
                orderExp.OrderBy(it => (it as ISort).Sort);
            orderExp.OrderByDescending(it => it.Id);
            return orderExp.ToSort();
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("page")]
        [Description("分页")]
        public virtual async Task<CommonResult> PageAsync([FromQuery] TQueryParameter request)
        {
            Check.NotNull(request, nameof(request));

            var repo = GetRepository<TModel>();

            Check.NotNull(repo, nameof(repo));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var result = await repo.PagedAsync<TListDto>(request.PageNo, request.PageSize, whereExpression: whereExpression?.ToExpression(), orderParameters: orderByExpression.ToArray());

            return CommonResult.Success(result);
        }

        /// <summary>
        /// 查询列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [Description("列表")]
        public virtual async Task<CommonResult> ListAsync([FromQuery] TQueryParameter request)
        {
            Check.NotNull(request, nameof(request));

            var repo = GetRepository<TModel>();

            Check.NotNull(repo, nameof(repo));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var list = await repo.TopAsync<TListDto>(request.PageSize, whereExpression: whereExpression?.ToExpression(), orderParameters: orderByExpression.ToArray());

            return CommonResult.Success(list);
        }

        /// <summary>
        /// 查询单条数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("detail")]
        [Description("详情")]
        public virtual async Task<CommonResult> Detail(TKey id)
        {
            Check.NotNull(id, nameof(id));

            var repo = GetRepository<TModel>();

            Check.NotNull(repo, nameof(repo));

            var model = repo.Get(id);

            var result = ToDetailDto(model);

            await DetailAfterAsync(model, result);

            return CommonResult.Success(result);
        }

        /// <summary>
        /// 转换多条数据查询结果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected virtual List<TListDto> ToListDto(TModel model) => model.MapTo<List<TListDto>>();

        /// <summary>
        /// 转换单条数据查询结果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected virtual TDetailDto ToDetailDto(TModel model) => model.MapTo<TDetailDto>();

        /// <summary>
        /// 单条数据查询后操作
        /// </summary>
        /// <param name="model"></param>
        /// <param name="dto"></param>
        protected virtual Task DetailAfterAsync(TModel model, TDetailDto dto) => Task.CompletedTask;
    }
}
