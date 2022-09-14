using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Findx.Data
{
    /// <summary>
    /// 工作单元
    /// </summary>
    public interface IUnitOfWork: IDisposable
    {
        /// <summary>
        /// 服务提供起
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// 获取 是否已提交
        /// </summary>
        bool HasCommitted { get; }
        
        /// <summary>
        /// 启用事务，事务代码写在 UnitOfWork.EnableTransaction() 与 UnitOfWork.Commit() 之间
        /// </summary>
        void EnableTransaction();

        /// <summary>
        /// 获取 是否启用事务
        /// </summary>
        bool IsEnabledTransaction { get; }
        
        /// <summary>
		/// 数据库连接
		/// </summary>
		DbConnection Connection { get; }

        /// <summary>
        /// 工作单元事务
        /// </summary>
        DbTransaction Transaction { get; set; }

        /// <summary>
        /// 开启事务
        /// </summary>
        void BeginOrUseTransaction();

        /// <summary>
        /// 提交
        /// </summary>
        void Commit();

        /// <summary>
        /// 回滚
        /// </summary>
        void Rollback();

        /// <summary>
        /// 对数据库连接开启事务
        /// </summary>
        /// <param name="cancellationToken">异步取消标记</param>
        /// <returns></returns>
        Task BeginOrUseTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步提交当前上下文的事务更改
        /// </summary>
        /// <returns></returns>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 异步回滚所有事务
        /// </summary>
        /// <returns></returns>
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }
}
