using Findx.Data;
using Findx.Extensions;
using Findx.Mapping;
using Findx.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

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
    /// <typeparam name="TKey">实体主键类型</typeparam>
    /// <typeparam name="TUserKey">用户字段类型</typeparam>
    public abstract class CrudControllerBase<TModel, TDto, TCreateRequest, TUpdateRequest, TQueryParameter, TKey, TUserKey> : QueryControllerBase<TModel, TDto, TQueryParameter, TKey>
        where TModel : EntityBase<TKey>, new()
        where TDto : IResponse, new()
        where TCreateRequest : IRequest, new()
        where TUpdateRequest : IRequest, new()
        where TQueryParameter : IPager, new()
        where TKey : struct
        where TUserKey : struct
    {
        /// <summary>
        /// 创建前操作
        /// </summary>
        /// <param name="dto">创建参数</param>
        protected virtual void AddBefore(TCreateRequest dto) { }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repository"></param>
        /// <param name="mapper"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        [HttpPost("create")]
        [Obsolete(message: "Please use the add method")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual CommonResult Create([FromBody] TCreateRequest request, [FromServices] IRepository<TModel> repository, [FromServices] IMapper mapper, [FromServices] ICurrentUser currentUser)
        {
            Check.NotNull(request, nameof(request));

            AddBefore(request);

            var model = mapper.MapTo<TModel>(request);

            // 创建时间
            if (model is ICreateTime time)
                time.CreateTime = DateTime.Now;

            // 创建人
            if (model is ICreateUser<TUserKey> user)
                user.CreateUser = currentUser?.UserId?.CastTo<TUserKey>();

            if (repository.Insert(model) > 0)
                return CommonResult.Success();
            else
                return CommonResult.Fail("db.create.error", "数据创建失败");
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repository"></param>
        /// <param name="mapper"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public virtual CommonResult Add([FromBody] TCreateRequest request, [FromServices] IRepository<TModel> repository, [FromServices] IMapper mapper, [FromServices] ICurrentUser currentUser)
        {
            Check.NotNull(request, nameof(request));

            AddBefore(request);

            var model = mapper.MapTo<TModel>(request);

            // 创建时间
            if (model is ICreateTime time)
                time.CreateTime = DateTime.Now;

            // 创建人
            if (model is ICreateUser<TUserKey> user)
                user.CreateUser = currentUser?.UserId?.CastTo<TUserKey>();

            if (repository.Insert(model) > 0)
                return CommonResult.Success();
            else
                return CommonResult.Fail("db.add.error", "数据创建失败");
        }

        /// <summary>
        /// 修改前操作
        /// </summary>
        /// <param name="dto">修改参数</param>
        protected virtual void EditBefore(TUpdateRequest dto) { }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repository"></param>
        /// <param name="mapper"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        [HttpPost("update")]
        [Obsolete(message: "Please use the edit method")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public virtual CommonResult Update([FromBody] TUpdateRequest request, [FromServices] IRepository<TModel> repository, [FromServices] IMapper mapper, [FromServices] ICurrentUser currentUser)
        {
            Check.NotNull(request, nameof(request));

            var model = mapper.MapTo<TModel>(request);

            EditBefore(request);

            // 修改时间
            if (model is IUpdateTime updateTime)
                updateTime.UpdateTime = DateTime.Now;
            // 修改信息
            if (model is IUpdateUser<TUserKey> updateUser)
                updateUser.UpdateUser = currentUser?.UserId?.CastTo<TUserKey>();

            if (repository.Update(model, true) > 0)
                return CommonResult.Success();
            else
                return CommonResult.Fail("db.update.error", "数据更新失败");
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="request"></param>
        /// <param name="repository"></param>
        /// <param name="mapper"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        [HttpPost("edit")]
        public virtual CommonResult Edit([FromBody] TUpdateRequest request, [FromServices] IRepository<TModel> repository, [FromServices] IMapper mapper, [FromServices] ICurrentUser currentUser)
        {
            Check.NotNull(request, nameof(request));

            EditBefore(request);

            var model = mapper.MapTo<TModel>(request);

            // 修改时间
            if (model is IUpdateTime updateTime)
                updateTime.UpdateTime = DateTime.Now;
            // 修改信息
            if (model is IUpdateUser<TUserKey> updateUser)
                updateUser.UpdateUser = currentUser?.UserId?.CastTo<TUserKey>();

            if (repository.Update(model, true) > 0)
                return CommonResult.Success();
            else
                return CommonResult.Fail("db.edit.error", "数据更新失败");
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        [HttpGet("deleteById")]
        [Obsolete(message: "Please use the delete method")]
        [ApiExplorerSettings(IgnoreApi = true)]
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
        [HttpPost("deleteMany")]
        [Obsolete(message: "Please use the delete method")]
        [ApiExplorerSettings(IgnoreApi = true)]
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

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        [HttpPost("delete")]
        public virtual CommonResult Delete([FromBody] List<TKey> ids, [FromServices] IRepository<TModel> repository)
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
