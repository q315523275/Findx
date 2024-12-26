using System.Security.Principal;
using Findx.Common;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Security;

namespace Findx.Data;

/// <summary>
///     实体扩展
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    ///     检测指定类型是否为<see cref="IEntity{TKey}" />实体类型
    /// </summary>
    /// <param name="type">要判断的类型</param>
    /// <returns></returns>
    public static bool IsEntityType(this Type type)
    {
        Check.NotNull(type, nameof(type));
        return type.IsBaseOn<IEntity>() && !type.IsAbstract && !type.IsInterface;
    }

    /// <summary>
    ///     获取指定实体扩展属性信息
    /// </summary>
    /// <param name="entityType"></param>
    /// <returns></returns>
    public static EntityExtensionAttribute GetEntityExtensionAttribute(this Type entityType)
    {
        var attribute = SingletonDictionary<Type, EntityExtensionAttribute>.Instance.GetOrAdd(entityType, () =>
        {
            var attribute = entityType.GetAttribute<EntityExtensionAttribute>() ?? new EntityExtensionAttribute();

            // 是否包含软删除
            attribute.HasSoftDeletable ??= entityType.IsBaseOn<ISoftDeletable>();

            // 是否包含表分片
            attribute.HasTableSharding ??= entityType.IsBaseOn<ITableSharding>();

            return attribute;
        });

        return attribute;
    }

    /// <summary>
    ///     获取指定实体扩展属性信息
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public static EntityExtensionAttribute GetEntityExtensionAttribute<TEntity>(this TEntity entity) where TEntity : IEntity
    {
        return entity.GetType().GetEntityExtensionAttribute();
    }

    /// <summary>
    ///     判断指定实体是否已过期
    /// </summary>
    /// <param name="entity">要检测的实体</param>
    /// <returns></returns>
    public static bool IsExpired(this IExpirable entity)
    {
        Check.NotNull(entity, nameof(entity));
        var now = DateTime.Now;
        return (entity.BeginTime != null && entity.BeginTime.Value > now) || (entity.EndTime != null && entity.EndTime.Value < now);
    }

    /// <summary>
    ///     检测并执行<see cref="ICreatedTime" />接口的逻辑
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static TEntity CheckCreatedTime<TEntity>(this TEntity entity) where TEntity : IEntity
    {
        if (entity is ICreatedTime entity1)
        {
            if (!entity1.CreatedTime.HasValue || entity1.CreatedTime == default(DateTime))
            {
                entity1.CreatedTime = DateTime.Now;
            }
            return (TEntity)entity1;
        }
        return entity;
    }

    /// <summary>
    ///     检测并执行<see cref="ICreationAudited{TUserKey}" />接口的处理
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TUserKey"></typeparam>
    /// <param name="entity"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static TEntity CheckCreationAudited<TEntity, TUserKey>(this TEntity entity, IPrincipal user) where TEntity : IEntity where TUserKey : struct
    {
        if (entity is ICreationAudited<TUserKey> entity1)
        {
            if (user is { Identity.IsAuthenticated: true })
            {
                entity1.CreatorId = user.Identity.GetUserId<TUserKey>();
                entity1.Creator = user.Identity.GetNickname();
            }
            if (!entity1.CreatedTime.HasValue || entity1.CreatedTime.Value == default)
            {
                entity1.CreatedTime = DateTime.Now;
            }
            return (TEntity)entity1;
        }

        return entity;
    }

    /// <summary>
    ///     检测并执行<see cref="IUpdateTime" />接口的逻辑
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static TEntity CheckUpdateTime<TEntity>(this TEntity entity) where TEntity : IEntity
    {
        if (entity is IUpdateTime entity1)
        {
            entity1.LastUpdatedTime = DateTime.Now;
            return (TEntity)entity1;
        }
        return entity;
    }

    /// <summary>
    ///     检测并执行<see cref="IUpdateAudited{TUserKey}" />接口的处理
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TUserKey"></typeparam>
    /// <param name="entity"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static TEntity CheckUpdateAudited<TEntity, TUserKey>(this TEntity entity, IPrincipal user) where TEntity : IEntity where TUserKey : struct
    {
        if (entity is IUpdateAudited<TUserKey> entity1)
        {
            if (user is { Identity.IsAuthenticated: true })
            {
                entity1.LastUpdaterId = user.Identity.GetUserId<TUserKey>();
                entity1.LastUpdater = user.Identity.GetNickname();
            }
            entity1.LastUpdatedTime = DateTime.Now;
            return (TEntity)entity1;
        }
        return entity;
    }

    /// <summary>
    ///     检测并执行<see cref="ITenant" />接口的处理
    ///     <para>TenantI:默认int</para>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entity"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static TEntity CheckTenant<TEntity>(this TEntity entity, IPrincipal user) where TEntity : IEntity
    {
        if (user.Identity != null && entity is ITenant entity1 && user is { Identity.IsAuthenticated: true })
        {
            entity1.TenantId = user.Identity.GetClaimValueFirstOrDefault(ClaimTypes.TenantId);
            return (TEntity)entity1;
        }
        return entity;
    }

    /// <summary>
    ///     检测并执行<see cref="ITenant{TTenantKey}" />接口的处理
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TTenantKey"></typeparam>
    /// <param name="entity"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static TEntity CheckTenant<TEntity, TTenantKey>(this TEntity entity, IPrincipal user) where TEntity : IEntity where TTenantKey : struct
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (entity is ITenant<TTenantKey> entity1 && user is { Identity.IsAuthenticated: true })
        {
            entity1.TenantId = user.Identity.GetTenantId<TTenantKey>();
            return (TEntity)entity1;
        }
        return entity;
    }
    
    /// <summary>
    ///     检测并执行<see cref="IDataOwner{TUserKey}" />接口的处理
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TUserKey"></typeparam>
    /// <param name="entity"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static TEntity CheckOwner<TEntity, TUserKey>(this TEntity entity, IPrincipal user) where TEntity : IEntity where TUserKey : struct
    {
        if (entity is IDataOwner<TUserKey> entity1 && user is { Identity.IsAuthenticated: true })
        {
            entity1.OwnerId = user.Identity.GetUserId<TUserKey>();
            entity1.Owner = user.Identity.GetNickname();
            return (TEntity)entity1;
        }
        return entity;
    }
    
    /// <summary>
    ///     检测并执行<see cref="IDataOwner{TUserKey}" />接口的处理
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TOrgKey"></typeparam>
    /// <param name="entity"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static TEntity CheckOrg<TEntity, TOrgKey>(this TEntity entity, IPrincipal user) where TEntity : IEntity where TOrgKey : struct
    {
        if (entity is IDataDepartment<TOrgKey> entity1 && user is { Identity.IsAuthenticated: true })
        {
            entity1.OrgId = user.Identity.GetOrgId<TOrgKey>();
            entity1.OrgName = user.Identity.GetOrgName();
            return (TEntity)entity1;
        }
        return entity;
    }
    
    /// <summary>
    ///     设置实体id默认值
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static IEntity<TKey> SetEmptyKey<TKey>(this IEntity<TKey> entity)
    {
        var keyType = typeof(TKey);
        
        // 雪花长整形
        if (typeof(long) == keyType && entity.Id.Equals(default(long))) 
            entity.Id = ServiceLocator.GetService<IKeyGenerator<long>>().Create().CastTo<TKey>();

        // 有序Guid
        if (typeof(Guid) == keyType && entity.Id.CastTo<Guid>() == Guid.Empty)
            entity.Id = ServiceLocator.GetService<IKeyGenerator<Guid>>().Create().CastTo<TKey>();

        return entity;
    }
}