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
    public abstract class QueryControllerBase<TModel, TDTO, TQueryParameter, TKey> : QueryControllerBase<TModel, TDTO, TDTO, TQueryParameter, TKey>
        where TModel : EntityBase<TKey>, new()
        where TDTO : IResponse, new()
        where TQueryParameter : IPager, new()
        where TKey : IEquatable<TKey>
    {

    }

        /// <summary>
        /// 通用查询控制器基类
        /// </summary>
        public abstract class QueryControllerBase<TModel, TListDTO, TDetailDTO, TQueryParameter, TKey> : ApiControllerBase
        where TModel : EntityBase<TKey>, new()
        where TListDTO : IResponse, new()
        where TDetailDTO : IResponse, new()
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
            var multiOrderBy = new List<OrderByParameter<TModel>>();
            if (typeof(TModel).IsAssignableTo(typeof(ISort)))
                multiOrderBy.Add(new OrderByParameter<TModel> { Expression = it => (it as ISort).Sort, SortDirection = ListSortDirection.Ascending });
            multiOrderBy.Add(new OrderByParameter<TModel> { Expression = it => it.Id, SortDirection = ListSortDirection.Descending });
            return multiOrderBy;
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

            var result = await repo.PagedAsync<TListDTO>(request.PageNo, request.PageSize, whereExpression: whereExpression?.ToExpression(), orderParameters: orderByExpression.ToArray());

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
            var js = DateTime.Now;

            Check.NotNull(request, nameof(request));

            var repo = GetRepository<TModel>();

            Check.NotNull(repo, nameof(repo));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var list = await repo.TopAsync<TListDTO>(request.PageSize, whereExpression: whereExpression?.ToExpression(), orderParameters: orderByExpression.ToArray());

            Debug.WriteLine($"动态API查询接口耗时:{(DateTime.Now - js).TotalMilliseconds:0.000}毫秒");

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

            var result = ToDetailDTO(model);

            await DetailAfterAsync(model, result);

            return CommonResult.Success(result);
        }

        /// <summary>
        /// 转换多条数据查询结果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected virtual List<TListDTO> ToListDTO(TModel model) => model.MapTo<List<TListDTO>>();

        /// <summary>
        /// 转换单条数据查询结果
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected virtual TDetailDTO ToDetailDTO(TModel model) => model.MapTo<TDetailDTO>();

        /// <summary>
        /// 单条数据查询后操作
        /// </summary>
        /// <param name="model"></param>
        protected virtual Task DetailAfterAsync(TModel model, TDetailDTO dto) => Task.CompletedTask;



        /// <summary>
        /// 获取仓储方法
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        protected IRepository<TEntity> GetRepository<TEntity>() where TEntity : class, new()
        {
            return Request?.HttpContext?.RequestServices.GetRequiredService<IRepository<TEntity>>();
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T GetService<T>()
        {
            return Request.HttpContext.RequestServices.GetRequiredService<T>();
        }
    }
}
