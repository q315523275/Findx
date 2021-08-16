using Microsoft.Extensions.Options;
using SqlSugar;
using System.Collections.Generic;

namespace Findx.SqlSugar
{
    public class SqlSugarOptions : IOptions<SqlSugarOptions>
    {
        public SqlSugarOptions Value => this;
        /// <summary>
        /// 默认数据源
        /// </summary>
        public string Primary { set; get; }
        /// <summary>
        /// 数据源列表
        /// </summary>
        public Dictionary<string, ConnectionConfig> DataSource { set; get; } = new Dictionary<string, ConnectionConfig>();
        /// <summary>
        /// 是否进行Debug调试
        /// </summary>
        public bool Debug { set; get; }
        /// <summary>
        /// 是否合并事务
        /// </summary>
        public bool MergeTrans { set; get; }
    }
}
