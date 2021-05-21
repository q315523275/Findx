using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Data
{
    public interface IRepository<TEntity> where TEntity : class, new()
    {
        /// <summary>
        /// 获取工作单元
        /// </summary>
        IUnitOfWork GetUnitOfWork();

        int Insert(TEntity entity);
        Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
        int Insert(IEnumerable<TEntity> entities);
        Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);


        int Delete(dynamic key);
        Task<int> DeleteAsync(dynamic key, CancellationToken cancellationToken = default);
        int Delete(Expression<Func<TEntity, bool>> whereExpression);
        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default);


        int Update(TEntity entity, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null);
        Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, CancellationToken cancellationToken = default);
        int Update(TEntity entity, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null);
        Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, CancellationToken cancellationToken = default);



        TEntity Get(dynamic key);
        Task<TEntity> GetAsync(dynamic key, CancellationToken cancellationToken = default);

        TEntity First(Expression<Func<TEntity, bool>> whereExpression);
        Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default);

        List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression);
        Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default);

        PagedResult<List<TEntity>> Paged(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false);
        Task<PagedResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, object>> orderByExpression = null, bool ascending = false, CancellationToken cancellationToken = default);


        int Count(Expression<Func<TEntity, bool>> whereExpression);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default);

        bool Exist(Expression<Func<TEntity, bool>> whereExpression);
        Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default);
    }
}
