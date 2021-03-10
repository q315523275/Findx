using System.Data;

namespace Findx.Data
{
    /// <summary>
    /// 工作单元
    /// </summary>
    public interface IUnitOfWork
    {
        void BeginTran();
        void BeginTran(IsolationLevel il);
        void CommitTran();
        void RollbackTran();
    }
    /// <summary>
    /// 泛型工作单元
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IUnitOfWork<T> : IUnitOfWork
    {
        T GetInstance();
    }
}
