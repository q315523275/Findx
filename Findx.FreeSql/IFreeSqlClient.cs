namespace Findx.FreeSql
{
    public interface IFreeSqlClient
    {
        IFreeSql Acquire(string primary);
        bool Add(string primary, IFreeSql freeSql);
    }
}
