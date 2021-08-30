using FreeSql;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Findx.FreeSql
{
    public class FreeSqlOptions : IOptions<FreeSqlOptions>
    {
        public FreeSqlOptions Value => this;
        /// <summary>
        /// 是否启用模块
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// 默认数据源
        /// </summary>
        public string Primary { set; get; }
        /// <summary>
        /// 数据源列表
        /// </summary>
        public Dictionary<string, FreeSqlConnectionConfig> DataSource { set; get; }
        /// <summary>
        /// 是否进行Debug调试
        /// </summary>
        public bool Debug { set; get; }
        /// <summary>
        /// 是否合并事务
        /// </summary>
        public bool MergeTrans { set; get; }
    }
    /// <summary>
    /// FreeSql数据库连接配置
    /// </summary>
    public class FreeSqlConnectionConfig
    {
        public string ConnectionString { set; get; }
        public DataType DataType { set; get; }
    }
}
