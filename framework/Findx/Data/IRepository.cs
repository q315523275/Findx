namespace Findx.Data;

/// <summary>
///     泛型仓储
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IRepository<TEntity>: IRepository<TEntity, Guid> where TEntity : class, IEntity<Guid>
{

}