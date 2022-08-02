using Findx.DependencyInjection;
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
        /// <returns></returns>
        public IUnitOfWork GetConnUnitOfWork(string dbPrimary = default, bool enableTransaction = false)
        {
            var unitOfWork = _scopedDictionary.GetConnUnitOfWork(dbPrimary ?? "default_0");
            if (unitOfWork != null)
            {
                if (enableTransaction)
                {
                    unitOfWork.EnableTransaction();
                }
                return unitOfWork;
            }

            unitOfWork = CreateConnUnitOfWork(dbPrimary);
            _scopedDictionary.SetConnUnitOfWork(dbPrimary ?? "default_0", unitOfWork);
            if (enableTransaction)
            {
                unitOfWork.EnableTransaction();
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
