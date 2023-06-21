using System;
using Findx.Data;

namespace Findx.FreeSql;

public class Repository<TEntity>: RepositoryWithTypedId<TEntity, Guid>, IRepository<TEntity> where TEntity: class, IEntity<Guid>
{
    public Repository(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}