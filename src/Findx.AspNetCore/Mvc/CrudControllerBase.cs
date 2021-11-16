using Findx.Data;
using Findx.Extensions;
using Findx.Mapping;
using Findx.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// 创建参数转换为实体
        /// </summary>
        /// <param name="request">创建参数</param>
        protected virtual TModel ToModelFromCreateRequest(TCreateRequest request) => request.MapTo<TModel>();

        /// <summary>
        /// 修改参数转换为实体
        /// </summary>
        /// <param name="request">修改参数</param>
        protected virtual TModel ToModelFromUpdateRequest(TUpdateRequest request) => request.MapTo<TModel>();

        /// <summary>
        /// 创建前操作
        /// </summary>
        /// <param name="model">实体</param>
        protected virtual Task AddBeforeAsync(TModel model) => Task.CompletedTask;

        /// <summary>
        /// 创建后操作
        /// </summary>
        /// <param name="model">实体</param>
        /// <param name="result">添加结果</param>
        protected virtual Task AddAfterAsync(TModel model, int result) => Task.CompletedTask;

        /// <summary>
        /// 修改前操作
        /// </summary>
        /// <param name="model">修改参数</param>
        protected virtual Task EditBeforeAsync(TModel model) => Task.CompletedTask;

        /// <summary>
        /// 修改后操作
        /// </summary>
        /// <param name="model">修改参数</param>
        /// <param name="result">处理结果</param>
        protected virtual Task EditAfterAsync(TModel model, int result) => Task.CompletedTask;

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public virtual async Task<CommonResult> AddAsync([FromBody] TCreateRequest request)
        {
            Check.NotNull(request, nameof(request));

            var repo = GetRepository<TModel>();
            var currentUser = GetService<ICurrentUser>();

            Check.NotNull(repo, nameof(repo));
            Check.NotNull(currentUser, nameof(currentUser));

            var model = ToModelFromCreateRequest(request);

            Check.NotNull(model, nameof(model));

            // 创建时间
            if (model is ICreateTime time)
                time.CreateTime = DateTime.Now;

            // 创建人
            if (model is ICreateUser<TUserKey> user)
                user.CreateUser = currentUser?.UserId?.CastTo<TUserKey>();

            await AddBeforeAsync(model);
            model.Init();
            var res = repo.Insert(model);
            await AddAfterAsync(model, res);

            if (res > 0)
                return CommonResult.Success();
            else
                return CommonResult.Fail("db.add.error", "数据创建失败");
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("edit")]
        public virtual async Task<CommonResult> EditAsync([FromBody] TUpdateRequest request)
        {
            Check.NotNull(request, nameof(request));

            var repo = GetRepository<TModel>();
            var currentUser = GetService<ICurrentUser>();

            Check.NotNull(repo, nameof(repo));
            Check.NotNull(currentUser, nameof(currentUser));

            var model = ToModelFromUpdateRequest(request);

            Check.NotNull(model, nameof(model));

            // 修改时间
            if (model is IUpdateTime updateTime)
                updateTime.UpdateTime = DateTime.Now;
            // 修改信息
            if (model is IUpdateUser<TUserKey> updateUser)
                updateUser.UpdateUser = currentUser?.UserId?.CastTo<TUserKey>();

            await EditBeforeAsync(model);
            var res = repo.Update(model, true);
            await EditAfterAsync(model, res);

            if (res > 0)
                return CommonResult.Success();
            else
                return CommonResult.Fail("db.edit.error", "数据更新失败");
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("delete")]
        public virtual Task<CommonResult> DeleteAsync([FromBody] List<DeleteParam<TKey>> request)
        {
            Check.NotNull(request, nameof(request));
            if (request.Count == 0)
                return Task.FromResult(CommonResult.Fail("delete.not.count", "不存在删除数据"));

            var repo = GetRepository<TModel>();
            var currentUser = GetService<ICurrentUser>();

            Check.NotNull(repo, nameof(repo));
            Check.NotNull(currentUser, nameof(currentUser));

            int total = 0;
            foreach (var item in request)
            {
                if (repo.Delete(key: item.Id) > 0)
                    total++;
            }
            return Task.FromResult(CommonResult.Success($"共删除{total}条数据,失败{request.Count - total}条"));
        }
    }

    /// <summary>
    /// 通用增删改查 - 删除入参
    /// </summary>
    /// <typeparam name="TTKey"></typeparam>
    public class DeleteParam<TTKey> where TTKey : struct
    {
        /// <summary>
        /// 删除编号
        /// </summary>
        public TTKey Id { set; get; }
    }
}
