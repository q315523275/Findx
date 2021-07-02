using Findx.Data;
using Findx.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Findx.AspNetCore.Mvc
{
    /// <summary>
    /// 通用查询控制器基类
    /// </summary>
    public abstract class QueryControllerBase<TModel, TDto, TQueryParameter, TKey> : ApiControllerBase
        where TModel : class, new()
        where TDto : IResponse, new()
        where TQueryParameter : IPager, new()
    {
        /// <summary>
        /// 构建分页查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual Expressionable<TModel> CreatePageWhereExpression(TQueryParameter request)
        {
            return ExpressionBuilder.Create<TModel>();
        }

        /// <summary>
        /// 构建分页查询条件
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual MultiOrderBy<TModel> CreatePageOrderExpression(TQueryParameter request)
        {
            return null;
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual CommonResult PagerQuery([FromQuery] TQueryParameter request, [FromServices] IRepository<TModel> repository)
        {
            Check.NotNull(request, nameof(request));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var pageResult = repository.Paged<TDto>(request.PageNo, request.PageSize, whereExpression: whereExpression.ToExpression(), orderByExpression: orderByExpression);

            return CommonResult.Success(ToPagerQueryResult(pageResult));
        }

        /// <summary>
        /// 转换分页查询结果
        /// </summary>
        /// <param name="result">分页查询结果</param>
        protected virtual dynamic ToPagerQueryResult(PagedResult<List<TDto>> result) => result;

        /// <summary>
        /// 查询列表数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual CommonResult Query([FromQuery] TQueryParameter request, [FromServices] IRepository<TModel> repository)
        {
            Check.NotNull(request, nameof(request));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var result = repository.Top<TDto>(request.PageSize, whereExpression: whereExpression.ToExpression(), orderByExpression: orderByExpression);

            return CommonResult.Success(ToQueryResult(result));
        }

        /// <summary>
        /// 转换列表查询结果
        /// </summary>
        /// <param name="result">分页查询结果</param>
        protected virtual dynamic ToQueryResult(List<TDto> result) => result;

        /// <summary>
        /// 查询单条数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual CommonResult GetById(TKey id, [FromServices] IRepository<TModel> repository)
        {
            Check.NotNull(id, nameof(id));

            var result = repository.Get(id);
            AfterGetById(repository, result);

            return CommonResult.Success(ToGetByIdResult(result));
        }

        /// <summary>
        /// 转换单条数据查询结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual dynamic ToGetByIdResult(TModel result) => result;

        /// <summary>
        /// 单条数据查询后操作
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="model"></param>
        protected virtual void AfterGetById(IRepository<TModel> repository, TModel model) { }
    }
}
