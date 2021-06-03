using Findx.Data;
using Findx.Linq;
using Findx.Mapping;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
        /// <param name="dto">修改参数</param>
        protected virtual Expressionable<TModel> CreatePageWhereExpression(TQueryParameter request)
        {
            return ExpressionBuilder.Create<TModel>();
        }

        /// <summary>
        /// 构建分页查询条件
        /// </summary>
        /// <param name="dto">修改参数</param>
        protected virtual Expression<Func<TModel, object>> CreatePageOrderExpression(TQueryParameter request)
        {
            return null;
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual CommonResult PagerQuery([FromQuery] TQueryParameter request, [FromServices] IRepository<TModel> repository, [FromServices] IMapper mapper)
        {
            Check.NotNull(request, nameof(request));

            var whereExpression = CreatePageWhereExpression(request);
            var orderByExpression = CreatePageOrderExpression(request);

            var pageResult = repository.Paged(request.PageNo, request.PageSize, whereExpression: whereExpression.ToExpression(), orderByExpression: orderByExpression, ascending: request.Asc);

            return CommonResult.Success(pageResult);
        }

        /// <summary>
        /// 转换分页查询结果
        /// </summary>
        /// <param name="result">分页查询结果</param>
        protected virtual dynamic ToPagerQueryResult(PagedResult<List<TModel>> result) => result;

        /// <summary>
        /// 查询单条数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual CommonResult GetById(TKey id, [FromServices] IRepository<TModel> repository)
        {
            Check.NotNull(id, nameof(id));
            return CommonResult.Success(repository.Get(id));
        }
    }
}
