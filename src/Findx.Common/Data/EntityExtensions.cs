using System;
using System.Security.Principal;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Guids;
using Findx.Security;
using Findx.Utils;

namespace Findx.Data
{
	public static class EntityExtensions
	{
        /// <summary>
        /// 检测指定类型是否为<see cref="IEntity{TKey}"/>实体类型
        /// </summary>
        /// <param name="type">要判断的类型</param>
        /// <returns></returns>
        public static bool IsEntityType(this Type type)
        {
            Check.NotNull(type, nameof(type));
            return typeof(IEntity<>).IsGenericAssignableFrom(type) && !type.IsAbstract && !type.IsInterface;
        }

        /// <summary>
        /// 判断指定实体是否已过期
        /// </summary>
        /// <param name="entity">要检测的实体</param>
        /// <returns></returns>
        public static bool IsExpired(this IExpirable entity)
        {
            Check.NotNull(entity, nameof(entity));
            DateTime now = DateTime.Now;
            return entity.BeginTime != null && entity.BeginTime.Value > now || entity.EndTime != null && entity.EndTime.Value < now;
        }

        /// <summary>
        /// 检测并执行<see cref="ICreatedTime"/>接口的逻辑
        /// </summary>
        public static TEntity CheckICreatedTime<TEntity, TKey>(this TEntity entity)
            where TEntity : IEntity<TKey>
            where TKey : IEquatable<TKey>
        {
            if (entity is ICreatedTime entity1)
            {
                if (!entity1.CreatedTime.HasValue || entity1.CreatedTime == default(DateTime))
                {
                    entity1.CreatedTime = DateTime.Now;
                }
                return (TEntity)entity1;
            }
            else
            {
                return entity;
            }
        }

        /// <summary>
        /// 检测并执行<see cref="ICreationAudited{TUserKey}"/>接口的处理
        /// </summary>
        public static TEntity CheckICreationAudited<TEntity, TKey, TUserKey>(this TEntity entity, IPrincipal user)
            where TEntity : IEntity<TKey>
            where TKey : IEquatable<TKey>
            where TUserKey : struct
        {
            if (entity is ICreationAudited<TUserKey> entity1)
            {
                entity1.CreatorId = user.Identity.IsAuthenticated ? user.Identity.GetUserId<TUserKey>() : null;
                if (!entity1.CreatedTime.HasValue || entity1.CreatedTime == default(DateTime))
                {
                    entity1.CreatedTime = DateTime.Now;
                }
                return (TEntity)entity1;
            }
            else
            {
                return entity;
            }
        }

        /// <summary>
        /// 检测并执行<see cref="IUpdateAudited{TUserKey}"/>接口的处理
        /// </summary>
        public static TEntity CheckIUpdateAudited<TEntity, TKey, TUserKey>(this TEntity entity, IPrincipal user)
            where TEntity : IEntity<TKey>
            where TKey : IEquatable<TKey>
            where TUserKey : struct, IEquatable<TUserKey>
        {
            if (entity is IUpdateAudited<TUserKey> entity1)
            {
                entity1.LastUpdaterId = user.Identity.IsAuthenticated ? user.Identity.GetUserId<TUserKey>() : default;
                entity1.LastUpdatedTime = DateTime.Now;
                return (TEntity)entity1;
            }
            else
            {
                return entity;
            }
        }

        /// <summary>
        /// 设置实体id默认值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static IEntity<TKey> SetEmptyKey<TKey>(this IEntity<TKey> entity)
        {
            var keyType = typeof(TKey);
            // 雪花长整形
            if (typeof(long) == keyType && entity.Id.Equals(default(long)))
            {
                entity.Id = SnowflakeId.Default().NextId().CastTo<TKey>();
            }

            // 有序Guid
            if (typeof(Guid) == keyType && entity.Id.CastTo<Guid>() == Guid.Empty)
            {
                entity.Id = ServiceLocator.GetService<IGuidGenerator>().Create().CastTo<TKey>();
            }

            return entity;
        }

        /// <summary>
        /// 设置实体id默认值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="entity"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static IEntity<TKey> SetEmptyKey<TKey>(this IEntity<TKey> entity, DatabaseType dbType)
        {
            var keyType = typeof(TKey);
            // 雪花长整形
            if (typeof(long) == keyType && entity.Id.Equals(default(long)))
            {
                entity.Id = SnowflakeId.Default().NextId().CastTo<TKey>();
            }

            // 有序Guid
            if (typeof(Guid) == keyType && entity.Id.CastTo<Guid>() == Guid.Empty)
            {
                entity.Id = SequentialGuid.Instance.Create(dbType).CastTo<TKey>();
            }

            return entity;
        }
    }
}

