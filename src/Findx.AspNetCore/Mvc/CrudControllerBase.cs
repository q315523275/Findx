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
    /// 增删改查通用控制器基类
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TCreateRequest"></typeparam>
    /// <typeparam name="TUpdateRequest"></typeparam>
    /// <typeparam name="TQueryParameter"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public abstract class CrudControllerBase<TModel, TDto, TCreateRequest, TUpdateRequest, TQueryParameter, TKey> : QueryControllerBase<TModel, TDto, TQueryParameter, TKey>
        where TModel : class, new()
        where TDto : IResponse, new()
        where TCreateRequest : IRequest, new()
        where TUpdateRequest : IRequest, new()
        where TQueryParameter : IPager, new()
    {
        /// <summary>
        /// 创建前操作
        /// </summary>
        /// <param name="dto">创建参数</param>
        protected virtual void CreateBefore(TCreateRequest dto) { }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repository"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual CommonResult Create([FromBody] TCreateRequest request, [FromServices] IRepository<TModel> repository, [FromServices] IMapper mapper)
        {
            Check.NotNull(request, nameof(request));

            CreateBefore(request);

            var model = mapper.MapTo<TModel>(request);

            if (repository.Insert(model) > 0)
                return CommonResult.Success();
            else
                return CommonResult.Fail("db.create.error", "数据创建失败");
        }

        /// <summary>
        /// 修改前操作
        /// </summary>
        /// <param name="dto">修改参数</param>
        protected virtual void UpdateBefore(TUpdateRequest dto) { }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repository"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual CommonResult Update([FromBody] TUpdateRequest request, [FromServices] IRepository<TModel> repository, [FromServices] IMapper mapper)
        {
            Check.NotNull(request, nameof(request));

            var model = mapper.MapTo<TModel>(request);

            UpdateBefore(request);

            if (repository.Update(model) > 0)
                return CommonResult.Success();
            else
                return CommonResult.Fail("db.update.error", "数据更新失败");
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual CommonResult DeleteById(TKey id, [FromServices] IRepository<TModel> repository)
        {
            Check.NotNull(id, nameof(id));

            if (repository.Delete(key: id) > 0)
                return CommonResult.Success();
            else
                return CommonResult.Fail("db.delete.error", "数据删除失败");
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual CommonResult DeleteMany([FromBody] List<TKey> ids, [FromServices] IRepository<TModel> repository)
        {
            Check.NotNull(ids, nameof(ids));

            int total = 0;
            foreach (var id in ids)
            {
                if (repository.Delete(key: id) > 0)
                    total++;
            }
            return CommonResult.Success($"共删除{total}条数据,失败{ids.Count - total}条");
        }
    }
}
