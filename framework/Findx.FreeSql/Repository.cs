using System;
using Findx.Data;

namespace Findx.FreeSql;

/// <summary>
///     默认仓储实现
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class Repository<TEntity>: RepositoryWithTypedId<TEntity, Guid>, IRepository<TEntity> where TEntity: class, IEntity<Guid>
{
    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="serviceProvider"></param>
    public Repository(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}