using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Findx.Data;

/// <summary>
/// 包含泛型Id的泛型仓储
/// </summary>
public interface IRepository<TEntity, in TKey> where TEntity : class, IEntity<TKey>
{
    /// <summary>
    ///     获取或设置：工作单元
    /// </summary>
    IUnitOfWork UnitOfWork { set; get; }

    #region 插入

    /// <summary>
    ///     插入
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>影响行数</returns>
    int Insert(TEntity entity);

    /// <summary>
    ///     异步插入
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数</returns>
    Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///     批量插入
    /// </summary>
    /// <param name="entities"></param>
    /// <returns>影响行数</returns>
    int Insert(IEnumerable<TEntity> entities);

    /// <summary>
    ///     异步批量插入
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数</returns>
    Task<int> InsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    #endregion

    #region 删除

    /// <summary>
    ///     删除
    /// </summary>
    /// <param name="key"></param>
    /// <returns>影响行数</returns>
    int Delete(TKey key);

    /// <summary>
    ///     异步删除
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数</returns>
    Task<int> DeleteAsync(TKey key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     根据条件删除
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns>影响行数</returns>
    int Delete(Expression<Func<TEntity, bool>> whereExpression = null);

    /// <summary>
    ///     异步根据条件删除
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数</returns>
    Task<int> DeleteAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default);

    #endregion

    #region 更新

    /// <summary>
    ///     实体更新
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="updateColumns"></param>
    /// <param name="ignoreColumns">过滤字段(优先级大于ignoreNullColumns)</param>
    /// <param name="ignoreNullColumns">过滤为NULL字段(注意与ignoreColumns优先级)</param>
    /// <returns>影响行数</returns>
    int Update(TEntity entity, Expression<Func<TEntity, object>> updateColumns = null,
        Expression<Func<TEntity, object>> ignoreColumns = null, bool ignoreNullColumns = false);

    /// <summary>
    ///     异步实体更新
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="updateColumns"></param>
    /// <param name="ignoreColumns"></param>
    /// <param name="ignoreNullColumns">过滤为NULL字段(注意与ignoreColumns优先级)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数</returns>
    Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, object>> updateColumns = null,
        Expression<Func<TEntity, object>> ignoreColumns = null, bool ignoreNullColumns = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     更新实体信息
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="updateColumns"></param>
    /// <param name="ignoreColumns"></param>
    /// <returns>影响行数</returns>
    int Update(IEnumerable<TEntity> entities, Expression<Func<TEntity, object>> updateColumns = null,
        Expression<Func<TEntity, object>> ignoreColumns = null);

