using System.Collections.Generic;

namespace Findx.FreeSql
{
    public interface IFreeSqlClient
    {
        ICollection<IFreeSql> All();
        IFreeSql Get(string primary = null);
        bool Add(string primary, IFreeSql freeSql);
    }
}
