using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Data
{
    /// <summary>
    /// 泛型仓储
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepository<TEntity> where TEntity : class, new()
    {
        /// <summary>
        /// 获取工作单元
        /// </summary>
        IUnitOfWork GetUnitOfWork();

        #region 插入
        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        int Insert(TEntity entity);
        /// <summary>
        /// 异步插入
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        int Insert(List<TEntity> entities);
        /// <summary>
        /// 异步批量插入
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> InsertAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
        #endregion

        #region 删除
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        int Delete(object key);
        /// <summary>
        /// 异步删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(object key, CancellationToken cancellationToken = default);
        /// <summary>
        /// 根据条件删除
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        int Delete(Expression<Func<TEntity, bool>> whereExpression = null);
        /// <summary>
        /// 异步根据条件删除
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default);
        #endregion

        #region 更新
        /// <summary>
        /// 更新实体信息
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ignoreNullColumns">过滤为NULL字段</param>
        /// <returns></returns>
        int Update(TEntity entity, bool ignoreNullColumns = false);
        /// <summary>
        /// 更新实体信息
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ignoreNullColumns">过滤为NULL字段</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(TEntity entity, bool ignoreNullColumns = false, CancellationToken cancellationToken = default);
        /// <summary>
        /// 更新实体信息
        /// </summary>
        /// <param name="entitys"></param>
        /// <returns></returns>
        int Update(List<TEntity> entitys);
        /// <summary>
        /// 更新实体信息
        /// </summary>
        /// <param name="entitys"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(List<TEntity> entitys, CancellationToken cancellationToken = default);
        /// <summary>
        /// 实体更新
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="whereExpression"></param>
        /// <param name="updateColumns"></param>
        /// <param name="ignoreColumns"></param>
        /// <returns></returns>
        int Update(TEntity entity, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null);
        /// <summary>
        /// 异步实体更新
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="whereExpression"></param>
        /// <param name="updateColumns"></param>
        /// <param name="ignoreColumns"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, object>> updateColumns = null, Expression<Func<TEntity, object>> ignoreColumns = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// 更新指定字段
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        int UpdateColumns(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression);
        /// <summary>
        /// 更新指定字段
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        int UpdateColumns(List<Expression<Func<TEntity, bool>>> columns, Expression<Func<TEntity, bool>> whereExpression);
        /// <summary>
        /// 更新指定字段
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="whereExpression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> UpdateColumnsAsync(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default);
        /// <summary>
        /// 更新指定字段
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="whereExpression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> UpdateColumnsAsync(List<Expression<Func<TEntity, bool>>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default);
        #endregion

        #region 查询
        /// <summary>
        /// 根据主键查询实体
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TEntity Get(object key);
        /// <summary>
        /// 根据主键查询实体-异步
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TEntity> GetAsync(object key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据条件查询第一条数据
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        TEntity First(Expression<Func<TEntity, bool>> whereExpression = null);
        /// <summary>
        /// 根据条件查询第一条数据
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询列表数据
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression = null);
        /// <summary>
        /// 查询列表数据
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// 查询列表数据并返回自定义参数
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="whereExpression"></param>
        /// <param name="selectByExpression"></param>
        /// <returns></returns>
        List<TObject> Select<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null);
        /// <summary>
        /// 查询列表数据并返回自定义参数
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="whereExpression"></param>
        /// <param name="selectByExpression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<TObject>> SelectAsync<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询指定条数列表数据
        /// </summary>
        /// <param name="topSize"></param>
        /// <param name="whereExpression"></param>
        /// <param name="orderByExpression"></param>
        /// <returns></returns>
        List<TEntity> Top(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null);
        /// <summary>
        /// 查询指定条数列表数据
        /// </summary>
        /// <param name="topSize"></param>
        /// <param name="whereExpression"></param>
        /// <param name="orderByExpression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<TEntity>> TopAsync(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// 查询指定条数自定义参数列表数据
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="topSize"></param>
        /// <param name="whereExpression"></param>
        /// <param name="orderByExpression"></param>
        /// <param name="selectByExpression"></param>
        /// <returns></returns>
        List<TObject> Top<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null);
        /// <summary>
        /// 查询指定条数自定义参数列表数据
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="topSize"></param>
        /// <param name="whereExpression"></param>
        /// <param name="orderByExpression"></param>
        /// <param name="selectByExpression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<TObject>> TopAsync<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default);

        #endregion

        #region 分页查询
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="whereExpression"></param>
        /// <param name="orderByExpression"></param>
        /// <returns></returns>
        PageResult<List<TEntity>> Paged(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null);
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="whereExpression"></param>
        /// <param name="orderByExpression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<PageResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, CancellationToken cancellationToken = default);
        /// <summary>
        /// 分页查询并返回指定参数
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="whereExpression"></param>
        /// <param name="orderByExpression"></param>
        /// <param name="selectByExpression"></param>
        /// <returns></returns>
        PageResult<List<TObject>> Paged<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null);
        /// <summary>
        /// 分页查询并返回指定参数
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="whereExpression"></param>
        /// <param name="orderByExpression"></param>
        /// <param name="selectByExpression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<PageResult<List<TObject>>> PagedAsync<TObject>(int pageNumber, int pageSize, Expression<Func<TEntity, bool>> whereExpression = null, MultiOrderBy<TEntity> orderByExpression = null, Expression<Func<TEntity, TObject>> selectByExpression = null, CancellationToken cancellationToken = default);
        #endregion

        #region 函数查询
        /// <summary>
        /// 查询指定条件记录总数
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        int Count(Expression<Func<TEntity, bool>> whereExpression = null);
        /// <summary>
        /// 查询指定条件记录总数
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 判断指定条件数据是否存在
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <returns></returns>
        bool Exist(Expression<Func<TEntity, bool>> whereExpression = null);
        /// <summary>
        /// 判断指定条件数据是否存在
        /// </summary>
        /// <param name="whereExpression"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default);
        #endregion

        #region 库表信息查询
        /// <summary>
        /// 获取数据库表名
        /// </summary>
        /// <returns></returns>
        string GetDbTableName();
        /// <summary>
        /// 获取数据库表字段名集合
        /// </summary>
        /// <returns></returns>
        List<string> GetDbColumnName();
        /// <summary>
        /// 切换表规则
        /// </summary>
        /// <param name="tableRule"></param>
        IRepository<TEntity> AsTable(Func<string, string> tableRule);
        #endregion
    }
}