    /// <summary>
    ///     更新实体信息
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="updateColumns"></param>
    /// <param name="ignoreColumns"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数</returns>
    Task<int> UpdateAsync(IEnumerable<TEntity> entities, Expression<Func<TEntity, object>> updateColumns = null,
        Expression<Func<TEntity, object>> ignoreColumns = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     更新指定字段
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="whereExpression"></param>
    /// <returns>影响行数</returns>
    int UpdateColumns(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression);

    /// <summary>
    ///     更新指定字段
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="whereExpression"></param>
    /// <returns>影响行数</returns>
    int UpdateColumns(List<Expression<Func<TEntity, bool>>> columns, Expression<Func<TEntity, bool>> whereExpression);
    
    /// <summary>
    ///     更新指定字段
    /// </summary>
    /// <param name="dict"></param>
    /// <returns>影响行数</returns>
    int UpdateColumns(Dictionary<string, object> dict);

    /// <summary>
    ///     更新指定字段
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="whereExpression"></param>
    /// <returns>影响行数</returns>
    int UpdateColumns(Dictionary<string, object> dict, Expression<Func<TEntity, bool>> whereExpression);

    /// <summary>
    ///     更新指定字段
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数</returns>
    Task<int> UpdateColumnsAsync(Expression<Func<TEntity, TEntity>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default);

    /// <summary>
    ///     更新指定字段
    /// </summary>
    /// <param name="columns"></param>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数</returns>
    Task<int> UpdateColumnsAsync(List<Expression<Func<TEntity, bool>>> columns, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default);

    /// <summary>
    ///     更新指定字段
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数</returns>
    Task<int> UpdateColumnsAsync(Dictionary<string, object> dict, CancellationToken cancellationToken = default);

    /// <summary>
    ///     更新指定字段
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>影响行数</returns>
    Task<int> UpdateColumnsAsync(Dictionary<string, object> dict, Expression<Func<TEntity, bool>> whereExpression, CancellationToken cancellationToken = default);

    /// <summary>
    ///     附加实体到状态管理，可用于不查询就更新或删除
    /// </summary>
    /// <param name="entity"></param>
    void Attach(TEntity entity);

    /// <summary>
    ///     更新或者新增数据,当更新时只更新变更值
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    int Save(TEntity entity);

    /// <summary>
    ///      更新或者新增数据,当更新时只更新变更值
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> SaveAsync(TEntity entity, CancellationToken cancellationToken = default);
    #endregion

    #region 查询

    /// <summary>
    ///     根据主键查询实体
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    TEntity Get(TKey key);

    /// <summary>
    ///     根据主键查询实体-异步
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TEntity> GetAsync(TKey key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     根据条件查询第一条数据
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    TEntity First(Expression<Func<TEntity, bool>> whereExpression = null);

    /// <summary>
    ///     根据条件查询第一条数据
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> whereExpression = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     查询列表数据
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="orderParameters"></param>
    /// <returns></returns>
    List<TEntity> Select(Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null);

    /// <summary>
    ///     查询列表数据
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="orderParameters"></param>
    /// <returns></returns>
    Task<List<TEntity>> SelectAsync(Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     查询列表数据并返回自定义参数
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="selectExpression"></param>
    /// <param name="orderParameters"></param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    List<TObject> Select<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null);

    /// <summary>
    ///     查询列表数据并返回自定义参数
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="selectExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="orderParameters"></param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    Task<List<TObject>> SelectAsync<TObject>(Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     查询指定条数列表数据
    /// </summary>
    /// <param name="topSize"></param>
    /// <param name="whereExpression"></param>
    /// <param name="orderParameters"></param>
    /// <returns></returns>
    List<TEntity> Top(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null);

    /// <summary>
    ///     查询指定条数列表数据
    /// </summary>
    /// <param name="topSize"></param>
    /// <param name="whereExpression"></param>
    /// <param name="orderParameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<List<TEntity>> TopAsync(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     查询指定条数自定义参数列表数据
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <param name="topSize"></param>
    /// <param name="whereExpression"></param>
    /// <param name="orderParameters"></param>
    /// <param name="selectExpression"></param>
    /// <returns></returns>
    List<TObject> Top<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null);

    /// <summary>
    ///     查询指定条数自定义参数列表数据
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <param name="topSize"></param>
    /// <param name="whereExpression"></param>
    /// <param name="selectExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="orderParameters"></param>
    /// <returns></returns>
    Task<List<TObject>> TopAsync<TObject>(int topSize, Expression<Func<TEntity, bool>> whereExpression = null, Expression<Func<TEntity, TObject>> selectExpression = null, IEnumerable<OrderByParameter<TEntity>> orderParameters = null, CancellationToken cancellationToken = default);

    #endregion

    #region 分页查询

    /// <summary>
    ///     分页查询
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="whereExpression"></param>
    /// <param name="orderParameters"></param>
    /// <param name="returnCount">返回总数</param>
    /// <returns></returns>
    PageResult<List<TEntity>> Paged(int pageNumber, int pageSize,
        Expression<Func<TEntity, bool>> whereExpression = null,
        IEnumerable<OrderByParameter<TEntity>> orderParameters = null,
        bool returnCount = true);

    /// <summary>
    ///     分页查询
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="whereExpression"></param>
    /// <param name="orderParameters"></param>
    /// <param name="returnCount">返回总数</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PageResult<List<TEntity>>> PagedAsync(int pageNumber, int pageSize,
        Expression<Func<TEntity, bool>> whereExpression = null,
        IEnumerable<OrderByParameter<TEntity>> orderParameters = null, 
        bool returnCount = true, 
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     分页查询并返回指定参数
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="whereExpression"></param>
    /// <param name="orderParameters"></param>
    /// <param name="selectExpression"></param>
    /// <param name="returnCount">返回总数</param>
    /// <returns></returns>
    PageResult<List<TObject>> Paged<TObject>(int pageNumber, int pageSize,
        Expression<Func<TEntity, bool>> whereExpression = null,
        Expression<Func<TEntity, TObject>> selectExpression = null,
        IEnumerable<OrderByParameter<TEntity>> orderParameters = null,
        bool returnCount = true);

    /// <summary>
    ///     分页查询并返回指定参数
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="whereExpression"></param>
    /// <param name="orderParameters"></param>
    /// <param name="selectExpression"></param>
    /// <param name="returnCount"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PageResult<List<TObject>>> PagedAsync<TObject>(int pageNumber, int pageSize,
        Expression<Func<TEntity, bool>> whereExpression = null,
        Expression<Func<TEntity, TObject>> selectExpression = null,
        IEnumerable<OrderByParameter<TEntity>> orderParameters = null, 
        bool returnCount = true, 
        CancellationToken cancellationToken = default);

    #endregion

    #region 函数查询

    /// <summary>
    ///     查询指定条件记录总数
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    int Count(Expression<Func<TEntity, bool>> whereExpression = null);

    /// <summary>
    ///     查询指定条件记录总数
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>> whereExpression = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     判断指定条件数据是否存在
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <returns></returns>
    bool Exist(Expression<Func<TEntity, bool>> whereExpression = null);

    /// <summary>
    ///     判断指定条件数据是否存在
    /// </summary>
    /// <param name="whereExpression"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> ExistAsync(Expression<Func<TEntity, bool>> whereExpression = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region 库表信息查询

    /// <summary>
    ///     获取数据源名称
    /// </summary>
    /// <returns></returns>
    string GetDataSource();
    
    /// <summary>
    ///     获取数据库表名
    /// </summary>
    /// <returns></returns>
    string GetDbTableName();

    /// <summary>
    ///     获取数据库表字段名集合
    /// </summary>
    /// <returns></returns>
    List<string> GetDbColumnName();

    /// <summary>
    ///     获取数据库类型
    /// </summary>
    /// <returns></returns>
    DatabaseType GetDbType();

    /// <summary>
    ///     切换表规则
    /// </summary>
    /// <param name="tableRule"></param>
    IRepository<TEntity, TKey> AsTable(Func<string, string> tableRule);

    /// <summary>
    ///     比较实体，计算出值发生变化的属性，以及属性变化的前后值
    /// </summary>
    /// <param name="newData"></param>
    /// <returns></returns>
    Dictionary<string, object[]> CompareState(TEntity newData);
    
    /// <summary>
    ///     比较实体，计算出值发生变化的属性，以及属性变化的前后值
    /// </summary>
    /// <param name="newData"></param>
    /// <param name="oldData"></param>
    /// <returns></returns>
    Dictionary<string, object[]> CompareState(TEntity newData, TEntity oldData);
    #endregion
}