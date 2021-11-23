using Findx.Data;
using Findx.Linq;
using Findx.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Findx.AspNetCore.Mvc
{
    /// <summary>
    /// 通用查询控制器基类
    /// </summary>
    public abstract class QueryControllerBase<TModel, TDto, TQueryParameter, TKey> : ApiControllerBase
        where TModel : EntityBase<TKey>, new()
        where TDto : IResponse, new()
        where TQueryParameter : IPager, new()
        where TKey : struct
    {
        /// <summary>
        /// 构建分页查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual Expressionable<TModel> CreatePageWhereExpression(TQueryParameter request)
        {
            // ExpressionBuilder.Create<TModel>();
            return null;
        }

        /// <summary>
        /// 构建分页查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual MultiOrderBy<TModel> CreatePageOrderExpression(TQueryParameter request)
        {
            var multiOrderBy = new MultiOrderBy<TModel>();
            if (typeof(TModel).IsAssignableTo(typeof(ISort)))
                multiOrderBy.OrderBy.Add(new OrderByParameter<TModel> { Expression = it => (it as ISort).Sort, SortDirection = ListSortDirection.Ascending });
            multiOrderBy.OrderBy.Add(new OrderByParameter<TModel> { Expression = it => it.Id, SortDirection = ListSortDirection.Descending });
            return multiOrderBy;
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("page")]
        public virtual async Task<CommonResult> PageAsync([FromQuery] TQueryParameter request)
        {
            Check.NotNull(request, nameof(request));

            var repo = GetRepository<TModel>();

            Check.NotNull(repo, nameof(repo));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var result = await repo.PagedAsync<TDto>(request.PageNo, request.PageSize, whereExpression: whereExpression?.ToExpression(), orderByExpression: orderByExpression);

            return CommonResult.Success(result);
        }

        /// <summary>
        /// 查询列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public virtual async Task<CommonResult> ListAsync([FromQuery] TQueryParameter request)
        {
            var js = DateTime.Now;

            Check.NotNull(request, nameof(request));

            var repo = GetRepository<TModel>();

            Check.NotNull(repo, nameof(repo));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var list = await repo.TopAsync<TDto>(request.PageSize, whereExpression: whereExpression?.ToExpression(), orderByExpression: orderByExpression);

            Debug.WriteLine($"动态API查询接口耗时:{(DateTime.Now - js).TotalMilliseconds:0.000}毫秒");

            return CommonResult.Success(list);
        }

        /// <summary>
        /// 查询单条数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("detail")]
        public virtual async Task<CommonResult> Detail(TKey id)
        {
            Check.NotNull(id, nameof(id));

            var repo = GetRepository<TModel>();

            Check.NotNull(repo, nameof(repo));

            var model = repo.Get(id);

            await DetailAfterAsync(model);

            return CommonResult.Success(ToDto(model));
        }

        /// <summary>
        /// 转换单条数据查询结果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected virtual TDto ToDto(TModel model) => model.MapTo<TDto>();

        /// <summary>
        /// 单条数据查询后操作
        /// </summary>
        /// <param name="model"></param>
        protected virtual Task DetailAfterAsync(TModel model) => Task.CompletedTask;



        /// <summary>
        /// 获取仓储方法
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        protected IRepository<TEntity> GetRepository<TEntity>() where TEntity : class, new()
        {
            return Request?.HttpContext?.RequestServices.GetService<IRepository<TEntity>>();
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T GetService<T>()
        {
            return Request.HttpContext.RequestServices.GetService<T>();
        }
    }
}
