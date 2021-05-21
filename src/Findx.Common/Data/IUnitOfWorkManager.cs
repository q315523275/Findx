namespace Findx.Data
{
    public interface IUnitOfWorkManager
    {
        void BeginTran();
        void CommitTran();
        void RollbackTran();
    }
}
