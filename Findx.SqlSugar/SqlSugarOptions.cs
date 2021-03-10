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
        public List<ConnectionConfig> DataSource { set; get; }
        /// <summary>
        /// 是否进行Debug调试
        /// </summary>
        public bool Debug { set; get; }
    }
}
