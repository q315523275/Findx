using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Extensions;

namespace Findx.Data
{
    /// <summary>
    /// 工作单元管理类基类
    /// </summary>
    public abstract class UnitOfWorkManagerBase : IUnitOfWorkManager
    {
        private readonly ScopedDictionary _scopedDictionary;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="scopedDictionary"></param>
        protected UnitOfWorkManagerBase(ScopedDictionary scopedDictionary)
        {
            _scopedDictionary = scopedDictionary;
        }

        /// <summary>
        /// 获取指定库工作单元
        /// </summary>
        /// <param name="dbPrimary"></param>
        /// <param name="enableTransaction">是否启用事务</param>
        /// <param name="beginTransaction">是否开启事物</param>
        /// <returns></returns>
        public IUnitOfWork GetConnUnitOfWork(bool enableTransaction = false, bool beginTransaction = false, string dbPrimary = default)
        {
            var unitOfWork = _scopedDictionary.GetConnUnitOfWork(dbPrimary ?? "null");
            if (unitOfWork != null)
            {
                if (enableTransaction)
                {
                    unitOfWork.EnableTransaction();
                }

                if (beginTransaction)
                {
                    unitOfWork.BeginOrUseTransaction();
                }
                return unitOfWork;
            }

            unitOfWork = CreateConnUnitOfWork(dbPrimary);
            _scopedDictionary.SetConnUnitOfWork(dbPrimary ?? "null", unitOfWork);
            if (enableTransaction)
            {
                unitOfWork.EnableTransaction();
            }
            if (beginTransaction)
            {
                unitOfWork.BeginOrUseTransaction();
            }
            return unitOfWork;
        }

        /// <summary>
        /// 获取指定库工作单元
        /// </summary>
        /// <param name="dbPrimary"></param>
        /// <param name="enableTransaction">是否启用事务</param>
        /// <param name="beginTransaction">是否开启事物</param>
        /// <returns></returns>
        public async Task<IUnitOfWork> GetConnUnitOfWorkAsync(bool enableTransaction = false, bool beginTransaction = false, string dbPrimary = default, CancellationToken cancellationToken = default)
        {
            var unitOfWork = _scopedDictionary.GetConnUnitOfWork(dbPrimary ?? "null");
            if (unitOfWork != null)
            {
                if (enableTransaction)
                {
                    unitOfWork.EnableTransaction();
                }

                if (beginTransaction)
                {
                    await unitOfWork.BeginOrUseTransactionAsync(cancellationToken);
                }
                return unitOfWork;
            }

            unitOfWork = CreateConnUnitOfWork(dbPrimary);
            _scopedDictionary.SetConnUnitOfWork(dbPrimary ?? "null", unitOfWork);
            if (enableTransaction)
            {
                unitOfWork.EnableTransaction();
            }
            if (beginTransaction)
            {
                await unitOfWork.BeginOrUseTransactionAsync(cancellationToken);
            }
            return unitOfWork;
        }

        /// <summary>
        /// 根据实体获取工作单元
        /// </summary>
        /// <param name="enableTransaction">是否启用事务</param>
        /// <param name="beginTransaction">是否启用事务</param>
        /// <returns></returns>
        public IUnitOfWork GetEntityUnitOfWork<TEntity>(bool enableTransaction = false, bool beginTransaction = false)
        {
            var entityType = typeof(TEntity);
            var extensionAttribute = SingletonDictionary<Type, EntityExtensionAttribute>.Instance.GetOrAdd(entityType, () => entityType.GetAttribute<EntityExtensionAttribute>());
            var dataSource = extensionAttribute?.DataSource;

            var unitOfWork = _scopedDictionary.GetEntityUnitOfWork(entityType);
            if (unitOfWork != null)
            {
                if (enableTransaction)
                {
                    unitOfWork.EnableTransaction();
                }
                if (beginTransaction)
                {
                    unitOfWork.BeginOrUseTransaction();
                }
                return unitOfWork;
            }

            unitOfWork = CreateConnUnitOfWork(dataSource);
            _scopedDictionary.SetEntityUnitOfWork(entityType, unitOfWork);
            if (enableTransaction)
            {
                unitOfWork.EnableTransaction();
            }
            if (beginTransaction)
            {
                unitOfWork.BeginOrUseTransaction();
            }

            return unitOfWork;
        }

        /// <summary>
        /// 根据实体获取工作单元
        /// </summary>
        /// <param name="enableTransaction">是否启用事务</param>
        /// <param name="beginTransaction">是否启用事务</param>
        /// <returns></returns>
        public async Task<IUnitOfWork> GetEntityUnitOfWorkAsync<TEntity>(bool enableTransaction = false, bool beginTransaction = false, CancellationToken cancellationToken = default)
        {
            var entityType = typeof(TEntity);
            var extensionAttribute = SingletonDictionary<Type, EntityExtensionAttribute>.Instance.GetOrAdd(entityType, () => entityType.GetAttribute<EntityExtensionAttribute>());
            var dataSource = extensionAttribute?.DataSource;

            var unitOfWork = _scopedDictionary.GetEntityUnitOfWork(entityType);
            if (unitOfWork != null)
            {
                if (enableTransaction)
                {
                    unitOfWork.EnableTransaction();
                }
                if (beginTransaction)
                {
                    await unitOfWork.BeginOrUseTransactionAsync(cancellationToken);
                }
                return unitOfWork;
            }

            unitOfWork = CreateConnUnitOfWork(dataSource);
            _scopedDictionary.SetEntityUnitOfWork(entityType, unitOfWork);
            if (enableTransaction)
            {
                unitOfWork.EnableTransaction();
            }
            if (beginTransaction)
            {
                await unitOfWork.BeginOrUseTransactionAsync(cancellationToken);
            }

            return unitOfWork;
        }

        /// <summary>
        /// 获取指定DB连接工作单元
        /// </summary>
        /// <param name="dbPrimary"></param>
        /// <returns></returns>
        protected abstract IUnitOfWork CreateConnUnitOfWork(string dbPrimary);
    }
}
