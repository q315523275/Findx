using Findx.Data;
using Findx.Linq;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

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
            multiOrderBy.OrderBy.Add(new OrderByParameter<TModel> { Expression = it => it.Id, Ascending = false });
            return multiOrderBy;
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        [HttpGet("pagerQuery")]
        [Obsolete(message: "Please use the page method")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual CommonResult PagerQuery([FromQuery] TQueryParameter request, [FromServices] IRepository<TModel> repository)
        {
            Check.NotNull(request, nameof(request));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var pageResult = repository.Paged<TDto>(request.PageNo, request.PageSize, whereExpression: whereExpression?.ToExpression(), orderByExpression: orderByExpression);

            return CommonResult.Success(ToPageResult(pageResult));
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        [HttpGet("page")]
        public virtual CommonResult Page([FromQuery] TQueryParameter request, [FromServices] IRepository<TModel> repository)
        {
            Check.NotNull(request, nameof(request));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var pageResult = repository.Paged<TDto>(request.PageNo, request.PageSize, whereExpression: whereExpression?.ToExpression(), orderByExpression: orderByExpression);

            return CommonResult.Success(ToPageResult(pageResult));
        }

        /// <summary>
        /// 转换分页查询结果
        /// </summary>
        /// <param name="result">分页查询结果</param>
        protected virtual dynamic ToPageResult(PageResult<List<TDto>> result) => result;

        /// <summary>
        /// 查询列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        [HttpGet("query")]
        [Obsolete(message: "Please use the list method")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual CommonResult Query([FromQuery] TQueryParameter request, [FromServices] IRepository<TModel> repository)
        {
            Check.NotNull(request, nameof(request));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var result = repository.Top<TDto>(request.PageSize, whereExpression: whereExpression?.ToExpression(), orderByExpression: orderByExpression);

            result = ToListResult(result);

            return CommonResult.Success(result);
        }

        /// <summary>
        /// 查询列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        [HttpGet("list")]
        public virtual CommonResult List([FromQuery] TQueryParameter request, [FromServices] IRepository<TModel> repository)
        {
            Check.NotNull(request, nameof(request));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var result = repository.Top<TDto>(request.PageSize, whereExpression: whereExpression?.ToExpression(), orderByExpression: orderByExpression);

            result = ToListResult(result);

            return CommonResult.Success(result);
        }

        /// <summary>
        /// 转换列表查询结果
        /// </summary>
        /// <param name="result">分页查询结果</param>
        protected virtual dynamic ToListResult(List<TDto> result) => result;

        /// <summary>
        /// 查询单条数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        [HttpGet("getById")]
        [Obsolete(message: "Please use the detail method")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual CommonResult GetById(TKey id, [FromServices] IRepository<TModel> repository)
        {
            Check.NotNull(id, nameof(id));

            var result = repository.Get(id);
            DetailAfter(repository, result);

            return CommonResult.Success(ToDetailResult(result));
        }

        /// <summary>
        /// 查询单条数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        [HttpGet("detail")]
        public virtual CommonResult Detail(TKey id, [FromServices] IRepository<TModel> repository)
        {
            Check.NotNull(id, nameof(id));

            var result = repository.Get(id);

            DetailAfter(repository, result);

            return CommonResult.Success(ToDetailResult(result));
        }

        /// <summary>
        /// 转换单条数据查询结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual dynamic ToDetailResult(TModel result) => result;

        /// <summary>
        /// 单条数据查询后操作
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="model"></param>
        protected virtual void DetailAfter(IRepository<TModel> repository, TModel model) { }
    }
}
