using Microsoft.Extensions.Options;
using SqlSugar;
using System.Collections.Generic;

namespace Findx.SqlSugar
{
    public class SqlSugarOptions : IOptions<SqlSugarOptions>
    {
        public SqlSugarOptions Value => this;
        /// <summary>
        /// 是否启用模块
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 默认数据源
        /// </summary>
        public string Primary { set; get; }

        /// <summary>
        /// 严格匹配数据源,默认false. true未匹配到指定数据源时抛异常,false使用默认数据源
        /// </summary>
        public bool Strict { set; get; } = false;

        /// <summary>
        /// 数据源列表
        /// </summary>
        public Dictionary<string, ConnectionConfig> DataSource { set; get; } = new Dictionary<string, ConnectionConfig>();
        /// <summary>
        /// 是否打印SQL日志调试
        /// </summary>
        public bool PrintSQL { set; get; }

        /// <summary>
        /// 是否开启慢SQL记录
        /// </summary>
        public bool OutageDetection { set; get; }

        /// <summary>
        /// 慢SQL记录标准2秒
        /// </summary>
        public int OutageDetectionInterval { set; get; } = 2;

        /// <summary>
        /// 是否逻辑删除
        /// </summary>
        public bool SoftDeletable { set; get; }
        /// <summary>
        /// 逻辑删除字段
        /// </summary>
        public string SoftDeletableField { set; get; }
        /// <summary>
        /// 逻辑删除时间字段
        /// </summary>
        public string SoftDeletableTimeField { set; get; }
        /// <summary>
        /// 逻辑已删除值(默认为 1)
        /// </summary>
        public int SoftDeletableValue { set; get; } = 1;
        /// <summary>
        /// 逻辑未删除值(默认为 0)
        /// </summary>
        public int SoftNotDeletableValue { set; get; } = 0;
    }
}
