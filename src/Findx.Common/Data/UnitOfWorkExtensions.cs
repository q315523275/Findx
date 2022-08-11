namespace Findx.Data
{
    /// <summary>
    /// 工作单元扩展
    /// </summary>
    public static class UnitOfWorkExtensions
    {
        /// <summary>
        /// 获取工作单元仓储
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="uow"></param>
        /// <returns></returns>
        public static IRepository<TEntity> GetRepository<TEntity>(this IUnitOfWork uow) where TEntity : class, IEntity, new()
        {
            var repo = uow.ServiceProvider.GetRequiredService<IRepository<TEntity>>();
            repo.UnitOfWork = uow;
            return repo;
        }
    }
}

