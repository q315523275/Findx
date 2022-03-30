using Findx.Data;
using Findx.Extensions;
using Findx.Mapping;
using Findx.Security;
using Findx.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Findx.AspNetCore.Mvc
{
    /// <summary>
    /// 增删改查通用控制器基类
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TQueryParameter"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TUserKey"></typeparam>
    public abstract class CrudControllerBase<TModel, TRequest, TQueryParameter, TKey, TUserKey>: CrudControllerBase<TModel, TModel, TModel, TRequest, TRequest, TQueryParameter, TKey, TUserKey>
                where TModel : EntityBase<TKey>, IResponse, new()
        where TRequest : IRequest, new()
        where TQueryParameter : IPager, new()
        where TKey : IEquatable<TKey>
        where TUserKey : struct
    {

    }

    /// <summary>
    /// 增删改查通用控制器基类
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TQueryParameter"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TUserKey"></typeparam>
    public abstract class CrudControllerBase<TModel, TDto, TRequest, TQueryParameter, TKey, TUserKey> : CrudControllerBase<TModel, TDto, TDto, TRequest, TRequest, TQueryParameter, TKey, TUserKey>
        where TModel : EntityBase<TKey>, new()
        where TDto : IResponse, new()
        where TRequest : IRequest, new()
        where TQueryParameter : IPager, new()
        where TKey : IEquatable<TKey>
        where TUserKey : struct
    {

    }

    /// <summary>
    /// 增删改查通用控制器基类
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TQueryParameter"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TUserKey"></typeparam>
    public abstract class CrudControllerBase<TModel, TDto, TCreateRequest, TUpdateRequest, TQueryParameter, TKey, TUserKey> : CrudControllerBase<TModel, TDto, TDto, TCreateRequest, TUpdateRequest, TQueryParameter, TKey, TUserKey>
        where TModel : EntityBase<TKey>, new()
        where TDto : IResponse, new()
        where TCreateRequest : IRequest, new()
        where TUpdateRequest : IRequest, new()
        where TQueryParameter : IPager, new()
        where TKey : IEquatable<TKey>
        where TUserKey : struct
    {

    }

    /// <summary>
    /// 增删改查通用控制器基类
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TListDto"></typeparam>
    /// <typeparam name="TDetailDto"></typeparam>
    /// <typeparam name="TCreateRequest"></typeparam>
    /// <typeparam name="TUpdateRequest"></typeparam>
    /// <typeparam name="TQueryParameter"></typeparam>
    /// <typeparam name="TKey">实体主键类型</typeparam>
    /// <typeparam name="TUserKey">用户字段类型</typeparam>
    public abstract class CrudControllerBase<TModel, TListDto, TDetailDto, TCreateRequest, TUpdateRequest, TQueryParameter, TKey, TUserKey>
        : QueryControllerBase<TModel, TListDto, TDetailDto, TQueryParameter, TKey>
        where TModel : EntityBase<TKey>, new()
        where TListDto : IResponse, new()
        where TDetailDto : IResponse, new()
        where TCreateRequest : IRequest, new()
        where TUpdateRequest : IRequest, new()
        where TQueryParameter : IPager, new()
        where TKey : IEquatable<TKey>
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
        /// 删除前前操作
        /// </summary>
        /// <param name="req">id集合</param>
        protected virtual Task DeleteBeforeAsync(List<DeleteParam<TKey>> req) => Task.CompletedTask;

        /// <summary>
        /// 删除后操作
        /// </summary>
        /// <param name="req">id集合</param>
        /// <param name="total">删除成功条数</param>
        protected virtual Task DeleteAfterAsync(List<DeleteParam<TKey>> req, int total) => Task.CompletedTask;

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
            var principal = GetService<IPrincipal>();

            Check.NotNull(repo, nameof(repo));

            var model = ToModelFromCreateRequest(request);

            Check.NotNull(model, nameof(model));

            model.CheckICreatedTime<TModel, TKey>();
            model.CheckICreationAudited<TModel, TKey, TUserKey>(principal);

            var keyType = typeof(TKey);
            // 雪花长整形
            if (typeof(long) == keyType && model.Id.Equals(default(long)))
            {
                model.Id = SnowflakeId.Default().NextId().CastTo<TKey>();
            }

            // 有序Guid
            if (typeof(Guid) == keyType && model.Id.CastTo<Guid>() == Guid.Empty)
            {
                var dbType = repo.GetDbType();
                model.Id = SequentialGuid.Instance.Create(dbType).CastTo<TKey>();
            }

            await AddBeforeAsync(model);
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
                updateTime.LastUpdatedTime = DateTime.Now;

            // 修改信息
            if (model is IUpdateAudited<TUserKey> updateUser)
            {
                updateUser.LastUpdatedTime = DateTime.Now;
                updateUser.LastUpdaterId = currentUser?.UserId?.CastTo<TUserKey>();
            }

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
        public virtual async Task<CommonResult> DeleteAsync([FromBody] List<DeleteParam<TKey>> request)
        {
            Check.NotNull(request, nameof(request));
            if (request.Count == 0)
                return CommonResult.Fail("delete.not.count", "不存在删除数据");

            var repo = GetRepository<TModel>();
            var currentUser = GetService<ICurrentUser>();

            Check.NotNull(repo, nameof(repo));
            Check.NotNull(currentUser, nameof(currentUser));

            await DeleteBeforeAsync(request);

            int total = 0;
            foreach (var item in request)
            {
                if (repo.Delete(key: item.Id) > 0)
                    total++;
            }

            await DeleteAfterAsync(request, total);

            return CommonResult.Success($"共删除{total}条数据,失败{request.Count - total}条");
        }
    }

    /// <summary>
    /// 通用增删改查 - 删除入参
    /// </summary>
    /// <typeparam name="TTKey"></typeparam>
    public class DeleteParam<TTKey> where TTKey : IEquatable<TTKey>
    {
        /// <summary>
        /// 删除编号
        /// </summary>
        public TTKey Id { set; get; }
    }
}
